# validation · 0.1.4.0 · 2026-11-02

## checked for this update

| check | result |
| --- | --- |
| core and presentation | 2,355 assertions passed across all 200 floor numbers and relevant Flight states |
| boss names | all 19 names checked against the linked floor guides, each appears below BOSS without requiring a target or kill observer |
| boss visibility | visible before the pull, hidden during boss combat, restored when combat ends or cancels, still hidden if passage opens before combat ends |
| visibility boundaries | ordinary-floor fights and floor 200 remain visible, combat hiding preserves the user's display preference |
| fresh floor 1 | tracker-to-HUD path shows ~3–7 with Flight Unavailable, one kill reduces the estimate to ~2–6 |
| late floor 1 | tracker establishes inactive incoming Flight but retains partial kill confidence, HUD shows ~1–7 and a partial label |
| later starting floor | floor 161 with unconfirmed incoming Flight shows ~2–10 and an UNKNOWN badge |
| conservative bounds | each sample range contains all compatible hidden quotas and missing-kill outcomes, with and without Flight |
| existing mechanics | kill deduplication, consecutive Flight carry, stale-flag settling, partial party counts and session boundaries remain covered |
| passage and failures | confirmed open state wins, unavailable tracking stays paused, exceeding the range cannot fabricate an open passage |
| presentation | 20 shared native/model examples, all headline widths checked, named-boss layout reviewed with no native layout changes |
| compilation | actual Dalamud API 15 references, zero warnings and zero errors |
| package | five reviewed runtime/license files, no debug metadata, host assemblies or identifying build paths |

the [release](https://github.com/bubblyslimea/potd-kill-counter/releases/tag/v0.1.4-alpha.1) supplies the Windows ZIP, checksum, repository entry and individual runtime file hashes
the local review build and Windows release are separate builds, use the release record when checking the download

## live evidence and remaining review

the user reported that the earlier counter worked well, then requested boss names and a quiet screen during boss fights
boss identity, visibility and restoration were checked through the real display model
the live snapshot reads the supported Dalamud combat condition at draw time, the tracker continues while the HUD is hidden

1. update to 0.1.4.0, open `/potd preview`, then `/potd` and choose **boss before pull**, **boss fight · HUD hidden** and **boss cleared**
2. confirm BOSS has Dendainsonne beneath it, the combat example hides the overlay, and the cleared example returns as OPEN
3. on a real boss floor, check that the overlay disappears when combat starts and returns when combat ends, including a reset
4. check the timing in both solo and party runs, and confirm ordinary-floor combat keeps the counter visible
5. retain the first-floor check: kills left stays above Flight Unavailable, each observed kill reduces the estimate, and confirmed passage activation takes priority

the native combat transition has not been exercised inside FFXIV in this environment
the layout studies use simulated inputs and the actual presentation model, they are not in-game captures
