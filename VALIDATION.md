# validation · 0.1.3.0 · 2026-11-02

## checked for this update

| check | result |
| --- | --- |
| core and presentation | 2,312 assertions passed across all 200 floor numbers and relevant Flight states |
| fresh floor 1 | tracker-to-HUD path shows ~3–7 with Flight Unavailable, one kill reduces the estimate to ~2–6 |
| late floor 1 | tracker establishes inactive incoming Flight but retains partial kill confidence, HUD shows ~1–7 and a partial label |
| later starting floor | floor 161 with unconfirmed incoming Flight shows ~2–10 and an UNKNOWN badge |
| conservative bounds | each sample range contains all compatible hidden quotas and missing-kill outcomes, with and without Flight |
| existing mechanics | kill deduplication, consecutive Flight carry, stale-flag settling, partial party counts and session boundaries remain covered |
| passage and failures | confirmed open state wins, unavailable tracking stays paused, exceeding the range cannot fabricate an open passage |
| presentation | 17 shared native/model examples, all headline widths checked, no native layout changes |
| compilation | actual Dalamud API 15 references, zero warnings and zero errors |
| package | five reviewed runtime/license files, no debug metadata, host assemblies or identifying build paths |

the [release](https://github.com/bubblyslimea/potd-kill-counter/releases/tag/v0.1.3-alpha.1) supplies the Windows ZIP, checksum, repository entry and individual runtime file hashes
the local review build and Windows release are separate builds, use the release record when checking the download

## live evidence and remaining review

the user reported that the earlier counter worked well, then requested that the first floor retain its kills-left estimate
the uncertainty paths were reproduced through the real core tracker and presentation code
the precise triggering condition in their live run was not captured

1. update to 0.1.3.0, open `/potd preview`, then `/potd` and choose **first floor**, **first floor, joined late** and **later starting floor**
2. confirm the number stays above the Flight badge, and the late example retains its partial label
3. enter a fresh floor and check that each observed kill reduces the estimate
4. confirm a real open passage replaces the estimate, even when count or Flight confidence is incomplete

the new live starting-floor case has not been exercised inside FFXIV in this environment
the layout studies use simulated inputs and the actual presentation model, they are not in-game captures
