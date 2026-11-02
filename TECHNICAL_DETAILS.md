# a closer look

## this build

0.1.2.0 is a testing update, built for Dalamud API 15, .NET 10 and Windows x64
the release ZIP contains one plugin assembly, its manifest and dependency description, plus the license and acknowledgements
Dalamud supplies the host assemblies, they are not included in the download

the counter itself makes no network requests and sends no telemetry
GitHub and the shared catalog are used by the installer for downloads and updates, counting runs locally

## installing the alpha

1. open `/xlsettings`, then **Experimental → Custom Plugin Repositories**
2. add `https://give-riri-a-cookie.pages.dev/plugins/repo.json`, click **+** and **Save**
3. enable **Get plugin testing builds** in **Experimental**, save, then open `/xlplugins` and install **Riri's PotD Counter**

this release is testing-only, it may be hidden until testing builds are enabled
the existing `https://give-riri-a-cookie.pages.dev/notation/repo.json` address serves the same catalog, keep only one of these rows
the old Netlify site's availability depends on its own hosting state, use the Pages address for a new installation

if you used the earlier development DLL, disable it and remove its **Dev Plugin Locations** row before installing this version
keep one copy enabled, both use the `PotdCounter` identity

## what happens while you play

the reader checks the deep dungeon director every 200 ms and takes extra reads when death events arrive
it only follows Palace of the Dead (`DeepDungeonId == 1`), and automatically hides the live overlay elsewhere

each floor gets its own count and set of observed enemy IDs
the observer uses the supported FFXIVClientStructs address for `PacketDispatcher.HandleActorControlPacket`, category 6 identifies a death
known non-enemies and recorded events are excluded, the original callback runs once with unchanged arguments
managed observation errors are contained, a missing native address disables the observer

there are no full object-table scans in the update loop, individual actors are looked up when death events arrive
there are no per-kill or per-floor disk writes, only display preferences are saved
the plugin never selects a target, sends a game action or moves your character

## Flight here and next

DeepDungeonItem row 6 identifies Flight
its client flag describes a use queued for the next floor, an inventory decrease alone does not establish a use

once observed, the queue stays latched for the current floor so a flag clearing just before transition does not erase it
a witnessed consecutive floor change carries that queue into **Flight here**
the next queue gets 750 ms to settle, a persisting old flag must clear before a fresh use is accepted
the first alpha has a positive general live report, specific transition edge cases still need their own in-game checks

loading mid-floor leaves current Flight unknown
even a fresh set above floor 1 may inherit a Flight from the previous set, this version does not persist that history across separate instances
unknown becomes known after a sufficiently observed consecutive floor transition

## why the numbers are ranges

each floor rolls a hidden activation requirement, the tracker cannot read its exact target
the [floor table](FLOOR_RANGES.md) includes documented rare outcomes, including 3–9 on floors 41–49 and 91–99, and 4–10 on 101–149
Flight halves each bound and rounds up

while the passage is closed, an estimated remainder starts at at least one
passing the expected maximum asks you to check the passage, it never fabricates an open signal
the client's passage progress is treated as active at `>= 11`, following the linked client-state reference in [the design](DESIGN.md)
that value is not interpreted as a kill count or a percentage
the game's actual open state takes priority, including a full clear that opens early

loading late, reconnecting, skipped floors, a changed director, an unidentified death or unavailable reads can leave a partial count
distant party kills may never reach your client, party counts are therefore marked partial and the numeric remainder is suppressed
the floor's target ranges and actual passage state remain available
boss floors 10–190 have no normal quota, floor 200 is completion

## make it comfortable

| command | what it does |
| --- | --- |
| `/potd` | opens settings |
| `/potd toggle` | shows or hides the automatic overlay |
| `/potd preview` | tries a labelled simulation outside PotD |

`/potdcount` accepts the same commands for compatibility
starting a preview also makes a hidden overlay visible

drag the background while unlocked, right-click it to open settings
locking also enables click-through, `/potd` lets you unlock it again
**run details** in settings holds observed kills, the floor total and explanations, and works while the overlay is locked
the short command is registered only if available, a conflicting plugin command is left intact
Dalamud remembers the placement and the plugin saves display preferences
preview values never affect the live tracker

the fixed-size card leads with the estimated kills left, then a current Flight badge and an independent next-floor notice
lavender means active here, blue means activated for the next floor, neutral means inactive, and amber marks unknown states or the next boss
text carries the meaning as well as color
`OPEN` with a checkmark replaces the estimate only when the client confirms passage activation

`FIRST FLOOR:` / `Flight Unavailable` appears only on actual floor 1 with known inactive incoming Flight
it is not an inventory check, a use on floor 1 can still prepare floor 2
floors 11, 51 and 191 are not treated as floor 1, unknown remains unknown
a boss notice has a separate reserved line so it cannot hide an observed Flight queue
floor 199 points to the final floor, floor 200 shows `CLEAR` without another Flight destination

partial counts and unknown Flight show `?` with a visible reason
passing the estimated maximum shows `CHECK`, never a fabricated open passage
settings put detailed counts, explanations and reader status behind separate expanders

the managed font atlas bakes the headline at its actual size and rebuilds for Dalamud UI scaling
the small embedded headline font is a renamed subset of DejaVu Sans Bold, supporting text uses the host's font assets
there is no window-font enlargement, new runtime dependency or network request
font handles are created once and disposed on unload
the 320 × 232 logical-pixel card keeps the same layout across states, with an extra simulation banner above previews
saved placement, lock and opacity are retained, new installations default to 96% opacity

`PREVIEW.html` is an interactive layout study with the same 15 examples used by the native preview controls
the README visual is also a labelled layout preview, neither is an in-game capture
the native simulation command lets you inspect the actual overlay without entering PotD

## building it yourself

install .NET SDK 10.0.403 and the Dalamud development files for API 15
set `DALAMUD_HOME` to the directory containing `Dalamud.dll` if it is not in the SDK's normal location
the project pins `Dalamud.NET.Sdk/15.0.0` and includes a dependency lockfile

```powershell
dotnet run --project tests/PotdCounter.Tests -c Release
dotnet restore src/PotdCounter/PotdCounter.csproj --locked-mode
dotnet build src/PotdCounter/PotdCounter.csproj -c Release --no-restore
python scripts/package.py
```

the checks exercise all 200 floors, rare bounds, odd Flight rounding, duplicate and simultaneous deaths, stale events, consecutive Flight uses, floor changes, late attachment, party uncertainty, full clears, bosses and completion
compilation uses actual Dalamud assemblies, offline checks cannot validate live native events or performance
see [validation](VALIDATION.md) for that boundary and the in-game checklist

## publishing updates

GitHub holds the public source and permanent release ZIPs
the cookie site's Pages build combines Notation's own release entry with the shared catalog entries, then serves identical JSON at `/plugins/repo.json` and `/notation/repo.json`
the counter's entry points directly to its versioned GitHub Release ZIP, future download hosts can change inside the entry without changing the user's repository row

the GitHub workflow checks and packages main-branch builds, then publishes an alpha release only if that version does not already exist
existing assets are compared by checksum, a changed build needs a new version
the release includes `repo.json`, `PotdCounter.zip`, its checksum and a file-hash record
the catalog must only be updated after those downloads have been verified

the feed entry has `IsTestingExclusive: true`, with the real version in `TestingAssemblyVersion`
live testing should precede a stable promotion, public source and a downloadable alpha do not imply that in-game validation is complete

## finding your way around

| file | responsibility |
| --- | --- |
| `src/PotdCounter.Core/FloorRules.cs` | floor ranges and Flight rounding |
| `src/PotdCounter.Core/FloorTracker.cs` | counts, confidence and floor transitions |
| `src/PotdCounter.Core/PassageEstimator.cs` | estimates from observed facts |
| `src/PotdCounter.Core/CounterPresentation.cs` | main answer, supporting text and visible uncertainty |
| `src/PotdCounter/Game/PotdReader.cs` | client data adapter |
| `src/PotdCounter/Game/EnemyDeathObserver.cs` | death observations |
| `src/PotdCounter/UI/` | counter and settings windows |

the core has no Dalamud dependencies and is compiled directly into the plugin
HoH, EO, run history, personal averages and sound cues can wait until the PotD foundation has had its live checks

[Asura design](DESIGN.md) · [acknowledgements](THIRD_PARTY_NOTICES.md)
