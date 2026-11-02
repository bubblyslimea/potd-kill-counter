"""Optional maintainer tool; normal builds use the committed font subset.

Usage: python scripts/subset-headline-font.py /path/to/DejaVuSans-Bold.ttf
Requires fonttools. Source: DejaVu Fonts 2.37, license in THIRD_PARTY_NOTICES.md.
"""
from pathlib import Path
import sys
from fontTools import subset
from fontTools.ttLib import TTFont

font = TTFont(sys.argv[1], recalcTimestamp=False)
options = subset.Options()
options.name_IDs = [0, 1, 2, 3, 4, 5, 6, 13, 14]
options.name_legacy = True
options.name_languages = [0x409]
builder = subset.Subsetter(options=options)
builder.populate(text=" 0123456789~–?OPENBOSSCLEARCHECKWAIT")
builder.subset(font)
names = {1: "Riri Counter Headline", 2: "Bold", 3: "RiriCounterHeadline-Bold-1",
         4: "Riri Counter Headline Bold", 6: "RiriCounterHeadline-Bold"}
for name in font['name'].names:
    if name.nameID in names:
        name.string = names[name.nameID].encode(name.getEncoding())
font['head'].created = font['head'].modified = 3849984000
target = Path(__file__).resolve().parents[1] / 'assets/CounterHeadline.ttf'
font.save(target)
print(f'headline subset: {target.stat().st_size} bytes')
