# validation · 0.1.2.0 · 2026-11-02

## checked for this update

| check | result |
| --- | --- |
| core and presentation | 2,287 assertions passed, including all 200 floor numbers and all current/next Flight combinations |
| kill handling | existing duplicate, simultaneous, stale-floor and invalid-ID checks pass |
| Flight | existing rounding, consecutive-use and transition checks pass |
| current and next | active, queued and boss notices coexist, unknown never becomes inactive |
| first floor | exact requested badge only on floor 1 with known inactive incoming Flight, queued use can coexist |
| passage | confirmed open state wins with unavailable tracking, unknown Flight or incomplete count |
| special floors | bosses have no normal quota, floor 199 points to completion, floor 200 has no next-floor display |
| compilation | actual Dalamud API 15 references, zero warnings and zero errors |
| commands and preferences | both aliases retained, same window identity and saved placement, lock and opacity |
| fonts | managed atlas sizes, embedded headline subset, scoped pushes and disposal on unload |
| presentation | 15 shared native/model examples, headline bounds checked, six-state vector study visually reviewed |
| packaging | five runtime/license files, embedded font notice, no host assemblies, debug files or private build paths |

the [release](https://github.com/bubblyslimea/potd-kill-counter/releases/tag/v0.1.2-alpha.1) includes a checksum file and individual runtime file hashes
the local review build and Windows release are separate builds, use the release record when checking the download

## live evidence and remaining review

the user reported that 0.1.0.0 works very well in a live PotD run
the previous overlay was then reviewed in-game and the replacement design was approved
that does not establish that every transition or uncertainty case has been exercised

the 0.1.2.0 native renderer has not been exercised inside FFXIV in this environment
the HTML and vector studies use real presentation outputs with simulated input, neither is an in-game capture
native font loading, scaling and readability over combat still need an in-game look

1. update to 0.1.2.0, open `/potd preview`, then `/potd` and choose the first-floor, active, queued, both and open examples
2. check your usual Dalamud UI scale, placement and opacity, then lock and unlock the overlay
3. inspect partial, unknown, unavailable, boss, final-floor and completion examples
the two-notice example must keep both lines visible without changing the card's size
4. open **run details** while locked and check the observed count, target range and explanation
5. in PotD, confirm the real run replaces simulation, one increment per observed enemy, and the existing Flight carry behavior
6. check queued Flight becomes active after descending, then expires without another use
7. check reloads and party counts remain partial, and the live overlay hides outside PotD

if something differs from the game, include the floor, plugin version, displayed status and what happened in a GitHub issue
