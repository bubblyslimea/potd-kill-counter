# a moment for the victory

the boss’s name gets the spotlight after a confirmed defeat
**DEFEATED** and a green check sit underneath, with clear, neutral wording in the tracker

the HUD stays hidden while you fight
once the expected boss’s death is observed and combat ends, the card fades in for a four-second victory moment
confirmed passage status stays visible, then OPEN returns
if passage data is still pending, the known defeat stays visible until it updates

resets and wipes do not trigger it, duplicate events and reopening the HUD do not replay it
long names fit on two lines, ordinary-floor counting and Flight behavior stay the same

update in `/xlplugins` through your existing [shared Riri repository](https://give-riri-a-cookie.pages.dev/plugins/repo.json)
try `/potd preview`, then open `/potd` and choose **boss victory**, **victory · two-line name** or **after the victory card**
preview holds the chosen state for inspection, the live card uses the timer

the offline checks cover all 19 bosses, rejected events, timing, fade, resets, passage priority and existing counting rules
the native boss-death event and timing still need their in-game confirmation

[installation and testing notes](https://github.com/bubblyslimea/potd-kill-counter/blob/main/TECHNICAL_DETAILS.md)
