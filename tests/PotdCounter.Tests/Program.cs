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
Check(PassageEstimator.Estimate(t).Kind == EstimateKind.Partial, "partial count suppresses remainder");
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
    "unknown Flight suppresses remaining estimate");
Check(PassageEstimator.Estimate(190, 0, KnownState.On, false, true).Kind == EstimateKind.Boss,
    "boss has no normal quota");
Check(PassageEstimator.Estimate(200, 0, KnownState.On, true, true).Kind == EstimateKind.Complete,
    "completion wins at 200");

Console.WriteLine($"PASS: {checks} assertions; all 200 floors, Flight, deaths, transitions, uncertainty and passage override.");

// Optional review fixtures are produced by the actual estimator, not a JavaScript copy.
if (args.Length == 2 && args[0] == "--export-preview")
{
    var scenes = new[]
    {
        (Id: "normal", Label: "Normal floor", Floor: 193, Kills: 4, Here: KnownState.Off, Next: KnownState.On, Open: false, Full: true),
        (Id: "flight", Label: "Flight active", Floor: 193, Kills: 4, Here: KnownState.On, Next: KnownState.Off, Open: false, Full: true),
        (Id: "open", Label: "Passage open", Floor: 193, Kills: 7, Here: KnownState.Off, Next: KnownState.On, Open: true, Full: true),
        (Id: "partial", Label: "Joined mid-floor", Floor: 193, Kills: 3, Here: KnownState.Unknown, Next: KnownState.Off, Open: false, Full: false),
        (Id: "boss", Label: "Boss floor", Floor: 190, Kills: 0, Here: KnownState.Off, Next: KnownState.Off, Open: false, Full: true),
        (Id: "complete", Label: "Floor 200", Floor: 200, Kills: 0, Here: KnownState.Off, Next: KnownState.Off, Open: true, Full: true),
    }.Select(s => new
    {
        id = s.Id, label = s.Label, floor = s.Floor, kills = s.Kills,
        here = s.Here.ToString(), next = s.Next.ToString(), full = s.Full,
        normalFloor = FloorRules.Kind(s.Floor) == FloorKind.Normal,
        headline = PassageEstimator.Estimate(s.Floor, s.Kills, s.Here, s.Open, s.Full).Headline,
        kind = PassageEstimator.Estimate(s.Floor, s.Kills, s.Here, s.Open, s.Full).Kind.ToString(),
        normal = FloorRules.NormalRange(s.Floor)?.ToString(),
        flight = FloorRules.NormalRange(s.Floor)?.WithFlight().ToString(),
    });
    File.WriteAllText(args[1], System.Text.Json.JsonSerializer.Serialize(scenes));
}
