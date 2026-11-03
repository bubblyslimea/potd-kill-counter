namespace PotdCounter.Core;

// Separate from kill quotas: only the expected boss's observed death confirms victory.
public sealed class BossVictory
{
    public const long DurationMs = 4000;
    public const long FadeInMs = 180;
    private long instanceKey;
    private int floor;
    private long? shownAt;
    public bool Defeated { get; private set; }

    public void Follow(long instance, int currentFloor)
    {
        if (instanceKey == instance && floor == currentFloor) return;
        Reset();
        instanceKey = instance;
        floor = currentFloor;
    }

    public bool Observe(EnemyDeath death)
    {
        if (Defeated || instanceKey == 0 || death.InstanceKey != instanceKey || death.Floor != floor ||
            !death.IsEnemy || death.EntityId is 0 or 0xE0000000 ||
            FloorRules.BossBaseId(floor) is not { } expected || death.BaseId != expected)
            return false;
        Defeated = true;
        return true;
    }

    public void Advance(bool inCombat, long nowMs)
    {
        // Independent of window visibility: toggling the HUD cannot replay the moment.
        if (Defeated && !inCombat && shownAt is null) shownAt = nowMs;
    }

    public bool Celebrating(long nowMs) =>
        Defeated && (shownAt is null || nowMs - shownAt.Value < DurationMs);

    public float Opacity(long nowMs) => shownAt is { } start
        ? Math.Clamp((nowMs - start) / (float)FadeInMs, 0f, 1f) : 0f;

    public void Reset()
    {
        instanceKey = 0;
        floor = 0;
        Defeated = false;
        shownAt = null;
    }
}
