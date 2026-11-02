# read the state, keep moving

the new HUD is here !!

big, crisp kills left, a clear Flight badge, and a separate reminder for the next floor
lavender means active here, blue means Flight was activated for the next floor, amber calls out a boss ahead
floor 1 gets the requested **FIRST FLOOR: Flight Unavailable** badge

when the game confirms the passage is open, **OPEN** takes the main spot
uncertain counts keep their question mark and a short reason
observed kills, full ranges and explanations are in **run details** inside `/potd`

open `/potd preview` to try the new card, then `/potd` to choose from 15 examples
`/potdcount` still works, and your placement, opacity and click-through settings are kept
counting and Flight timing are unchanged

update in `/xlplugins` using your existing [shared Riri repository](https://give-riri-a-cookie.pages.dev/plugins/repo.json) with testing builds enabled
both the `/plugins/repo.json` and `/notation/repo.json` addresses work, keep only one row

the first alpha has a positive live PotD report
this update passed 2,287 offline assertions and compilation against the actual Dalamud host
the layout study is labelled simulated, the new native rendering still needs its in-game look

[installation and testing notes](https://github.com/bubblyslimea/potd-kill-counter/blob/main/TECHNICAL_DETAILS.md)
