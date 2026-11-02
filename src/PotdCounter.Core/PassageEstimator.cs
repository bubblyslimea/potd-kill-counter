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
        EstimateKind.Partial => $"~{Remaining} more kills · partial count",
        EstimateKind.UnknownFlight => $"~{Remaining} more kills · Flight unconfirmed",
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
        var target = flight switch
        {
            KnownState.On => withFlight,
            KnownState.Off => normal,
            _ => new KillRange(withFlight.Min, normal.Max),
        };
        var observed = Math.Max(0, kills);
        if (observed >= target.Max) return new(EstimateKind.RangeExceeded, normal, withFlight);

        var partial = !countComplete || kills < 0;
        var estimateKind = partial ? EstimateKind.Partial :
            flight == KnownState.Unknown ? EstimateKind.UnknownFlight : EstimateKind.Normal;

        // Closed passage rules out zero remaining, even after the published minimum.
        // Missing kills can bring us closer than the observed count suggests, but cannot
        // increase the maximum. Unknown Flight must cover both possible target ranges.
        return new(estimateKind, normal, withFlight,
            new(partial ? 1 : Math.Max(1, target.Min - observed), target.Max - observed));
    }
}
