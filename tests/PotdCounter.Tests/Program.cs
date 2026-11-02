using PotdCounter.Core;

var checks = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new Exception($"FAIL: {name}");
    checks++;
}

FloorSnapshot S(int floor, KnownState next = KnownState.Off, byte progress = 0,
    int party = 1, long key = 1) => new(key, floor, progress, next, party);
void Kill(FloorTracker t, uint id) => t.Observe(new(t.InstanceKey, t.Floor, id, true));

// Every floor is accounted for; bosses/completion never receive an ordinary quota.
for (var f = 1; f <= 200; f++)
{
    var range = FloorRules.NormalRange(f);
    Check((range is null) == (f % 10 == 0), $"quota availability on {f}");
    if (range is not { } r) continue;
    Check(r.Min > 0 && r.Max >= r.Min, $"valid range on {f}");
    var flight = r.WithFlight();
    Check(flight.Min == (int)Math.Ceiling(r.Min / 2d) &&
          flight.Max == (int)Math.Ceiling(r.Max / 2d), $"Flight rounding on {f}");
    for (var k = 0; k < r.Max; k++)
    {
        var e = PassageEstimator.Estimate(f, k, KnownState.Off, false, true);
        Check(e.Remaining is { Min: >= 1 } left && left.Max >= left.Min,
            $"closed passage has a positive possible remainder: {f}, {k}");
    }
}
Check(FloorRules.UsualRange(41) == new KillRange(4, 8), "41 usual range");
Check(FloorRules.NormalRange(41) == new KillRange(3, 9), "41 includes rare outcomes");
Check(FloorRules.NormalRange(91) == new KillRange(3, 9), "91 includes rare outcomes");
Check(FloorRules.UsualRange(101) == new KillRange(4, 9), "101 usual range");
Check(FloorRules.NormalRange(101) == new KillRange(4, 10), "101 includes rare 10");
Check(FloorRules.NormalRange(151) == new KillRange(4, 10), "151 range");
Check(FloorRules.NormalRange(191) == new KillRange(5, 13), "191 range");
Check(FloorRules.NormalRange(191)!.Value.WithFlight() == new KillRange(3, 7), "191 Flight range");
Check(FloorRules.Kind(200) == FloorKind.Complete, "200 is completion, not boss");
Check(FloorRules.Kind(0) == FloorKind.Invalid && FloorRules.Kind(201) == FloorKind.Invalid,
    "invalid floors guarded");

var t = new FloorTracker();
t.Apply(S(191, KnownState.On), 0); // Hot-load on a floor with Flight queued.
Check(!t.CountComplete && t.FlightHere == KnownState.Unknown, "late attachment is uncertain");
Check(t.FlightNext == KnownState.On, "client queue observed on attachment");
Kill(t, 123);
Kill(t, 123);
Check(t.Kills == 1, "duplicate death counted once");
Check(!t.Observe(new(1, 191, 124, false)), "non-enemy excluded");
Check(!t.Observe(new(2, 191, 124, true)), "different instance excluded");
Check(!t.Observe(new(1, 190, 124, true)), "stale floor excluded");
Check(!t.Observe(new(1, 191, 0xE0000000, true)), "sentinel actor excluded");
Check(PassageEstimator.Estimate(t).Kind == EstimateKind.Partial, "partial count retains its uncertainty state");
t.Apply(S(191, KnownState.Off), 900);
Check(t.FlightNext == KnownState.On, "queued Flight survives flag clearing before floor number changes");

t.Apply(S(192, KnownState.On), 1000); // Transitional old queue flag.
Check(t.Kills == 0 && t.CountComplete, "witnessed next floor resets count");
Check(t.FlightHere == KnownState.On, "Flight carries to exactly next floor");
Check(t.FlightNext == KnownState.Unknown, "new-floor queue waits for settling");
t.Apply(S(192, KnownState.Off), 1400);
Check(t.FlightNext == KnownState.Unknown, "settling period respected");
t.Apply(S(192, KnownState.Off), 1800);
Check(t.FlightNext == KnownState.Off, "new-floor queue read after settling");
Kill(t, 123); // A prior floor's ID must be allowed after floor reset.
Kill(t, 124);
Kill(t, 125);
Check(t.Kills == 3, "multiple enemies, no double multiplication under Flight");
Check(PassageEstimator.Estimate(t).Remaining == new KillRange(1, 4), "Flight remaining range");
t.Apply(S(192, KnownState.On), 2000);
Check(t.FlightHere == KnownState.On && t.FlightNext == KnownState.On, "active and queued Flight coexist");
t.Apply(S(193, KnownState.Off), 3000);
Check(t.FlightHere == KnownState.On, "successive Flight carried");
t.Apply(S(193, KnownState.Off), 3800);
t.Apply(S(194), 4000);
Check(t.FlightHere == KnownState.Off, "Flight expires after one floor");
Check(PassageEstimator.Estimate(t).Remaining == new KillRange(5, 13), "normal target after Flight expires");
t.Apply(S(196), 5000);
Check(t.FlightHere == KnownState.Unknown && !t.CountComplete, "skipped floors lose continuity");

var staleFlag = new FloorTracker();
staleFlag.Apply(S(191, KnownState.On), 0);
staleFlag.Apply(S(192, KnownState.On), 1000);
staleFlag.Apply(S(192, KnownState.On), 2000);
Check(staleFlag.FlightHere == KnownState.On && staleFlag.FlightNext == KnownState.Unknown,
    "a persistently stale queue flag is not mistaken for a second Flight use");
staleFlag.Apply(S(192, KnownState.Off), 2200);
Check(staleFlag.FlightNext == KnownState.Off, "old queue clear establishes next floor baseline");
staleFlag.Apply(S(192, KnownState.On), 2400);
Check(staleFlag.FlightNext == KnownState.On, "fresh queue after baseline is accepted");

var fresh = new FloorTracker();
fresh.Apply(S(1), 0, observedEntrance: true);
Check(fresh.CountComplete && fresh.FlightHere == KnownState.Off, "fresh floor 1 known");
fresh.Apply(S(1, progress: 11), 100);
Check(PassageEstimator.Estimate(fresh).Kind == EstimateKind.Open, "early clear-all overrides minimum");
fresh.Apply(S(1, progress: 12), 150);
Check(PassageEstimator.Estimate(fresh).Kind == EstimateKind.Open, "active progress encoding at least 11");
fresh.Leave();
Check(!fresh.Visible && fresh.Kills == 0 && fresh.FlightNext == KnownState.Unknown, "exit discards stale state");
fresh.Apply(S(1), 200);
Check(!fresh.CountComplete, "same-floor re-entry not assumed fresh");
fresh.Suspend("Reconnect");
Check(!fresh.Visible && !fresh.Observe(new(1, 1, 999, true)), "suspended events ignored");

var partyTracker = new FloorTracker();
partyTracker.Apply(S(191, party: 2), 0, observedEntrance: true);
Check(!partyTracker.CountComplete, "party coverage is qualified");
partyTracker.Apply(S(192, party: 2), 1000);
Check(!partyTracker.CountComplete, "new party floor stays qualified");

Check(PassageEstimator.Estimate(193, 4, KnownState.Off, false, true).Remaining == new KillRange(1, 9),
    "floor 193 example");
Check(PassageEstimator.Estimate(193, 13, KnownState.Off, false, true).Kind == EstimateKind.RangeExceeded,
    "maximum never implies passage is open");
Check(PassageEstimator.Estimate(193, 100, KnownState.Unknown, true, false).Kind == EstimateKind.Open,
    "game open overrides uncertain count and Flight");
Check(PassageEstimator.Estimate(193, 4, KnownState.Unknown, false, true).Kind == EstimateKind.UnknownFlight,
    "unknown Flight retains its uncertainty state");
Check(PassageEstimator.Estimate(190, 0, KnownState.On, false, true).Kind == EstimateKind.Boss,
    "boss has no normal quota");
Check(PassageEstimator.Estimate(200, 0, KnownState.On, true, true).Kind == EstimateKind.Complete,
    "completion wins at 200");

// The compact view must never hide uncertainty or manufacture progress.
var view = CounterPresentation.Create(193, 4, KnownState.Off, false, true);
Check(view.IsEstimate && view.Main == "~1–9" && view.Caption == "kills left",
    "normal card separates an approximate remaining range from its unit");
Check(view.Observed == "4 kills observed" && view.Range!.Contains("5–13"),
    "observed kills stay distinct from the total target");
view = CounterPresentation.Create(193, 4, KnownState.On, false, true);
Check(view.Main == "~1–3" && view.Range!.Contains("3–7"), "active Flight adjusts displayed ranges");
view = CounterPresentation.Create(193, 3, KnownState.Unknown, false, false);
Check(view.IsEstimate && view.Main == "~1–10" && view.Caption == "kills left · partial" && view.Tone == CounterTone.Attention,
    "partial counts remain visibly qualified with details closed");
view = CounterPresentation.Create(193, 4, KnownState.Unknown, false, true);
Check(view.IsEstimate && view.Main == "~1–9" && view.Flight.Status == "UNKNOWN" && view.Tone == CounterTone.Attention,
    "unknown Flight widens the estimate without claiming inactive");
Check(CounterPresentation.FlightLabel(KnownState.On) == "active" &&
      CounterPresentation.FlightLabel(KnownState.On, true) == "queued" &&
      CounterPresentation.FlightLabel(KnownState.Unknown, true) == "unknown",
    "Flight wording distinguishes current, next and unknown states");
view = CounterPresentation.Create(193, 13, KnownState.Off, false, true);
Check(!view.IsEstimate && view.Main == "CHECK" && view.Tone == CounterTone.Attention,
    "the range ceiling never presents an open passage");
view = CounterPresentation.Create(193, 4, KnownState.Off, false, true, false);
Check(!view.IsEstimate && view.Observed is null && view.Main == "?" && view.Caption == "tracking unavailable",
    "unavailable observer cannot present a trustworthy count or remainder");
foreach (var available in new[] { false, true })
foreach (var complete in new[] { false, true })
{
    view = CounterPresentation.Create(193, 100, KnownState.Unknown, true, complete, available);
    Check(view.Main == "OPEN" && view.Tone == CounterTone.Ready && !view.IsEstimate,
        "confirmed passage wins over every observer and confidence state");
}
view = CounterPresentation.Create(190, 0, KnownState.On, false, true, false);
Check(view.Main == "BOSS" && view.Observed is null && view.Range is null,
    "boss floors retain their own state even when counting is unavailable");
view = CounterPresentation.Create(200, 0, KnownState.Unknown, true, false, false);
Check(view.Tone == CounterTone.Ready && !view.ShowFlight && view.Range is null,
    "completion has no fictional next floor or monster target");
view = CounterPresentation.Create(0, 0, KnownState.Unknown, false, false);
Check(!view.ShowFlight && !view.IsEstimate && view.Observed is null, "outside view has no stale floor facts");

// Explicit boundary expectations guard the meaning of the new badges and notices.
var boundaries = new[]
{
    (Floor: 0, Show: false, First: false, Destination: (string?)null),
    (Floor: 1, Show: true, First: true, Destination: (string?)null),
    (Floor: 2, Show: true, First: false, Destination: (string?)null),
    (Floor: 9, Show: true, First: false, Destination: (string?)"BOSS NEXT FLOOR"),
    (Floor: 10, Show: true, First: false, Destination: (string?)null),
    (Floor: 11, Show: true, First: false, Destination: (string?)null),
    (Floor: 19, Show: true, First: false, Destination: (string?)"BOSS NEXT FLOOR"),
    (Floor: 51, Show: true, First: false, Destination: (string?)null),
    (Floor: 189, Show: true, First: false, Destination: (string?)"BOSS NEXT FLOOR"),
    (Floor: 190, Show: true, First: false, Destination: (string?)null),
    (Floor: 191, Show: true, First: false, Destination: (string?)null),
    (Floor: 199, Show: true, First: false, Destination: (string?)"FINAL FLOOR NEXT"),
    (Floor: 200, Show: false, First: false, Destination: (string?)null),
};
foreach (var expected in boundaries)
{
    view = CounterPresentation.Create(expected.Floor, 0, KnownState.Off, false, true, next: KnownState.Off);
    Check(view.ShowFlight == expected.Show && view.Flight.TwoLines == expected.First &&
          view.DestinationNotice == expected.Destination, $"HUD floor boundary {expected.Floor}");
}
var currentStates = new[]
{
    (State: KnownState.Off, Text: "INACTIVE", Tone: CounterTone.Neutral),
    (State: KnownState.On, Text: "ACTIVE", Tone: CounterTone.Active),
    (State: KnownState.Unknown, Text: "UNKNOWN", Tone: CounterTone.Attention),
};
var nextStates = new[]
{
    (State: KnownState.Off, Text: (string?)null, Tone: CounterTone.Attention),
    (State: KnownState.On, Text: (string?)"NEXT FLOOR: Flight activated", Tone: CounterTone.Future),
    (State: KnownState.Unknown, Text: (string?)"NEXT FLOOR: Flight unknown", Tone: CounterTone.Attention),
};
foreach (var here in currentStates)
foreach (var next in nextStates)
{
    view = CounterPresentation.Create(193, 0, here.State, false, true, next: next.State);
    Check(view.Flight.Status == here.Text && view.Flight.Tone == here.Tone &&
          view.NextFlightNotice == next.Text && view.NextFlightTone == next.Tone,
        "current and next Flight retain independent text and colors");
}
view = CounterPresentation.Create(1, 0, KnownState.Unknown, false, false);
Check(view.Flight.Status == "UNKNOWN" && !view.Flight.TwoLines && view.Main == "~1–7",
    "unknown preview input keeps an explicit badge and conservative range");
view = CounterPresentation.Create(200, 0, KnownState.On, true, true, next: KnownState.On);
Check(view.Main == "CLEAR" && view.NextFlightNotice is null, "completion has no queued Flight destination");
view = CounterPresentation.Create(1, 0, KnownState.Off, false, true, next: KnownState.On);
Check(view.Flight.Status == "FIRST FLOOR:" && view.Flight.Detail == "Flight Unavailable" &&
      view.NextFlightNotice == "NEXT FLOOR: Flight activated", "floor 1 can queue a pomander without an incoming effect");
view = CounterPresentation.Create(189, 2, KnownState.On, false, true, next: KnownState.On);
Check(view.Flight.Tone == CounterTone.Active && view.NextFlightTone == CounterTone.Future &&
      view.NextFlightNotice is not null && view.DestinationNotice == "BOSS NEXT FLOOR",
    "active, queued and boss information coexist");
foreach (var example in CounterExamples.All)
{
    view = example.State.Presentation;
    Check(view.Main.Length <= 5, "headline stays glanceable");
    Check(view.Caption.Length <= 24, "visible caveat fits the caption band");
    Check(!view.IsEstimate || view.Main.StartsWith('~'), "numeric estimates always approximate");
}

// Reproduce the live starting-floor paths through the actual tracker and view.
var opening = new FloorTracker();
opening.Apply(S(1), 0, observedEntrance: true);
view = CounterPresentation.Create(opening.Floor, opening.Kills, opening.FlightHere, opening.PassageOpen, opening.CountComplete);
Check(view.Main == "~3–7" && view.Caption == "kills left" && view.Flight.TwoLines,
    "fresh floor 1 keeps its kills left above the unavailable Flight badge");
Kill(opening, 70);
Check(PassageEstimator.Estimate(opening).Remaining == new KillRange(2, 6), "first-floor estimate decreases after a kill");
opening.Leave();
opening.Apply(S(1), 500); // Entrance missed or plugin reloaded mid-floor.
Check(opening.FlightHere == KnownState.Off && !opening.CountComplete,
    "floor 1 cannot inherit Flight, but missed kills remain unknown");
view = CounterPresentation.Create(opening.Floor, opening.Kills, opening.FlightHere, opening.PassageOpen, opening.CountComplete);
Check(view.Main == "~1–7" && view.Caption == "kills left · partial" && view.Flight.TwoLines,
    "late first-floor attachment still has a qualified numeric estimate");
opening.Leave();
opening.Apply(S(161), 1000, observedEntrance: true);
view = CounterPresentation.Create(opening.Floor, opening.Kills, opening.FlightHere, opening.PassageOpen, opening.CountComplete);
Check(view.Main == "~2–10" && view.Flight.Status == "UNKNOWN" && !view.Flight.TwoLines,
    "later set start covers either incoming Flight outcome without being called floor 1");

// A conservative range must contain every compatible hidden quota / missing-kill outcome.
foreach (var floor in new[] { 1, 191 })
foreach (var flight in Enum.GetValues<KnownState>())
foreach (var full in new[] { false, true })
{
    const int observed = 1;
    var estimate = PassageEstimator.Estimate(floor, observed, flight, false, full);
    var possibleFlight = flight == KnownState.Unknown ? new[] { KnownState.Off, KnownState.On } : [flight];
    var outcomes = new List<int>();
    foreach (var actualFlight in possibleFlight)
    {
        var target = FloorRules.NormalRange(floor)!.Value;
        if (actualFlight == KnownState.On) target = target.WithFlight();
        for (var quota = target.Min; quota <= target.Max; quota++)
        for (var actualKills = observed; actualKills <= (full ? observed : quota - 1); actualKills++)
            if (actualKills < quota) outcomes.Add(quota - actualKills);
    }
    Check(estimate.Remaining is { Min: >= 1 } bounds && outcomes.All(n => n >= bounds.Min && n <= bounds.Max),
        "remaining estimate includes all compatible hidden outcomes");
}
Check(PassageEstimator.Estimate(193, 13, KnownState.Unknown, false, false).Kind == EstimateKind.RangeExceeded,
    "partial and unknown states cannot invent a numeric remainder beyond the maximum");
Check(PassageEstimator.Estimate(1, -1, KnownState.Off, false, true).Remaining == new KillRange(1, 7),
    "invalid negative counts are treated as partial without inflating the maximum");

// Boss identity belongs to the floor, independent of targets, kill tracking or Flight.
Check(FloorRules.BossName(10) == "Palace Deathgaze" && FloorRules.BossName(50) == "Edda Blackbosom" &&
      FloorRules.BossName(100) == "Nybeth Obdilord" && FloorRules.BossName(180) == "Dendainsonne" &&
      FloorRules.BossName(190) == "The Godfather", "named early, story and deep bosses");
var namedBosses = Enumerable.Range(1, 200).Where(f => FloorRules.BossName(f) is not null).ToArray();
Check(namedBosses.SequenceEqual(Enumerable.Range(1, 19).Select(i => i * 10)),
    "exactly 19 boss floors have names, completion does not");
foreach (var floor in namedBosses)
{
    view = CounterPresentation.Create(floor, 0, KnownState.Unknown, false, false, false);
    Check(view.Main == "BOSS" && view.Caption == FloorRules.BossName(floor) && view.Range is null,
        "boss name appears below BOSS even when ordinary counting is unavailable");
}
var beforeBoss = new CounterDisplay(180, 0, KnownState.Off, KnownState.Off);
Check(!beforeBoss.HideForBossCombat && beforeBoss.Presentation.Caption == "Dendainsonne",
    "boss card is visible before the pull");
var duringBoss = beforeBoss with { InCombat = true };
Check(duringBoss.HideForBossCombat, "boss combat hides the entire HUD");
Check((duringBoss with { Open = true, Available = false }).HideForBossCombat,
    "boss HUD stays hidden until combat ends even if the passage opens first");
var afterBoss = duringBoss with { InCombat = false, Open = true };
Check(!afterBoss.HideForBossCombat && afterBoss.Presentation.Main == "OPEN",
    "ending combat restores the confirmed passage card");
Check(!(duringBoss with { InCombat = false }).HideForBossCombat,
    "combat cancellation restores the boss card without a permanent visibility latch");
foreach (var floor in new[] { 1, 9, 11, 179, 181, 193, 199, 200 })
    Check(!(duringBoss with { Floor = floor }).HideForBossCombat,
        "ordinary combat and completion never trigger boss-only hiding");

Console.WriteLine($"PASS: {checks} assertions; all 200 floors, Flight, deaths, transitions, uncertainty, passage priority and presentation.");

// Shared examples drive native preview controls and the external labelled studies.
if (args.Length == 2 && args[0] == "--export-preview")
{
    var scenes = CounterExamples.All.Select(s => new
    {
        id = s.Id, label = s.Label, floor = s.State.Floor, hidden = s.State.HideForBossCombat,
        view = s.State.Presentation,
    });
    File.WriteAllText(args[1], System.Text.Json.JsonSerializer.Serialize(scenes));
}
