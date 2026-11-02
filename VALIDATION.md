# validation · 0.1.1.0 · 2026-11-02

## checked for this update

| check | result |
| --- | --- |
| core and presentation | 2,216 assertions passed across all 200 floor numbers |
| kill handling | existing duplicate, simultaneous, stale-floor and invalid-ID checks pass |
| Flight | existing rounding, consecutive-use and transition checks pass |
| compact display | partial counts, unknown Flight and unavailable tracking keep visible caveats |
| passage | confirmed open state wins even with an unavailable observer or incomplete count |
| special floors | boss floors have no normal target, completion has no next-floor display |
| compilation | actual Dalamud API 15 references, zero warnings and zero errors |
| commands | both aliases use the same handler, disposal removes only owned registrations |
| preferences | existing position, opacity, lock and range settings retained, details default closed |
| presentation | nine states exported by the real C# presentation model, labelled vector preview visually reviewed |
| packaging | five reviewed runtime/license files, no host assemblies or debug files |
| Windows release | the publication workflow repeats checks and packaging before creating the release |

the [release](https://github.com/bubblyslimea/potd-kill-counter/releases/tag/v0.1.1-alpha.1) includes a checksum file and a record of individual runtime file hashes
the local review build and Windows release are separate builds, use the release's record when checking the download

## live evidence and remaining review

the user reported that 0.1.0.0 works very well in a live PotD run
that is useful general confirmation, it does not establish that every transition or uncertainty case has been exercised

the 0.1.1.0 native layout and slash-command aliases have not been exercised inside FFXIV in this environment
the HTML and vector studies show simulated values, neither is an in-game capture
the HTML script passed a syntax check, native font rendering and global UI scaling still need an in-game look

1. update to 0.1.1.0, open both `/potd` and `/potdcount`, then try `/potd preview`
2. check the larger remaining range, Flight columns, details toggle and opacity over the game
try your usual Dalamud UI scale, lock and unlock the overlay, confirm its placement survived the update
3. check the preview's partial, unknown Flight, paused, open, boss and completion states
a confirmed open passage must replace an estimate even when counting is unavailable
4. in PotD, confirm one increment per enemy death and the same Flight carry behavior as before
5. check a queued Flight becomes active after descending, and goes off on the following floor without another use
6. check reloads and party counts remain partial, and the live overlay hides outside PotD

if something differs from the game, include the floor, plugin version, displayed status and what happened in a GitHub issue
