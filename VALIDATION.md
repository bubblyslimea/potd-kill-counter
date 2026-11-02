# validation · 2026-11-02

## checked before publication

| check | result |
| --- | --- |
| core behavior | 2,201 assertions passed across all 200 floor numbers |
| kill handling | duplicate IDs, simultaneous deaths, stale floors and invalid IDs covered |
| Flight | rounding, active and queued states, consecutive uses and transition flags covered |
| uncertainty | late attachment, party coverage, skipped floors and suspended reads covered |
| passage | actual open state overrides estimates, the range ceiling never claims it is open |
| compilation | actual Dalamud API 15 references, zero warnings and zero errors |
| Windows release | GitHub Actions rebuilt, tested and packaged the published alpha |
| presentation | labelled vector layout preview visually reviewed, no in-game capture claimed |
| privacy | proposed source, creator attribution, archive members and embedded path strings reviewed |

the published Windows ZIP has this SHA-256:

`229c41e3560397c182ae2b92024381928cd9adfe5e4b4f4fbd999560cd650874`

the [release](https://github.com/bubblyslimea/potd-kill-counter/releases/tag/v0.1.0-alpha.1) also includes a checksum file and a record of the individual runtime file hashes
the local review build and Windows release are separate builds, use the release's record when checking the download

## still needs a trip into the palace

no live game run, native hook execution, in-game UI capture or CPU/FPS benchmark has been performed
the standalone HTML layout study has only had its script syntax checked, it is supplemental to the native preview

please try these in a low-floor run before relying on the alpha for an important climb:

1. install the testing plugin and use `/potdcount preview`, check the position, size, click-through lock and each simulation state
2. enter solo PotD with the counter already enabled, confirm the floor and one increment per enemy death, including simultaneous kills, player and pet deaths must not count
3. use Flight, check **next: queued**, then descend and check **Flight here: active**, without another use the following floor should show it off
4. watch the Cairn activate and check that **passage open** replaces the estimate, including a possible early full clear
5. reload mid-floor and check that the count is partial and current Flight is unknown
6. check party mode stays partial, boss floors have no ordinary quota, and the live overlay hides outside PotD

if something differs from the game, include the floor, plugin version, displayed status and what happened in a GitHub issue
the reader is separate from the estimator so a native observation fix does not need a new counting model
