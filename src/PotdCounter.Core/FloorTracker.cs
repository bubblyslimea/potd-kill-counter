namespace PotdCounter.Core;

public readonly record struct FloorSnapshot(
    long InstanceKey, int Floor, byte PassageProgress, KnownState FlightQueued, int PartySize);

public readonly record struct EnemyDeath(long InstanceKey, int Floor, uint EntityId, bool IsEnemy, uint BaseId = 0);

/// <summary>Managed floor state. No native memory, UI, files or network access.</summary>
public sealed class FloorTracker
{
    private readonly HashSet<uint> counted = [];
    private long queueReadyAt;
    private byte lastProgress;
    private bool waitingForOldQueueToClear;

    public bool HasFloor { get; private set; }
    public bool Visible { get; private set; }
    public long InstanceKey { get; private set; }
    public int Floor { get; private set; }
    public int Kills { get; private set; }
    public bool CountComplete { get; private set; }
    public bool PassageOpen { get; private set; }
    public KnownState FlightHere { get; private set; } = KnownState.Unknown;
    public KnownState FlightNext { get; private set; } = KnownState.Unknown;
    public string ConfidenceNote { get; private set; } = "waiting for PotD";

    public void Apply(FloorSnapshot snapshot, long nowMs, bool observedEntrance = false)
    {
        if (FloorRules.Kind(snapshot.Floor) == FloorKind.Invalid)
        {
            Suspend("waiting for floor data");
            return;
        }

        var newFloor = !HasFloor || InstanceKey != snapshot.InstanceKey || Floor != snapshot.Floor;
        if (newFloor)
        {
            var consecutive = HasFloor && InstanceKey == snapshot.InstanceKey && snapshot.Floor == Floor + 1;
            var inheritedFlight = consecutive ? FlightNext : KnownState.Unknown;
            // Floor 1 cannot inherit Flight, even if we missed the entrance or earlier kills.
            FlightHere = snapshot.Floor == 1 ? KnownState.Off : inheritedFlight;
            CountComplete = consecutive || observedEntrance;
            ConfidenceNote = CountComplete ? "counting since you entered this floor" : "joined mid-floor, earlier kills may be missing";
            InstanceKey = snapshot.InstanceKey;
            Floor = snapshot.Floor;
            Kills = 0;
            counted.Clear();
            HasFloor = true;
            // During zoning, the new floor number and old item flags may briefly coexist.
            // Do not call the old flag a new use. New-floor queue data settles for 750 ms.
            queueReadyAt = consecutive ? nowMs + 750 : nowMs;
            waitingForOldQueueToClear = consecutive && inheritedFlight == KnownState.On &&
                snapshot.FlightQueued == KnownState.On;
            FlightNext = consecutive ? KnownState.Unknown : snapshot.FlightQueued;
        }
        else if (snapshot.PassageProgress < lastProgress)
        {
            // A reset or partially refreshed director cannot establish a fresh complete count.
            MarkIncomplete("passage data reset, a kill may be missing");
        }

        Visible = true;
        lastProgress = snapshot.PassageProgress;
        PassageOpen = snapshot.PassageProgress >= 11;
        if (snapshot.FlightQueued == KnownState.Off)
            waitingForOldQueueToClear = false;
        // Once a use is observed, it cannot be undone on the same floor. Latch it so
        // flags clearing a moment before the floor number changes cannot lose Flight.
        if (nowMs >= queueReadyAt && !waitingForOldQueueToClear && FlightNext != KnownState.On)
            FlightNext = snapshot.FlightQueued;
        if (snapshot.PartySize > 1)
            MarkIncomplete("party view, distant kills may be missing");
    }

    public bool Observe(EnemyDeath death)
    {
        if (!HasFloor || !Visible || death.InstanceKey != InstanceKey || death.Floor != Floor ||
            !death.IsEnemy || death.EntityId is 0 or 0xE0000000 ||
            FloorRules.Kind(Floor) != FloorKind.Normal || !counted.Add(death.EntityId))
            return false;
        Kills++;
        return true;
    }

    public void MarkIncomplete(string reason)
    {
        CountComplete = false;
        ConfidenceNote = reason;
    }

    public void Suspend(string reason)
    {
        Visible = false;
        if (HasFloor)
            MarkIncomplete(reason);
    }

    public void Leave()
    {
        HasFloor = Visible = PassageOpen = CountComplete = false;
        InstanceKey = 0;
        Floor = Kills = 0;
        lastProgress = 0;
        queueReadyAt = 0;
        waitingForOldQueueToClear = false;
        FlightHere = FlightNext = KnownState.Unknown;
        ConfidenceNote = "waiting for PotD";
        counted.Clear();
    }
}
