using Dalamud.Game.Command;
using Dalamud.Game.DutyState;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using PotdCounter.Core;
using PotdCounter.Game;
using PotdCounter.UI;

namespace PotdCounter;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static IClientState Client { get; private set; } = null!;
    [PluginService] internal static ICondition Condition { get; private set; } = null!;
    [PluginService] internal static IPartyList Party { get; private set; } = null!;
    [PluginService] internal static IObjectTable Objects { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider Interop { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IDutyState Duty { get; private set; } = null!;
    [PluginService] internal static ICommandManager Commands { get; private set; } = null!;

    private readonly WindowSystem windows = new("PotdCounter");
    private readonly PotdReader reader;
    private readonly EnemyDeathObserver deaths;
    private readonly SettingsWindow settings;
    private long nextRead;
    private long freshEntranceUntil;
    private bool readErrorLogged;

    internal FloorTracker Tracker { get; } = new();
    internal Configuration Config { get; }
    internal bool Preview { get; set; }
    internal PreviewState PreviewData { get; } = new();
    internal bool CanPreview { get; private set; } = true;
    internal bool CounterAvailable => deaths.Available && !deaths.Faulted;
    internal string ReaderStatus { get; private set; } = "waiting for PotD";

    public Plugin()
    {
        Config = PluginInterface.GetPluginConfig() as Configuration ?? new();
        Config.BackgroundOpacity = Math.Clamp(Config.BackgroundOpacity, 0.25f, 1f);
        reader = new(Client, Condition, Party);
        deaths = new(Objects, reader, Interop, Log);
        settings = new(this);
        windows.AddWindow(new CounterWindow(this));
        windows.AddWindow(settings);
        Commands.AddHandler("/potdcount", new CommandInfo(OnCommand)
        {
            HelpMessage = "open the counter settings | /potdcount preview tries a simulation | /potdcount toggle shows or hides the counter",
        });
        Framework.Update += Update;
        Duty.DutyStarted += OnDutyStarted;
        Client.Logout += OnLogout;
        PluginInterface.UiBuilder.Draw += windows.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += OpenSettings;
        PluginInterface.UiBuilder.OpenMainUi += OpenSettings;
    }

    private void Update(IFramework _)
    {
        var now = Environment.TickCount64;
        if (now < nextRead && !deaths.HasEvents) return;
        nextRead = now + 200;
        try
        {
            var state = reader.Read(out var snapshot);
            CanPreview = state == ReadState.Outside;
            deaths.SetEnabled(state == ReadState.Ready);
            ReaderStatus = state switch
            {
                ReadState.Ready => "following your PotD run",
                ReadState.Transition => "changing areas",
                ReadState.Unavailable => "waiting for PotD data",
                _ => "waiting for PotD",
            };
            if (state == ReadState.Ready)
            {
                Tracker.Apply(snapshot, now, now <= freshEntranceUntil);
                freshEntranceUntil = 0;
                while (deaths.TryRead(out var message))
                {
                    if (message.Death.InstanceKey != snapshot.InstanceKey || message.Death.Floor != snapshot.Floor)
                        continue;
                    if (message.Unclassified)
                        Tracker.MarkIncomplete("an unidentified death was received, a kill may be missing");
                    else
                        Tracker.Observe(message.Death);
                }
                if (!CounterAvailable)
                    Tracker.MarkIncomplete("kill tracking is unavailable, remaining estimate paused");
            }
            else if (state == ReadState.Outside)
            {
                Tracker.Leave();
                deaths.Clear();
            }
            else
            {
                Tracker.Suspend(ReaderStatus);
            }
        }
        catch (Exception ex)
        {
            CanPreview = false;
            Tracker.Suspend("game data is unavailable");
            ReaderStatus = "game data is unavailable";
            if (!readErrorLogged)
            {
                Log.Warning(ex, "PotD counter suspended until game data can be read");
                readErrorLogged = true;
            }
        }
    }

    private void OnDutyStarted(IDutyStateEventArgs _)
    {
        Tracker.Leave();
        deaths.Clear();
        freshEntranceUntil = Environment.TickCount64 + 10000;
        nextRead = 0;
    }

    private void OnLogout(int _, int __)
    {
        deaths.SetEnabled(false);
        deaths.Clear();
        Tracker.Leave();
        freshEntranceUntil = 0;
    }

    private void OnCommand(string _, string args)
    {
        switch (args.Trim().ToLowerInvariant())
        {
            case "preview": Preview = !Preview; break;
            case "toggle": Config.ShowOverlay = !Config.ShowOverlay; Save(); break;
            default: OpenSettings(); break;
        }
    }

    internal void Save() => PluginInterface.SavePluginConfig(Config);
    internal void OpenSettings() => settings.IsOpen = true;

    public void Dispose()
    {
        Framework.Update -= Update;
        Duty.DutyStarted -= OnDutyStarted;
        Client.Logout -= OnLogout;
        deaths.Dispose();
        PluginInterface.UiBuilder.Draw -= windows.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= OpenSettings;
        PluginInterface.UiBuilder.OpenMainUi -= OpenSettings;
        Commands.RemoveHandler("/potdcount");
        windows.RemoveAllWindows();
    }
}
