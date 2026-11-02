using PotdCounter.Core;

namespace PotdCounter.UI;

internal sealed class PreviewState
{
    public int Floor = 193;
    public int Kills = 4;
    public KnownState Flight = KnownState.Off;
    public KnownState Queued = KnownState.On;
    public bool PassageOpen;
    public bool Partial;
    public bool Unavailable;

    public CounterDisplay Snapshot => new(Floor, Kills, Flight, Queued, PassageOpen, !Partial, !Unavailable);

    public void Use(CounterDisplay state)
    {
        Floor = state.Floor;
        Kills = state.Kills;
        Flight = state.Here;
        Queued = state.Next;
        PassageOpen = state.Open;
        Partial = !state.Complete;
        Unavailable = !state.Available;
    }
}
