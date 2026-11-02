namespace PotdCounter.Core;

public enum CounterTone { Neutral, Ready, Attention }

// Select readable, truthful display content independently of the native drawing code.
public readonly record struct CounterPresentation(
    string Main, string Caption, string? Observed, string? Range, string Note,
    CounterTone Tone, bool IsEstimate, bool ShowFlight)
{
    public static CounterPresentation Create(int floor, int kills, KnownState here,
        bool passageOpen, bool countComplete, bool counterAvailable = true)
    {
        var estimate = PassageEstimator.Estimate(floor, kills, here, passageOpen, countComplete);
        var normalFloor = FloorRules.Kind(floor) == FloorKind.Normal;
        var observed = normalFloor && counterAvailable ? $"{Math.Max(0, kills)} kills observed" : null;
        var range = FloorRules.NormalRange(floor) is not { } target ? null : here switch
        {
            KnownState.On => $"estimated floor total: {target.WithFlight()} kills with Flight",
            KnownState.Off => $"estimated floor total: {target} kills",
            _ => $"floor total: {target} normal / {target.WithFlight()} with Flight",
        };
        var showFlight = floor is >= 1 and < 200;

        if (!counterAvailable && normalFloor && estimate.Kind != EstimateKind.Open)
            return new("tracking paused", "kill count unavailable", null, range,
                "the passage status can still be read, remaining estimates are paused",
                CounterTone.Attention, false, showFlight);

        var (main, caption, note, tone) = estimate.Kind switch
        {
            EstimateKind.Normal => ($"~{estimate.Remaining}", "estimated kills left",
                "each floor rolls its own target, clearing every enemy can open the passage early",
                CounterTone.Neutral),
            EstimateKind.Open => ("passage open", "ready for the next floor",
                "the game has confirmed the passage is open", CounterTone.Ready),
            EstimateKind.Partial => ("kills left unknown", "partial count",
                "earlier or distant party kills may be missing, the observed count is not a full floor total",
                CounterTone.Attention),
            EstimateKind.UnknownFlight => ("kills left unknown", "Flight not confirmed",
                "Flight on this floor is unknown until there is enough observed floor history",
                CounterTone.Attention),
            EstimateKind.RangeExceeded => ("check the passage", "past the estimated range",
                "the count has reached the published range, only the game can confirm an open passage",
                CounterTone.Attention),
            EstimateKind.Boss => ("boss floor", "no normal kill target",
                "the normal monster ranges do not apply on boss floors", CounterTone.Neutral),
            EstimateKind.Complete => ("you made it !!", "the climb is complete",
                "floor 200 is the completion floor", CounterTone.Ready),
            _ => ("waiting for PotD", "appears when your run starts",
                "the live counter follows Palace of the Dead", CounterTone.Neutral),
        };
        return new(main, caption, observed, range, note, tone,
            estimate.Kind == EstimateKind.Normal, showFlight);
    }

    public static string FlightLabel(KnownState value, bool nextFloor = false) => value switch
    {
        KnownState.On => nextFloor ? "queued" : "active",
        KnownState.Off => nextFloor ? "not queued" : "off",
        _ => "unknown",
    };
}
