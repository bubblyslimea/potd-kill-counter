# a little more room to breathe

/potd works now too !!

open `/potd` for settings, `/potd preview` for a simulation or `/potd toggle` to show and hide the counter
`/potdcount` still accepts the same commands

the rough kills left is the main thing you see now
Flight this floor and Flight next floor have their own little columns, with full ranges and explanations tucked into **details**
when the passage opens, that becomes the main message

counting and Flight timing keep the same behavior, and unknown counts stay clearly labelled
there are no new runtime dependencies or network requests

update the plugin in `/xlplugins`, using your existing [shared Riri repository](https://give-riri-a-cookie.pages.dev/plugins/repo.json) with testing builds enabled
both the `/plugins/repo.json` and `/notation/repo.json` addresses serve this update, keep only one row

the first alpha has a positive live PotD report
this redesign passed 2,216 offline assertions and a build against the actual Dalamud host, its native appearance still needs an in-game look

[installation and testing notes](https://github.com/bubblyslimea/potd-kill-counter/blob/main/TECHNICAL_DETAILS.md)
