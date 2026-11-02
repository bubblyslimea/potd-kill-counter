# Riri's PotD Counter · Asura design

## 0.1.3 starting-floor correction

keep the kills-left estimate visible even when no incoming Flight applies or its state is unknown
the prior presentation could replace the entire estimate with a question mark on live starting floors

keep the existing native layout and counting hooks
the tracker can establish that actual floor 1 has no previous-floor Flight without assuming every kill was observed
the estimator returns a wider range when confidence is incomplete
unknown Flight spans both possible targets, partial counts start at one and subtract only observed kills from the maximum
the HUD keeps its approximation marker, amber tint and partial-count caption or unknown Flight badge
unavailable tracking still pauses estimates, confirmed passage activation still wins, and a passed maximum never becomes a false open signal

sequence: reproduce fresh and late-attached floor 1 plus a later starting floor, fix the pure estimate and presentation, then verify the same paths through the tracker
check conservative bounds against possible missing kills and both Flight outcomes, compile, and publish a small testing update through the existing catalog

## 0.1.2 approved HUD brief

help a runner read the counter during combat with the hierarchy approved in the Watch-inspired design study
one large answer, a current Flight badge, then a separate upcoming-floor notice

- keep a fixed 320 × 232 logical-pixel card, scaled with Dalamud, with a quiet floor label and a crisp, centred headline
- use `~1–9` and `kills left` for estimates, `OPEN` and a checkmark only for a confirmed open passage
- show wider, qualified estimates for partial counts or unknown Flight, `?` for unavailable tracking, and `CHECK` when the range is exceeded
- keep `ACTIVE Flight` lavender, `INACTIVE Flight` neutral and `UNKNOWN Flight` amber in the same badge position
- on actual floor 1 with known inactive Flight, show the requested two-line `FIRST FLOOR:` / `Flight Unavailable` badge
this describes the incoming effect, not pomander inventory, and does not apply to floors 11, 51 or 191
the tracker knows actual floor 1 cannot inherit a previous-floor effect, even when its kill count is partial
- show `NEXT FLOOR: Flight activated` in blue for an observed queue and `NEXT FLOOR: Flight unknown` in amber when unsettled
reserve a second upcoming line for `BOSS NEXT FLOOR` so a boss notice cannot erase a Flight observation
floor 199 says `FINAL FLOOR NEXT`, floor 200 has no Flight display or fictional next floor
- keep observed kills, full ranges and explanations in `/potd` settings, including while the overlay is locked
- the card has no controls, columns, dividers, progress rings or animation
keep its current window identity, placement, opacity and click-through preference
- label native previews as simulations above the card, real runs always take priority
provide the approved states as preview presets as well as manual simulation controls

keep the existing C# / Dalamud / ImGui stack and all counting, floor, Flight transition and polling mechanics
pure presentation code selects truthful content, native drawing owns fonts and placement, settings own details
use the managed atlas at actual rendering sizes, with no window-font enlargement
an embedded, renamed Latin headline subset of DejaVu Sans Bold supplies the heavy digits without OS font assumptions or network requests
the font notice ships with the existing acknowledgements, other text uses Dalamud's own font assets

implementation sequence: verify presentation states and boundary floors, add managed fonts and fixed native drawing, move details to settings, then refresh the labelled previews and publish a new testing version
verify all existing mechanics plus all current/next Flight combinations, first-floor truth, boss and completion boundaries, confirmed-passage priority and text fitting
compile against actual Dalamud references, inspect the generated layout study, and keep native appearance and scaling marked for in-game review

## build brief

build a lightweight, automatic Dalamud overlay for Palace of the Dead that counts observed enemy kills on each floor, estimates the kills still required to activate the Cairn of Passage, and distinguishes Flight active on this floor from Flight queued for the next
show uncertainty honestly and let the game's passage state override every estimate

## goal

help a PotD runner decide how much fighting remains before the next floor without manually counting kills or remembering Flight

## requirements and boundaries

- automatically detect PotD and floors 1–200; hide outside PotD except for an explicitly labelled preview
- count enemy death events once per entity per floor, including multiple simultaneous deaths
exclude players, pets, replays and other duties
- show an observed kill count, a published activation range and a remaining range when the observation is complete enough to support it
- the activation requirement is random
never claim to know the hidden target
passage progress is a client state, not a percentage or an exact kill counter
- Flight applies on the next floor, halves the activation requirement and rounds up
its queued flag is read from the client
carry an observed queue to the immediately following floor; never infer use from an inventory decrease
- treat late attachment, missing information, reconnects, skipped floors and unavailable observations explicitly
unknown Flight is not equivalent to no Flight
- boss floors 10–190 have no normal monster quota
floor 200 is the completion floor, not another boss
- A clear-all can open the passage before the published minimum; the actual open state always wins
- party kills outside the receiving client's view can be missed
mark party counts as partial and show a conservative remaining range, never a complete party total
- disable the optional native death observer cleanly if its supported address cannot be resolved; do not substitute a misleading working counter
- moveable, compact overlay; saved display preferences; slash command for settings and a simulation preview
- no backend, telemetry, texture downloads, combat control, full object-table polling, per-kill disk writes, or run-history database

## tools and components

| Component | Responsibility | Change location |
| --- | --- | --- |
| `Core/FloorRules.cs` | Published floor ranges and Flight rounding | New/revised research |
| `Core/FloorTracker.cs` | Session boundaries, kill deduplication, confidence and Flight carry | Counting and transition behavior |
| `Core/PassageEstimator.cs` | Pure estimates from tracked facts | Estimation policy |
| `Game/PotdReader.cs` | Supported FFXIVClientStructs and Dalamud data access | Game/Dalamud API updates |
| `Game/EnemyDeathObserver.cs` | Observe the supported actor-control death callback | Death event integration |
| `Plugin.cs` | Polling, lifecycle and event coordination | Plugin wiring |
| `UI/CounterWindow.cs` | Compact display and settings | Presentation |

C# and .NET 10; Dalamud API 15; framework-provided FFXIVClientStructs and ImGui
A dependency-free console test harness exercises the same core C# sources as the plugin
A release build uses actual Dalamud assemblies, not mocks

## event sequence

1. every 200 ms, the game reader obtains a validated PotD snapshot on the framework thread
2. A new floor resets the per-floor count and deduplication set
A witnessed consecutive transition carries the prior Flight queue into this floor's active state
3. A witnessed queue is latched for the rest of its floor
A short settling interval prevents an old queued flag from being mistaken for a new-floor use during transition
if the old flag persists, first observe it clearing; a subsequent queue belongs to the next floor
4. the death observer captures the floor and enemy identity, then queues a small managed observation
the game's callback always runs exactly once and its arguments are unchanged
5. on framework updates, observations matching the current floor/session are consumed
unknown actor identities lower confidence; known non-enemies do not count
6. the pure estimator returns an estimated remaining range, widening it for unknown Flight or missing earlier kills
while passage is closed its lower bound cannot be zero
passing the published maximum produces an uncertainty message, never a false open signal
7. the overlay renders the snapshot
actual passage activation overrides all estimates
outside PotD the observer is disabled and the live window is hidden

## implementation sequence

1. implement and execute tests for floor rules, rounding, unknown states and remaining estimates
2. implement and test session transitions, duplicate deaths, late attachment, skipped floors, Flight queues and boss/completion floors
3. connect the game adapter and death observer; build against the current Dalamud SDK
4. add the compact overlay and preview
inspect the rendered layout and package the build and source

## verification boundary

offline tests and compilation can verify calculations, transitions and API compatibility
they cannot certify live packet reception, native address resolution, client flag timing, performance in FFXIV or the appearance over the actual game
those remain explicitly marked for a live PotD smoke test before distribution

## research

ranges verified 2026-11-02 against the floor pages of [Deep Dungeon Compendium](https://www.ddcompendium.com/). Source URLs accompany the complete range table in `FLOOR_RANGES.md`

- [Activation randomness, Flight and clear-all exception](https://www.ddcompendium.com/knowledge.html)
- [Flight halves the next floor's requirement, rounded up](https://www.ddcompendium.com/pomanders.html)
- [Official PotD guide](https://na.finalfantasyxiv.com/lodestone/playguide/contentsguide/deepdungeon/)
- [Supported deep dungeon struct](https://github.com/aers/FFXIVClientStructs/blob/main/FFXIVClientStructs/FFXIV/Client/Game/InstanceContent/InstanceContentDeepDungeon.cs)
- [Supported actor-control callback](https://github.com/aers/FFXIVClientStructs/blob/main/FFXIVClientStructs/FFXIV/Client/Network/PacketDispatcher.cs)
- [Passage active encoding reference](https://github.com/awgil/ffxiv_bossmod/blob/master/BossMod/Data/DeepDungeonState.cs)
- [Game pomander-slot table](https://github.com/xivapi/ffxiv-datamining/blob/master/csv/en/DeepDungeon.csv)

the implementation is written for this task
public implementations were consulted to establish protocol and API facts, not incorporated as source code
