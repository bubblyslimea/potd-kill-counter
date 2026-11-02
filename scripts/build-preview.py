"""Regenerate labelled studies from the same C# examples used by /potd preview.

Requires fonttools only for converting the embedded headline font into SVG outlines.
"""
from pathlib import Path
import base64
import html
import json
import sys
from fontTools.ttLib import TTFont
from fontTools.pens.svgPathPen import SVGPathPen

root = Path(__file__).resolve().parents[1]
scenes = json.loads(Path(sys.argv[1]).read_text())
font = TTFont(root / "assets/CounterHeadline.ttf")
cmap = font.getBestCmap()
glyphs = font.getGlyphSet()
font_scale = 80 / (font["hhea"].ascent - font["hhea"].descent)
font_css_size = font_scale * font["head"].unitsPerEm
colors = ["#f7f7fc", "#66fab0", "#ffcf4a", "#e0b8ff", "#66d6ff"]
fills = {0: "#2b3038", 2: "#45381f", 3: "#3b2154"}

template = """<!doctype html>
<html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>Riri's PotD Counter · HUD preview</title>
<style>
@font-face{font-family:CounterHeadline;src:url(data:font/ttf;base64,FONT_BASE64) format('truetype');font-weight:700}
:root{color-scheme:dark;font-family:system-ui,sans-serif;background:#16181d;color:#f7f7fc}
*{box-sizing:border-box}body{margin:0;padding:36px 16px}main{max-width:860px;margin:auto}
.eyebrow{color:#e0b8ff;font-size:13px}h1{font-size:34px;letter-spacing:-.04em;margin:12px 0}
p{color:#b9bdc7;line-height:1.6;font-size:14px}.stage{display:grid;place-items:center;min-height:310px;margin:24px 0;background:#22262c;border:1px solid #3c4149;border-radius:20px}
.hud{position:relative;width:320px;height:232px;background:#0e1112;border:1px solid #424a4f;border-radius:18px;flex:none}
.hud.ready{border:2px solid #66fab0}.floor{position:absolute;left:17px;top:11px;font-size:18px}
.answer{position:absolute;top:31px;left:0;width:100%;height:80px;display:flex;justify-content:center;align-items:center}
.headline{font:700 FONT_SIZEpx/80px CounterHeadline, sans-serif;white-space:nowrap}
.check{width:29px;height:29px;margin-left:13px;stroke:#66fab0;stroke-width:5;fill:none;overflow:visible}
.caption{position:absolute;top:107px;width:100%;text-align:center;font-size:20px;line-height:24px}
.badge{position:absolute;top:139px;left:50%;transform:translateX(-50%);height:40px;min-width:198px;padding:0 18px;border:1px solid #646a74;border-radius:20px;display:flex;align-items:center;justify-content:center;font-size:18px;white-space:nowrap;background:#2b3038;line-height:22px}
.badge.first{width:230px;flex-direction:column;gap:0;padding:0;line-height:18px}.badge.first .status{font-size:16px;line-height:17px}.badge .detail{margin-left:10px}.badge.first .detail{margin-left:0}
.notice{position:absolute;top:186px;width:100%;font-size:16px;text-align:center;line-height:20px;white-space:nowrap}
.controls{display:flex;flex-wrap:wrap;gap:8px}button{font:inherit;font-size:13px;color:#eee;background:#242832;border:1px solid #484e5a;padding:8px 12px;border-radius:8px;cursor:pointer}button[aria-pressed=true]{border-color:#e0b8ff;background:#3b2154}button:focus-visible,summary:focus-visible{outline:2px solid #66d6ff;outline-offset:4px}
details{margin-top:24px}summary{cursor:pointer;color:#e0b8ff}.foot{font-size:12px}[hidden]{display:none!important}
@media(max-width:360px){body{padding:24px 8px}.stage{margin-left:-8px;margin-right:-8px}}
</style>
<main><div class="eyebrow">Riri's PotD Counter · 0.1.4</div><h1>read the state, keep moving</h1>
<p>the count up front, Flight where you expect it, details in /potd when you need them</p>
<p class="foot">layout study · simulated values · not an in-game capture</p>
<section class="stage" aria-label="Simulated HUD"><div class="hud" id="hud">
<div class="floor" id="floor"></div>
<div class="answer"><span class="headline" id="headline"></span><svg class="check" id="check" viewBox="0 0 29 29" aria-hidden="true"><path d="M0 15L9 24L29 1"/></svg></div>
<div class="caption" id="caption"></div>
<div class="badge" id="badge"><span class="status" id="status"></span><span class="detail" id="detail"></span></div>
<div class="notice" id="next"></div><div class="notice" id="destination"></div>
</div><p id="combat-hidden" hidden>boss combat · HUD hidden</p></section><nav class="controls" id="controls" aria-label="Example states"></nav>
<details><summary>run details · these live in /potd settings</summary><p id="observed"></p><p id="range"></p><p id="note"></p></details>
<p class="foot">these examples come from the plugin's actual presentation model<br>the headline uses the shipped font, supporting text approximates Dalamud's font</p></main>
<script>
const scenes=SCENES_JSON,colors=COLORS_JSON,fills=FILLS_JSON;
const $=id=>document.getElementById(id);
function show(s){
 const v=s.view,b=v.Flight;
 $('hud').hidden=s.hidden;$('combat-hidden').hidden=!s.hidden;
 $('floor').textContent='floor '+s.floor;
 $('headline').textContent=v.Main;$('headline').style.color=colors[v.Tone];
 $('check').toggleAttribute('hidden',v.Main!=='OPEN');$('hud').classList.toggle('ready',v.Tone===1);
 $('caption').textContent=v.Caption;$('caption').style.fontSize=v.Caption.length>22?'16px':'20px';
 $('badge').hidden=!v.ShowFlight;$('badge').classList.toggle('first',b.TwoLines);
 $('badge').style.color=colors[b.Tone];$('badge').style.background=fills[b.Tone]||fills[0];
 $('status').textContent=b.Status;$('detail').textContent=b.Detail;
 $('next').textContent=v.NextFlightNotice||'';$('next').style.color=colors[v.NextFlightTone];
 $('destination').textContent=v.DestinationNotice||'';$('destination').style.color=colors[2];
 $('destination').style.top=v.NextFlightNotice?'206px':'186px';
 $('observed').textContent=v.Observed||'';$('range').textContent=v.Range||'';$('note').textContent=v.Note;
 document.querySelectorAll('button').forEach(b=>b.setAttribute('aria-pressed',String(b.dataset.id===s.id)));
}
for(const s of scenes){const b=document.createElement('button');b.textContent=s.label;b.dataset.id=s.id;b.addEventListener('click',()=>show(s));$('controls').appendChild(b)}
show(scenes[0]);
</script></html>
"""
for key, value in {
    "FONT_BASE64": base64.b64encode((root / "assets/CounterHeadline.ttf").read_bytes()).decode(),
    "FONT_SIZE": str(font_css_size),
    "SCENES_JSON": json.dumps(scenes, ensure_ascii=False),
    "COLORS_JSON": json.dumps(colors),
    "FILLS_JSON": json.dumps(fills),
}.items():
    template = template.replace(key, value)
(root / "PREVIEW.html").write_text(template, encoding="utf-8")

def text(x, y, value, size=16, color="#b9bdc7", center=False):
    align = ' text-anchor="middle"' if center else ""
    return f'<text x="{x}" y="{y}" fill="{color}" font-size="{size}"{align}>{html.escape(value)}</text>'

def headline(value, color):
    # ImGui advances glyphs without font kerning; match that in the outlined preview.
    advances = [font["hmtx"][cmap[ord(c)]][0] for c in value]
    width = sum(advances) * font_scale
    extra = 42 if value == "OPEN" else 0
    if width + extra > 288:
        raise ValueError("headline exceeds HUD safe width: " + value)
    x = (320 - width - extra) / 2
    parts = []
    for ch, advance in zip(value, advances):
        pen = SVGPathPen(glyphs)
        glyphs[cmap[ord(ch)]].draw(pen)
        parts.append(f'<path d="{pen.getCommands()}" fill="{color}" transform="translate({x:.3f} 96) scale({font_scale:.6f} {-font_scale:.6f})"/>')
        x += advance * font_scale
    if value == "OPEN":
        parts.append(f'<path d="M{x+13:.3f} 70l9 9l20 -23" stroke="{colors[1]}" stroke-width="5" fill="none"/>')
    return parts

# Exercise every fixture's real headline, including states outside the six-card overview.
for s in scenes:
    headline(s["view"]["Main"], colors[s["view"]["Tone"]])

parts = ['<svg xmlns="http://www.w3.org/2000/svg" width="1080" height="720" viewBox="0 0 1080 720">',
         '<rect width="1080" height="720" rx="18" fill="#16181d"/>',
         '<g font-family="DejaVu Sans,Arial,sans-serif">',
         text(36, 49, "Riri's PotD Counter", 28, "#f7f7fc"),
         text(36, 80, "read the state, keep moving", 17, "#e0b8ff")]
for index, scene_id in enumerate(["first", "flight", "normal", "both", "boss-before", "open"]):
    s = next(s for s in scenes if s["id"] == scene_id)
    v, x, y = s["view"], 36 + (index % 3) * 344, 128 + (index // 3) * 280
    b = v["Flight"]
    parts += [text(x, y - 14, s["label"], 15),
              f'<g transform="translate({x} {y})">',
              f'<rect x="1" y="1" width="318" height="230" rx="18" fill="#0e1112" stroke="{colors[1] if v["Tone"] == 1 else "#424a4f"}" stroke-width="{2 if v["Tone"] == 1 else 1}"/>',
              text(18, 29, f'floor {s["floor"]}', 18, colors[0])]
    parts += headline(v["Main"], colors[v["Tone"]])
    parts += [text(160, 126, v["Caption"], 20, colors[0], True)]
    bw = 230 if b["TwoLines"] else 218
    parts += [f'<rect x="{160-bw/2}" y="140" width="{bw}" height="40" rx="20" fill="{fills.get(b["Tone"], fills[0])}" stroke="{colors[b["Tone"]]}" stroke-opacity=".35"/>']
    if b["TwoLines"]:
        parts += [text(160, 155, b["Status"], 16, colors[b["Tone"]], True),
                  text(160, 174, b["Detail"], 18, colors[b["Tone"]], True)]
    else:
        parts += [text(160, 167, b["Status"]+"  "+b["Detail"], 18, colors[b["Tone"]], True)]
    ny = 202
    if v["NextFlightNotice"]:
        parts += [text(160, ny, v["NextFlightNotice"], 16, colors[v["NextFlightTone"]], True)]
        ny += 20
    if v["DestinationNotice"]:
        parts += [text(160, ny, v["DestinationNotice"], 16, colors[2], True)]
    parts += ["</g>"]
parts += [text(36, 681, "layout study · simulated values · not an in-game capture", 13),
          text(36, 704, "/potd opens settings · /potd preview tries these states in the native HUD", 13, "#e0b8ff"),
          "</g></svg>"]
(root / "assets/counter-preview.svg").write_text("\n".join(parts)+"\n", encoding="utf-8")
print(f"refreshed layout studies; {len(scenes)} real presentation states fit the headline band")
