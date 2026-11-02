# Riri's PotD Counter

a little counter for the climb

![layout preview with simulated kills and Flight states](assets/counter-preview.svg)

*a layout preview with pretend values, in-game screenshots are still to come*

keeps track of the kills you see, your Flight pomanders, and roughly how much might be left
when the passage opens, it lets you know

## come try it

1. add the [shared Riri repository](https://give-riri-a-cookie.pages.dev/plugins/repo.json) in `/xlsettings` → **Experimental → Custom Plugin Repositories** and save
2. enable **Get plugin testing builds**, then find **Riri's PotD Counter** in `/xlplugins` and install
3. open `/potd` to settle it into place, or `/potd preview` to try it outside PotD

`/potdcount` still works too

the big number is the rough **kills left**, with Flight split into **this floor** and **next floor**
open **details** when you want the full floor range, the rest stays tucked away

already using the Pages `/notation/repo.json` address? that row works too, you only need one

the first alpha has a positive live PotD report, this update gives the counter a little more room to breathe
the new layout is still in testing while it gets an in-game look

![the shared repository page with Notation and the counter](assets/shared-repository.jpg)

[one address for my little tools](https://give-riri-a-cookie.pages.dev/plugins/)

[what changed](CHANGELOG.md) · [floor ranges](FLOOR_RANGES.md) · [technical details](TECHNICAL_DETAILS.md) · [MIT license](LICENSE)

by Riri Fujiwara
