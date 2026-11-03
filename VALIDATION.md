# validation · 0.1.5.0 · 2026-11-03

## stable promotion

the stable catalog uses AssemblyVersion 0.1.5.0, IsTestingExclusive false and no Counter testing-only metadata
four release-channel tests cover stable visibility, testing isolation, invalid channel/version combinations and preserved identity
the Windows promotion must match the existing alpha ZIP SHA-256 exactly before it can publish
plugin source and runtime behavior are unchanged by this channel promotion

## checked for this update

| check | result |
| --- | --- |
| core and presentation | 2,433 assertions passed across all 200 floor numbers and relevant Flight states |
| victory confirmation | all 19 expected boss IDs accepted, wrong identities, non-enemies, unknown actors, stale floors and instances rejected |
| victory timing | no celebration from combat ending alone, pending while combat continues, 180 ms fade, four-second boundary, duplicates and repeated reads cannot restart it |
| victory resets | instance/floor changes and explicit reset clear the latch, reloading on a cleared floor cannot replay it |
| boss names | all 19 names checked against the linked floor guides, each appears below BOSS without requiring a target or kill observer |
| boss visibility | visible before the pull, hidden during boss combat, restored when combat ends or cancels, still hidden if passage opens before combat ends |
| visibility boundaries | ordinary-floor fights and floor 200 remain visible, combat hiding preserves the user's display preference |
| fresh floor 1 | tracker-to-HUD path shows ~3–7 with Flight Unavailable, one kill reduces the estimate to ~2–6 |
| late floor 1 | tracker establishes inactive incoming Flight but retains partial kill confidence, HUD shows ~1–7 and a partial label |
| later starting floor | floor 161 with unconfirmed incoming Flight shows ~2–10 and an UNKNOWN badge |
| conservative bounds | each sample range contains all compatible hidden quotas and missing-kill outcomes, with and without Flight |
| existing mechanics | kill deduplication, consecutive Flight carry, stale-flag settling, partial party counts and session boundaries remain covered |
| passage and failures | confirmed passage status remains visible during victory and takes over after its timer, unavailable counting stays paused, exceeding a range cannot fabricate an open passage |
| presentation | 23 shared native/model examples plus all 19 victory headlines checked against the actual font, one-line and two-line victory layouts reviewed |
| compilation | actual Dalamud API 15 references, zero warnings and zero errors |
| package | five reviewed runtime/license files, no debug metadata, host assemblies or identifying build paths |

the [release](https://github.com/bubblyslimea/potd-kill-counter/releases/tag/v0.1.5) supplies the Windows ZIP, checksum, repository entry and individual runtime file hashes
the local review build and Windows release are separate builds, use the release record when checking the download

## live evidence and remaining review

the user reported that the PotD tracker works perfectly before this optional victory update
the new identity and timing paths were exercised through the actual core model
compilation used the actual Dalamud host references, native event delivery cannot be exercised in this environment

1. update to 0.1.5.0, open `/potd preview`, then `/potd` and select **boss victory**, **victory · two-line name**, **victory · passage pending** and **after the victory card**
2. confirm the name is the headline, DEFEATED is underneath, and long names fit cleanly at your UI scale
3. in a live boss fight, confirm the HUD stays hidden, then shows the correct name only after a confirmed kill and the end of combat
4. check the gentle fade and four-second duration, confirmed passage text, and transition back to OPEN
5. confirm resets and wipes do not celebrate, reopening the HUD does not replay a victory, and ordinary-floor counts remain visible during combat

preview presets intentionally hold their chosen state, the live timer is verified by the core checks and needs the in-game timing check above
the layout studies use simulated inputs and the actual presentation model, they are not in-game captures
