using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Network;
using PotdCounter.Core;
using BattleNpcSubKind = Dalamud.Game.ClientState.Objects.Enums.BattleNpcSubKind;

namespace PotdCounter.Game;

internal readonly record struct DeathMessage(EnemyDeath Death, bool Unclassified);

/// <summary>One read-only observer. Never sends, suppresses or modifies a game event.</summary>
internal sealed unsafe class EnemyDeathObserver : IDisposable
{
    private delegate void ActorControl(uint entityId, uint category,
        uint arg1, uint arg2, uint arg3, uint arg4, uint arg5, uint arg6,
        uint arg7, uint arg8, GameObjectId targetId, [MarshalAs(UnmanagedType.I1)] bool isRecorded);

    private readonly Hook<ActorControl>? hook;
    private readonly IObjectTable objects;
    private readonly PotdReader reader;
    private readonly ConcurrentQueue<DeathMessage> queue = new();
    private int observerFault;

    public bool Available => hook != null;
    public bool HasEvents => !queue.IsEmpty;
    public bool Faulted => Volatile.Read(ref observerFault) != 0;

    public EnemyDeathObserver(IObjectTable objects, PotdReader reader,
        IGameInteropProvider interop, IPluginLog log)
    {
        this.objects = objects;
        this.reader = reader;
        try
        {
            var address = (nint)PacketDispatcher.MemberFunctionPointers.HandleActorControlPacket;
            if (address == 0) throw new InvalidOperationException("Actor-control address is unavailable");
            hook = interop.HookFromAddress<ActorControl>(address, OnActorControl);
        }
        catch (Exception ex)
        {
            log.Warning(ex, "Kill observer unavailable; remaining estimates will be suppressed");
        }
    }

    public void SetEnabled(bool enabled)
    {
        if (hook == null || hook.IsEnabled == enabled) return;
        if (enabled) hook.Enable(); else hook.Disable();
    }

    public bool TryRead(out DeathMessage message) => queue.TryDequeue(out message);

    public void Clear()
    {
        queue.Clear();
        Interlocked.Exchange(ref observerFault, 0);
    }

    private void OnActorControl(uint entityId, uint category,
        uint arg1, uint arg2, uint arg3, uint arg4, uint arg5, uint arg6,
        uint arg7, uint arg8, GameObjectId targetId, bool isRecorded)
    {
        // Capture identity before the game's handler can remove an object. Exceptions in
        // our observation must not prevent the original handler from running exactly once.
        try
        {
            if (category == 6 && !isRecorded && reader.Read(out var floor) == ReadState.Ready &&
                FloorRules.Kind(floor.Floor) is FloorKind.Normal or FloorKind.Boss)
            {
                var actor = objects.SearchByEntityId(entityId);
                var enemy = actor is IBattleNpc npc && npc.BattleNpcKind == BattleNpcSubKind.Combatant;
                if (enemy || actor == null)
                {
                    if (queue.Count < 1024)
                        queue.Enqueue(new(new(floor.InstanceKey, floor.Floor, entityId, enemy, actor?.BaseId ?? 0), actor == null));
                    else
                        Interlocked.Exchange(ref observerFault, 1);
                }
            }
        }
        catch
        {
            // Report via managed state, not repeated logging from a native callback.
            Interlocked.Exchange(ref observerFault, 1);
        }

        hook!.Original(entityId, category, arg1, arg2, arg3, arg4,
            arg5, arg6, arg7, arg8, targetId, isRecorded);
    }

    public void Dispose()
    {
        hook?.Disable();
        hook?.Dispose();
        queue.Clear();
    }
}
