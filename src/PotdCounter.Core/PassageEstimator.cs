namespace PotdCounter.Core;

public enum EstimateKind { Waiting, Normal, Partial, UnknownFlight, RangeExceeded, Open, Boss, Complete }

public readonly record struct PassageEstimate(
    EstimateKind Kind, KillRange? NormalTarget = null, KillRange? FlightTarget = null,
    KillRange? Remaining = null)
{
    public string Headline => Kind switch
    {
        EstimateKind.Waiting => "waiting for PotD",
        EstimateKind.Open => "passage open",
        EstimateKind.Boss => "boss floor",
        EstimateKind.Complete => "floor 200 · you made it !!",
        EstimateKind.Normal => $"~{Remaining} more kills",
        EstimateKind.Partial => "remaining unknown · partial count",
        EstimateKind.UnknownFlight => "remaining unknown · Flight unconfirmed",
        EstimateKind.RangeExceeded => "past the usual range · check passage",
        _ => "waiting for data",
    };
}

public static class PassageEstimator
{
    public static PassageEstimate Estimate(FloorTracker tracker) => Estimate(
        tracker.Visible ? tracker.Floor : 0, tracker.Kills, tracker.FlightHere,
        tracker.PassageOpen, tracker.CountComplete);

    public static PassageEstimate Estimate(int floor, int kills, KnownState flight,
        bool passageOpen, bool countComplete)
    {
        var kind = FloorRules.Kind(floor);
        if (kind == FloorKind.Invalid) return new(EstimateKind.Waiting);
        if (kind == FloorKind.Complete) return new(EstimateKind.Complete);
        if (passageOpen) return new(EstimateKind.Open);
        if (kind == FloorKind.Boss) return new(EstimateKind.Boss);

        var normal = FloorRules.NormalRange(floor)!.Value;
        var withFlight = normal.WithFlight();
        if (!countComplete || kills < 0) return new(EstimateKind.Partial, normal, withFlight);
        if (flight == KnownState.Unknown) return new(EstimateKind.UnknownFlight, normal, withFlight);
        var target = flight == KnownState.On ? withFlight : normal;
        if (kills >= target.Max) return new(EstimateKind.RangeExceeded, normal, withFlight);

        // Closed passage rules out zero remaining, even after the published minimum.
        return new(EstimateKind.Normal, normal, withFlight,
            new(Math.Max(1, target.Min - kills), target.Max - kills));
    }
}
