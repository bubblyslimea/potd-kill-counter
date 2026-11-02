# Riri's PotD Counter · Asura design

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
mark party counts as partial and suppress numerical remaining estimates rather than misrepresenting them as a complete party total
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
6. the pure estimator returns an estimated remaining range only for a known Flight state and a sufficiently observed floor
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
