"""Prepare a silent, seekable README film from exported C# presentation states.

Usage: python scripts/build-showcase.py states.json output-dir --body-font font.ttf
Vendor GSAP 3.14.2 into output-dir/assets/vendor before checking or rendering.
"""
from pathlib import Path
import argparse
import html
import json
import shutil
from fontTools.ttLib import TTFont
from fontTools.pens.svgPathPen import SVGPathPen

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("states", type=Path)
parser.add_argument("output", type=Path)
parser.add_argument("--body-font", required=True, type=Path)
args = parser.parse_args()
root = Path(__file__).resolve().parents[1]
data = json.loads(args.states.read_text())
assert data["duration"] == 23
assets = args.output / "assets"
assets.mkdir(parents=True, exist_ok=True)
shutil.copyfile(args.body_font, assets / "ShowcaseSans.ttf")
shutil.copyfile(root / "THIRD_PARTY_NOTICES.md", assets / "FONT-NOTICES.md")
font = TTFont(root / "assets/CounterHeadline.ttf")
glyphs, cmap = font.getGlyphSet(), font.getBestCmap()
font_scale = 80 / (font["hhea"].ascent - font["hhea"].descent)
colors = ["#f7f7fc", "#66fab0", "#ffcf4a", "#e0b8ff", "#66d6ff"]
fills = {0: "#2b3038", 2: "#45381f", 3: "#3b2154"}

def text(x, y, value, size=16, color=colors[0], center=False):
    anchor = ' text-anchor="middle"' if center else ""
    return f'<text x="{x}" y="{y}" fill="{color}" font-size="{size}"{anchor}>{html.escape(value)}</text>'

def width(value, size):
    return sum(font["hmtx"][cmap[ord(c)]][0] for c in value) * font_scale * size / 80

def headline(value, color, size=80, baseline=96):
    scale = font_scale * size / 80
    extra = 42 if value == "OPEN" else 0
    assert width(value, size) + extra <= 288
    x = (320 - width(value, size) - extra) / 2
    paths = []
    for char in value:
        glyph = cmap[ord(char)]
        pen = SVGPathPen(glyphs)
        glyphs[glyph].draw(pen)
        paths.append(f'<path d="{pen.getCommands()}" fill="{color}" transform="translate({x:.3f} {baseline:.3f}) scale({scale:.6f} {-scale:.6f})"/>')
        x += font["hmtx"][glyph][0] * scale
    if value == "OPEN":
        paths.append(f'<path d="M{x+13:.3f} 70l9 9l20 -23" stroke="{colors[1]}" stroke-width="5" fill="none"/>')
    return paths

def card(state):
    v, parts = state["view"], []
    ready = v["Tone"] == 1
    parts += [f'<rect x="1" y="1" width="318" height="230" rx="18" fill="#0e1112" stroke="{colors[1] if ready else "#424a4f"}" stroke-width="{2 if ready else 1}"/>',
              text(18, 29, f'floor {state["floor"]}', 18)]
    if v["IsBossVictory"]:
        lines = v["Main"].split("\n")
        size = next(s for s in (48, 40, 34) if all(width(line, s) <= 280 for line in lines))
        top = 40 + (96 - len(lines) * size) / 2
        for n, line in enumerate(lines):
            baseline = top + n * size + font["hhea"].ascent * font_scale * size / 80
            parts += headline(line, colors[0], size, baseline)
        parts += [text(145, 165, v["Caption"], 20, colors[1], True),
                  '<path d="M205 158l6 6l14 -16" stroke="#66fab0" stroke-width="3" fill="none"/>']
        if v["DestinationNotice"]:
            parts += [text(160, 209, v["DestinationNotice"], 16, colors[1], True)]
    else:
        parts += headline(v["Main"], colors[v["Tone"]])
        parts += [text(160, 126, v["Caption"], 16 if len(v["Caption"]) > 22 else 20, center=True)]
        if v["ShowFlight"]:
            b = v["Flight"]
            bw = 230 if b["TwoLines"] else 218
            parts += [f'<rect x="{160-bw/2}" y="140" width="{bw}" height="40" rx="20" fill="{fills.get(b["Tone"], fills[0])}" stroke="{colors[b["Tone"]]}" stroke-opacity=".35"/>']
            if b["TwoLines"]:
                parts += [text(160, 155, b["Status"], 16, colors[b["Tone"]], True),
                          text(160, 174, b["Detail"], 18, colors[b["Tone"]], True)]
            else:
                parts += [text(160, 167, b["Status"] + "  " + b["Detail"], 18, colors[b["Tone"]], True)]
            ny = 202
            if v["NextFlightNotice"]:
                parts += [text(160, ny, v["NextFlightNotice"], 16, colors[v["NextFlightTone"]], True)]
                ny += 20
            if v["DestinationNotice"]:
                parts += [text(160, ny, v["DestinationNotice"], 16, colors[2], True)]
    return '<svg viewBox="0 0 320 232" width="512" height="371.2" role="img" aria-label="simulated counter HUD">' + "".join(parts) + '</svg>'

chapters = [
    (0, 5, "01 / the count", "count less,<br>keep moving", "estimated kills left, updated as enemies fall", "#f7f7fc"),
    (5, 8, "02 / Flight", "ready for<br>the next floor", "blue shows Flight queued for the floor ahead", "#66d6ff"),
    (8, 12.5, "02 / Flight", "active here", "lavender means Flight is active, green means the passage is open", "#e0b8ff"),
    (12.5, 14.5, "03 / the boss", "know who's<br>waiting", "the boss name, before the pull", "#ffcf4a"),
    (14.5, 16.5, "03 / the boss", "just the fight", "the HUD hides during boss combat", "#ffcf4a"),
    (16.5, 20.5, "04 / the victory", "a moment<br>for the win", "a confirmed defeat brings their name forward", "#66fab0"),
    (20.5, 23, "04 / the victory", "onward", "then back to the passage", "#66fab0"),
]
chapter_html = []
timeline = []
for i, (start, end, label, heading, copy, color) in enumerate(chapters):
    # The shared grid holds the complete end-state layout, fades handle transitions.
    chapter_html.append(f'<section class="chapter" id="chapter-{i}"><div class="chapter-label" style="color:{color}">{label}</div><h2>{heading}</h2><p>{copy}</p></section>')
    if i == 0:
        timeline.append('tl.set("#chapter-0", {opacity:1}, 0);')
        timeline.append('tl.from("#chapter-0", {y:6,duration:0.3,ease:"power1.out"}, 0.12);')
    else:
        timeline.append(f'tl.fromTo("#chapter-{i}", {{opacity:0}}, {{opacity:1,duration:0.22,ease:"power1.out"}}, {start});')
    if i < len(chapters) - 1:
        timeline.append(f'tl.set("#chapter-{i}", {{opacity:0}}, {end + 0.22});')

state_html = []
for i, s in enumerate(data["states"]):
    if s["hidden"]:
        continue
    state_html.append(f'<div class="hud-state" id="hud-{i}">{card(s)}</div>')
    if i == 0:
        timeline.append('tl.set("#hud-0", {opacity:1}, 0);')
    elif s["view"]["IsBossVictory"]:
        timeline.append(f'tl.fromTo("#hud-{i}", {{opacity:0}}, {{opacity:1,duration:0.18,ease:"none"}}, {s["start"]});')
    elif s["start"] in (0, 5, 8, 12.5):
        timeline.append(f'tl.fromTo("#hud-{i}", {{opacity:0}}, {{opacity:1,duration:0.22,ease:"power2.out"}}, {s["start"] + (0.12 if i == 0 else 0)});')
    else:
        timeline.append(f'tl.set("#hud-{i}", {{opacity:1}}, {s["start"]});')
    hide_at = s["end"] + (0.22 if s["end"] in (5, 8, 12.5) else 0)
    timeline.append(f'tl.set("#hud-{i}", {{opacity:0}}, {hide_at});')

template = r'''<!doctype html>
<html lang="en"><head><meta charset="utf-8"><title>Riri's PotD Counter · short showcase</title>
<script src="./assets/vendor/gsap-3.14.2.min.js"></script>
<style>
@font-face{font-family:ShowcaseSans;src:url('./assets/ShowcaseSans.ttf') format('truetype');font-weight:400}
*{box-sizing:border-box;margin:0}html,body{width:1280px;height:720px;overflow:hidden;background:#16181d}
#root{width:100%;height:100%;font-family:ShowcaseSans,sans-serif;color:#f7f7fc;background:#16181d;display:flex;flex-direction:column;padding:56px 64px 44px;gap:0}
.masthead{display:flex;justify-content:space-between;align-items:flex-start;width:100%;height:100px;flex-shrink:0}
.brand{font-size:28px;line-height:36px;letter-spacing:-1px}.tagline{font-size:20px;color:#b9bdc7;line-height:32px}
.simulation{font-size:20px;line-height:28px;text-align:right;color:#b9bdc7;padding-top:5px}
.body{display:grid;grid-template-columns:1fr 512px;align-items:center;gap:44px;flex:1;min-height:0;width:100%}
.copy{display:grid;align-items:center;min-width:0}.chapter{grid-area:1/1;opacity:0;background:#16181d;min-height:270px;display:flex;flex-direction:column;gap:16px;justify-content:center}
.chapter-label{font-size:20px;line-height:28px;letter-spacing:1px}h2{font-size:52px;line-height:1.1;letter-spacing:-2px;font-weight:700}p{font-size:23px;line-height:1.45;color:#c3c7d0;max-width:470px}
.hud-stage{display:grid;place-items:center;width:512px;height:372px}.hud-state{grid-area:1/1;opacity:0;width:512px;height:372px}.hud-state svg{display:block;font-family:ShowcaseSans,sans-serif}
.footer{display:flex;align-items:center;justify-content:space-between;border-top:1px solid #383c45;padding-top:24px;font-size:20px;line-height:28px;color:#b9bdc7;width:100%;height:64px;flex-shrink:0}
.command{color:#e0b8ff}.track{height:3px;background:#e0b8ff;width:100%;transform-origin:left center;position:absolute;left:0;bottom:0}
</style></head><body>
<div id="root" data-composition-id="potd-showcase" data-start="0" data-duration="23" data-width="1280" data-height="720" data-fps="30">
<header class="masthead"><div><div class="brand">Riri's PotD Counter</div><div class="tagline">read the state, keep moving</div></div><div class="simulation"><div>simulated HUD · sample values</div><div>not an in-game recording</div></div></header>
<main class="body"><div class="copy" data-layout-allow-overlap>CHAPTERS</div><div class="hud-stage">STATES</div></main>
<footer class="footer"><span>Palace of the Dead</span><span class="command">/potd · settings &amp; preview</span></footer>
<div class="track" data-layout-ignore></div>
</div>
<script>
const tl=gsap.timeline({paused:true});
tl.from(".brand",{y:6,duration:0.35,ease:"power2.out"},0.1);
tl.from(".tagline",{y:4,duration:0.4,ease:"power1.out"},0.15);
tl.from(".footer",{y:2,duration:0.4,ease:"sine.out"},0.1);
tl.fromTo(".track",{scaleX:0},{scaleX:1,duration:23,ease:"none"},0);
TIMELINE
window.__timelines=window.__timelines||{};
window.__timelines["potd-showcase"]=tl;
</script></body></html>'''
template = template.replace("CHAPTERS", "".join(chapter_html)).replace("STATES", "".join(state_html)).replace("TIMELINE", "\n".join(timeline))
(args.output / "index.html").write_text(template)
(args.output / "DESIGN.md").write_text("""# counter showcase

23 seconds, silent, 1280 × 720, with a small continuous simulation label
use the shipped 320 × 232 HUD proportions and actual C# presentation states
supporting text approximates the host font, headline paths come from CounterHeadline.ttf

## palette and typography

charcoal #16181d, card #0e1112, white #f7f7fc, lavender #e0b8ff
green #66fab0 means ready or defeated, blue #66d6ff means next-floor Flight
amber #ffcf4a marks the boss chapter, matching the product's semantic palette
DejaVu Sans supports the surrounding editorial type

## motion

quiet 220 ms chapter fades, instant count updates, no invented HUD interaction
hide during boss combat, use the real 180 ms victory fade and four-second hold
no game footage, no sound, no other deep dungeons, no combat advice
keep one readable card and one short explanation in a fixed two-column layout
""")
print("prepared a 23-second showcase using 12 real presentation snapshots")
