"""Regenerate labelled layout studies from the real C# presentation fixtures."""
from pathlib import Path
import html
import json
import sys

root = Path(__file__).resolve().parents[1]
scenes = json.loads(Path(sys.argv[1]).read_text())
template = """<!doctype html>
<html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>Riri's PotD Counter · layout preview</title>
<style>
:root{color-scheme:dark;font-family:system-ui,sans-serif;background:#15151b;color:#f5f2f7}
*{box-sizing:border-box}body{margin:0;padding:40px 24px}main{max-width:820px;margin:auto}
.eyebrow{color:#fab8d1;font-size:13px}h1{font-size:32px;font-weight:600;letter-spacing:-.03em;margin:12px 0}
p{color:#b8b5c2;line-height:1.6;font-size:14px}.stage{display:flex;justify-content:center;align-items:center;min-height:340px;padding:30px 12px;margin:26px 0;background:#20202a;border:1px solid #38333e;border-radius:16px}
.hud{width:320px;padding:14px 16px;border:1px solid #4d4552;border-radius:10px;background:#1a1a21;font-size:16px}
.context{display:flex;justify-content:space-between;flex-wrap:wrap;gap:6px;font-size:14px}.floor{color:#fab8d1}.observed{color:#b8b5c2}
.answer{margin:14px 0 12px}.main{font-size:22px;line-height:1.3}.main.number{font-size:32px}.caption{font-size:15px;color:#b8b5c2;margin-top:5px}
.tone-1{color:#85e6b8}.tone-2{color:#ffcf75}
.flight{border-top:1px solid #48404d;padding-top:12px;display:grid;grid-template-columns:1fr 1fr;gap:12px}
.flight .label{font-size:13px;color:#b8b5c2;margin-bottom:5px}.flight .off{color:#b8b5c2}.flight .on{color:#85e6b8}.flight .unknown{color:#ffcf75}
details{margin-top:13px}summary{color:#fab8d1;font-size:14px;cursor:pointer;list-style:none}summary:focus-visible{outline:2px solid #fab8d1;outline-offset:4px}.details-text{font-size:14px;line-height:1.5;margin:10px 0 0}
.controls{display:flex;flex-wrap:wrap;gap:8px}button{background:#25232c;border:1px solid #49414f;color:#dad4e0;padding:9px 12px;border-radius:8px;font:inherit;font-size:13px;cursor:pointer}button[aria-pressed=true]{background:#48323e;border-color:#c68da5;color:#ffe9f2}button:focus-visible{outline:2px solid #fab8d1;outline-offset:3px}
[hidden]{display:none!important}.foot{font-size:12px}@media(max-width:420px){body{padding:24px 12px}h1{font-size:27px}.stage{padding:24px 8px}.hud{max-width:100%}}
</style>
<main>
<div class="eyebrow">Riri's PotD Counter · 0.1.1</div>
<h1>a little more room to breathe</h1>
<p>one main answer, a little Flight reminder, and details when you want them</p>
<p class="foot">layout study · pretend values · this is not an in-game capture</p>
<section class="stage" aria-label="Simulated overlay">
<div class="hud">
<div class="context"><span class="floor" id="floor"></span><span class="observed" id="observed"></span></div>
<div class="answer" role="status" aria-live="polite"><div id="main" class="main"></div><div id="caption" class="caption"></div></div>
<div class="flight" id="flight"><div><div class="label">Flight this floor</div><div id="here"></div></div><div><div class="label">Flight next floor</div><div id="next"></div></div></div>
<details id="details"><summary id="detail-label">details</summary><p id="range" class="details-text"></p><p id="note" class="details-text"></p><p id="flight-note" class="details-text">Flight used here helps on the next floor</p></details>
</div></section>
<nav class="controls" id="controls" aria-label="Example states"></nav>
<p class="foot">open /potd for settings · /potdcount still works too<br>these examples use the counter's real presentation model, the native font follows Dalamud</p>
</main>
<script>
const scenes=SCENES_JSON;
const $=id=>document.getElementById(id);
function show(s){
  const v=s.view;
  $('floor').textContent='floor '+s.floor;
  $('observed').textContent=v.Observed||'';
  $('main').textContent=v.Main;
  $('main').className='main tone-'+v.Tone+(v.IsEstimate?' number':'');
  $('caption').textContent=v.Caption;
  $('flight').hidden=!v.ShowFlight;
  $('here').textContent=s.hereLabel;
  $('next').textContent=s.nextLabel;
  $('here').className=s.here.toLowerCase();
  $('next').className=s.next.toLowerCase();
  $('range').textContent=v.Range||'';
  $('range').hidden=!v.Range;
  $('note').textContent=v.Note;
  $('flight-note').hidden=!v.ShowFlight;
  document.querySelectorAll('button').forEach(b=>b.setAttribute('aria-pressed',String(b.dataset.id===s.id)));
}
$('details').addEventListener('toggle',()=>{$('detail-label').textContent=$('details').open?'less detail':'details'});
for(const s of scenes){
  const b=document.createElement('button');
  b.textContent=s.label;
  b.dataset.id=s.id;
  b.addEventListener('click',()=>show(s));
  $('controls').appendChild(b);
}
show(scenes[0]);
</script></html>
"""
(root / "PREVIEW.html").write_text(template.replace("SCENES_JSON", json.dumps(scenes, ensure_ascii=False)), encoding="utf-8")
colors = ["#f5f2f7", "#85e6b8", "#ffcf75"]
def text(x, y, value, size=14, color="#b8b5c2", anchor=None):
    alignment = f' text-anchor="{anchor}"' if anchor else ""
    return f'<text x="{x}" y="{y}" fill="{color}" font-size="{size}"{alignment}>{html.escape(value)}</text>'
parts = ['<svg xmlns="http://www.w3.org/2000/svg" width="1080" height="430" viewBox="0 0 1080 430">',
         '<rect width="1080" height="430" rx="18" fill="#15151b"/>',
         '<g font-family="DejaVu Sans,Arial,sans-serif">',
         text(36, 48, "Riri's PotD Counter", 25, "#f5f2f7"),
         text(36, 79, "a little more room to breathe", 16, "#fab8d1")]
for x, scene_id in zip([36, 380, 724], ["normal", "open", "partial"]):
    s = next(s for s in scenes if s["id"] == scene_id)
    v = s["view"]
    parts += [f'<g transform="translate({x} 111)">',
              '<rect width="320" height="238" rx="10" fill="#1a1a21" stroke="#4d4552"/>',
              text(16, 30, f'floor {s["floor"]}', color="#fab8d1"),
              text(304, 30, v["Observed"] or "", anchor="end"),
              text(16, 86, v["Main"], 34 if v["IsEstimate"] else 23, colors[v["Tone"]]),
              text(16, 113, v["Caption"], 15),
              '<path d="M16 135H304" stroke="#48404d"/>',
              text(16, 160, "Flight this floor", 13),
              text(164, 160, "Flight next floor", 13)]
    for cx, label, state in [(16, s["hereLabel"], s["here"]), (164, s["nextLabel"], s["next"])]:
        parts.append(text(cx, 186, label, 16, {"On":"#85e6b8","Off":"#b8b5c2","Unknown":"#ffcf75"}[state]))
    parts += [text(16, 220, "details", 14, "#fab8d1"), "</g>"]
parts += [text(36, 387, "layout preview · simulated values · not an in-game capture", 13),
          text(36, 410, "/potd opens settings · /potdcount still works too", 13, "#c9a8b9"),
          "</g></svg>"]
(root / "assets/counter-preview.svg").write_text("\n".join(parts)+"\n", encoding="utf-8")
print(f"refreshed layout studies from {len(scenes)} C# presentation states")
