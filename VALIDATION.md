# validation · 2026-11-02

## completed

| Check | Result |
| --- | --- |
| Pure C# behavior tests | PASS · 2,201 assertions |
| Floor coverage | All 200 floor numbers, normal quotas, bosses and completion |
| Kill handling | Enemy-only core input, duplicate IDs, multiple deaths, stale floor/instance IDs, invalid IDs |
| Flight | Rounded-up halving, active/queued separation, one-floor duration, consecutive uses, early flag clearing and stale flags |
| Confidence | Hot-load, party coverage, suspended reads, skipped floors and unknown Flight |
| Passage | Actual open state overrides estimates; no false open from reaching the range ceiling |
| Native API compilation | PASS against actual Dalamud API 15 reference assemblies |
| Release build | PASS · zero warnings, zero errors |
| Runtime assembly | 35,840 bytes; no extra core assembly |
| Preview | labelled vector layout preview; HTML JavaScript syntax checked |
| Privacy | Approved creator identity; workspace paths absent from release assembly |

release DLL SHA-256:

`8df7f4b7ff7fcbd8d452eff995938dad9c3fe0bbaaf4bc53153488a5018bc0b3`

## still requires FFXIV

this environment does not run FFXIV. No live game run, native hook execution, in-game UI rendering or CPU/FPS benchmark was performed
the standalone HTML review was not browser-tested because the headless-browser download failed; it is supplemental to the plugin's native preview

the following checks should happen in a low-floor test run before relying on this build for a high-floor attempt:

1. load the dev plugin and run `/potdcount preview`. Check size, position, click-through and each native simulation state
2. enter solo PotD with the plugin already enabled
confirm the floor number and one increment for each enemy death, including a multi-target kill
player/pet deaths must not increment it
3. use Flight
confirm **Next: queued**, then descend and confirm **Flight here: active**. With no second use, the following floor should show Flight off
4. watch the Cairn activate and confirm **passage open** immediately replaces the estimate, including a possible early full clear
5. reload mid-floor
confirm the count is labelled partial and current Flight unknown rather than silently starting a supposedly complete count at zero
6. confirm party mode is labelled partial, boss floors have no ordinary quota, and the overlay hides outside PotD

if counts or Flight differ from the client, report the floor, observed behavior, displayed status and plugin version
the game reader is isolated so these details can be corrected without changing the estimator or UI
