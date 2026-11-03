namespace PotdCounter.Core;

// A snapshot shared by the HUD and settings; preview data never touches the tracker.
public readonly record struct CounterDisplay(int Floor, int Kills, KnownState Here, KnownState Next,
    bool Open = false, bool Complete = true, bool Available = true, bool InCombat = false,
    bool BossDefeated = false, bool CelebrateVictory = false, float VictoryOpacity = 1f)
{
    public CounterPresentation Presentation => CounterPresentation.Create(Floor, Kills, Here, Open, Complete, Available, Next,
        BossDefeated, CelebrateVictory);
    public bool HideForBossCombat => InCombat && FloorRules.Kind(Floor) == FloorKind.Boss;
}

public readonly record struct CounterExample(string Id, string Label, CounterDisplay State);

public static class CounterExamples
{
    public static IReadOnlyList<CounterExample> All { get; } = Array.AsReadOnly<CounterExample>([
        new("normal", "Flight ready for next floor", new(193, 4, KnownState.Off, KnownState.On)),
        new("first", "first floor", new(1, 0, KnownState.Off, KnownState.Off)),
        new("first-partial", "first floor, joined late", new(1, 0, KnownState.Off, KnownState.Off, Complete: false)),
        new("set-start", "later starting floor", new(161, 0, KnownState.Unknown, KnownState.Off)),
        new("flight", "Flight active", new(193, 4, KnownState.On, KnownState.Off)),
        new("both", "active here, next floor prepared", new(193, 4, KnownState.On, KnownState.On)),
        new("boss-next", "boss ahead", new(189, 2, KnownState.On, KnownState.Off)),
        new("open", "passage open", new(193, 7, KnownState.Off, KnownState.Off, Open: true)),
        new("inactive", "Flight inactive", new(193, 4, KnownState.Off, KnownState.Off)),
        new("partial", "partial count", new(193, 3, KnownState.Unknown, KnownState.Off, Complete: false)),
        new("unknown", "unknown Flight", new(193, 4, KnownState.Unknown, KnownState.Unknown)),
        new("exceeded", "range exceeded", new(193, 13, KnownState.Off, KnownState.Off)),
        new("paused", "tracking unavailable", new(193, 0, KnownState.Off, KnownState.Off, Available: false)),
        new("boss", "boss · The Godfather", new(190, 0, KnownState.Off, KnownState.Off)),
        new("boss-before", "boss before pull", new(180, 0, KnownState.Off, KnownState.Off)),
        new("boss-combat", "boss fight · HUD hidden", new(180, 0, KnownState.Off, KnownState.Off, InCombat: true)),
        new("boss-victory", "boss victory", new(180, 0, KnownState.Off, KnownState.Off, Open: true, BossDefeated: true, CelebrateVictory: true)),
        new("boss-victory-long", "victory · two-line name", new(50, 0, KnownState.Off, KnownState.Off, Open: true, BossDefeated: true, CelebrateVictory: true)),
        new("boss-victory-wait", "victory · passage pending", new(180, 0, KnownState.Off, KnownState.Off, BossDefeated: true)),
        new("boss-defeated", "after the victory card", new(180, 0, KnownState.Off, KnownState.Off, Open: true, BossDefeated: true)),
        new("final-next", "final floor ahead", new(199, 4, KnownState.On, KnownState.Off)),
        new("complete", "floor 200", new(200, 0, KnownState.Off, KnownState.Off, Open: true)),
        new("boss-queued", "boss ahead with Flight observed", new(189, 2, KnownState.On, KnownState.On)),
    ]);
}
