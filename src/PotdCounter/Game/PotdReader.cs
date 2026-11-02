using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using PotdCounter.Core;

namespace PotdCounter.Game;

internal enum ReadState { Outside, Transition, Unavailable, Ready }

internal sealed unsafe class PotdReader(IClientState client, ICondition condition, IPartyList party)
{
    public ReadState Read(out FloorSnapshot snapshot)
    {
        snapshot = default;
        if (!client.IsLoggedIn) return ReadState.Outside;
        // Loading can briefly clear the duty condition. Preserve the prior floor's queue.
        if (condition[ConditionFlag.BetweenAreas] || condition[ConditionFlag.BetweenAreas51])
            return ReadState.Transition;
        if (!condition[ConditionFlag.InDeepDungeon]) return ReadState.Outside;
        var framework = EventFramework.Instance();
        if (framework == null) return ReadState.Unavailable;
        var director = framework->GetInstanceContentDeepDungeon();
        if (director == null) return ReadState.Unavailable;
        if (director->DeepDungeonId == 0) return ReadState.Unavailable;
        if (director->DeepDungeonId != 1) return ReadState.Outside;
        if (director->Floor is < 1 or > 200)
            return ReadState.Unavailable;

        var flight = KnownState.Unknown;
        // PotD sheet row 1 maps Flight (DeepDungeonItem row 6) to slot 5.
        // Verify the row ID instead of accepting an arbitrary slot as Flight.
        foreach (ref var item in director->Items)
        {
            if (item.ItemId != 6) continue;
            flight = item.IsActive ? KnownState.On : KnownState.Off;
            break;
        }

        snapshot = new((long)director, director->Floor, director->PassageProgress,
            flight, Math.Max(1, party.Length));
        return ReadState.Ready;
    }
}
