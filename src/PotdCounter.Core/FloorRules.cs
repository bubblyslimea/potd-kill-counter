namespace PotdCounter.Core;

public enum KnownState { Unknown, Off, On }
public enum FloorKind { Invalid, Normal, Boss, Complete }

public readonly record struct KillRange(int Min, int Max)
{
    public KillRange WithFlight() => new((Min + 1) / 2, (Max + 1) / 2);
    public override string ToString() => Min == Max ? Min.ToString() : $"{Min}–{Max}";
}

public static class FloorRules
{
    // Deep Dungeon Compendium, verified 2026-11-02. See FLOOR_RANGES.md.
    private static readonly KillRange[] Ranges =
    [
        new(3, 7), new(3, 7), new(3, 7), new(3, 7), new(4, 8),
        new(3, 7), new(3, 7), new(3, 7), new(3, 7), new(4, 8),
        new(4, 9), new(4, 9), new(4, 9), new(4, 9), new(4, 9),
        new(4, 10), new(4, 10), new(4, 10), new(4, 10), new(5, 13),
    ];

    public static FloorKind Kind(int floor) => floor switch
    {
        < 1 or > 200 => FloorKind.Invalid,
        200 => FloorKind.Complete,
        _ when floor % 10 == 0 => FloorKind.Boss,
        _ => FloorKind.Normal,
    };

    public static KillRange? UsualRange(int floor) =>
        Kind(floor) == FloorKind.Normal ? Ranges[(floor - 1) / 10] : null;

    // Retain documented rare outcomes in the estimator's envelope.
    public static KillRange? NormalRange(int floor) => Kind(floor) != FloorKind.Normal ? null : floor switch
    {
        >= 41 and <= 49 or >= 91 and <= 99 => new(3, 9),
        >= 101 and <= 149 => new(4, 10),
        _ => UsualRange(floor),
    };
}
