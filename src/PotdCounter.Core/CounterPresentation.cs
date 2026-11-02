namespace PotdCounter.Core;

public enum CounterTone { Neutral, Ready, Attention, Active, Future }

public readonly record struct FlightBadge(string Status, string Detail, CounterTone Tone, bool TwoLines);

// Select readable, truthful display content independently of the native drawing code.
public readonly record struct CounterPresentation(
    string Main, string Caption, string? Observed, string? Range, string Note,
    CounterTone Tone, bool IsEstimate, bool ShowFlight, FlightBadge Flight,
    string? NextFlightNotice, CounterTone NextFlightTone, string? DestinationNotice)
{
    public static CounterPresentation Create(int floor, int kills, KnownState here,
        bool passageOpen, bool countComplete, bool counterAvailable = true, KnownState next = KnownState.Unknown)
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
        var badge = floor == 1 && here == KnownState.Off
            ? new FlightBadge("FIRST FLOOR:", "Flight Unavailable", CounterTone.Neutral, true)
            : new FlightBadge(here switch
                {
                    KnownState.On => "ACTIVE",
                    KnownState.Off => "INACTIVE",
                    _ => "UNKNOWN",
                }, "Flight", here switch
                {
                    KnownState.On => CounterTone.Active,
                    KnownState.Unknown => CounterTone.Attention,
                    _ => CounterTone.Neutral,
                }, false);
        // Completion cannot benefit from another Flight; retain the raw queue in settings.
        var nextNotice = floor is >= 1 and < 199 ? next switch
        {
            KnownState.On => "NEXT FLOOR: Flight activated",
            KnownState.Unknown => "NEXT FLOOR: Flight unknown",
            _ => null,
        } : null;
        var nextTone = next == KnownState.On ? CounterTone.Future : CounterTone.Attention;
        var destination = showFlight && FloorRules.Kind(floor + 1) == FloorKind.Boss
            ? "BOSS NEXT FLOOR" : floor == 199 ? "FINAL FLOOR NEXT" : null;

        CounterPresentation Present(string main, string caption, string note, CounterTone tone, bool isEstimate = false,
            bool includeObserved = true) => new(main, caption, includeObserved ? observed : null, range, note,
                tone, isEstimate, showFlight, badge, nextNotice, nextTone, destination);

        if (!counterAvailable && normalFloor && estimate.Kind != EstimateKind.Open)
            return Present("?", "tracking unavailable",
                "the passage status can still be read, remaining estimates are paused",
                CounterTone.Attention, includeObserved: false);

        var (main, caption, note, tone) = estimate.Kind switch
        {
            EstimateKind.Normal => ($"~{estimate.Remaining}", "kills left",
                "each floor rolls its own target, clearing every enemy can open the passage early",
                CounterTone.Neutral),
            EstimateKind.Open => ("OPEN", "passage",
                "the game has confirmed the passage is open", CounterTone.Ready),
            EstimateKind.Partial => ($"~{estimate.Remaining}", "kills left · partial",
                "earlier or distant kills may be missing, the range starts at one and only observed kills reduce its upper estimate",
                CounterTone.Attention),
            EstimateKind.UnknownFlight => ($"~{estimate.Remaining}", "kills left",
                "Flight on this floor is unconfirmed, this wider range includes both normal and Flight targets",
                CounterTone.Attention),
            EstimateKind.RangeExceeded => ("CHECK", "past the estimated range",
                "the count has reached the published range, only the game can confirm an open passage",
                CounterTone.Attention),
            EstimateKind.Boss => ("BOSS", FloorRules.BossName(floor) ?? "boss floor",
                "the normal monster ranges do not apply on boss floors", CounterTone.Neutral),
            EstimateKind.Complete => ("CLEAR", "you made it !!",
                "floor 200 is the completion floor", CounterTone.Ready),
            _ => ("WAIT", "waiting for PotD",
                "the live counter follows Palace of the Dead", CounterTone.Neutral),
        };
        return Present(main, caption, note, tone, estimate.Remaining is not null);
    }

    public static string FlightLabel(KnownState value, bool nextFloor = false) => value switch
    {
        KnownState.On => nextFloor ? "queued" : "active",
        KnownState.Off => nextFloor ? "not queued" : "inactive",
        _ => "unknown",
    };
}
