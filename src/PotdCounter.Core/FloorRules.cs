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

    // English names verified against the linked floor guides in FLOOR_RANGES.md.
    public static string? BossName(int floor) => floor switch
    {
        10 => "Palace Deathgaze",
        20 => "Spurge",
        30 => "Ningishzida",
        40 => "Ixtab",
        50 => "Edda Blackbosom",
        60 => "The Black Rider",
        70 => "Yaquaru",
        80 => "Gudanna",
        90 => "The Godmother",
        100 => "Nybeth Obdilord",
        110 => "Alicanto",
        120 => "Kirtimukha",
        130 => "Alfard",
        140 => "Ah Puch",
        150 => "Tisiphone",
        160 => "Todesritter",
        170 => "Yulunggu",
        180 => "Dendainsonne",
        190 => "The Godfather",
        _ => null,
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
