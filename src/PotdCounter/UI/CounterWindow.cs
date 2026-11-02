using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using PotdCounter.Core;

namespace PotdCounter.UI;

internal sealed class PreviewState
{
    public int Floor = 193;
    public int Kills = 4;
    public bool Flight;
    public bool Queued = true;
    public bool PassageOpen;
    public bool Partial;
}

internal sealed class CounterWindow : Window
{
    private static readonly Vector4 Pink = new(0.98f, 0.72f, 0.82f, 1);
    private static readonly Vector4 Green = new(0.52f, 0.90f, 0.72f, 1);
    private static readonly Vector4 Amber = new(1f, 0.81f, 0.46f, 1);
    private readonly Plugin plugin;

    public CounterWindow(Plugin plugin) : base("Riri's PotD Counter###PotdCounterLive")
    {
        this.plugin = plugin;
        IsOpen = true;
        ShowCloseButton = false;
        RespectCloseHotkey = false;
        Position = new Vector2(30, 180);
        PositionCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(340, 0),
            MaximumSize = new Vector2(460, 600),
        };
    }

    public override bool DrawConditions() =>
        plugin.Config.ShowOverlay && (plugin.Tracker.Visible || (plugin.Preview && plugin.CanPreview));

    public override void PreDraw()
    {
        Flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize |
                ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav |
                ImGuiWindowFlags.NoScrollbar;
        if (plugin.Config.Locked) Flags |= ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoInputs;
        ImGui.SetNextWindowBgAlpha(plugin.Config.BackgroundOpacity);
    }

    public override void Draw()
    {
        var t = plugin.Tracker;
        var simulated = !t.Visible && plugin.Preview && plugin.CanPreview;
        var demo = plugin.PreviewData;
        var floor = simulated ? demo.Floor : t.Floor;
        var kills = simulated ? demo.Kills : t.Kills;
        var here = simulated ? (demo.Flight ? KnownState.On : KnownState.Off) : t.FlightHere;
        var next = simulated ? (demo.Queued ? KnownState.On : KnownState.Off) : t.FlightNext;
        var complete = simulated ? !demo.Partial : t.CountComplete;
        var estimate = simulated
            ? PassageEstimator.Estimate(floor, kills, here, demo.PassageOpen, complete)
            : PassageEstimator.Estimate(t);

        if (simulated) ImGui.TextColored(Amber, "simulation · not connected to your run");
        ImGui.TextColored(Pink, $"PotD  ·  floor {floor}");
        ImGui.Separator();

        var unavailable = !simulated && !plugin.CounterAvailable;
        var headline = unavailable && estimate.Kind is not (EstimateKind.Open or EstimateKind.Boss or EstimateKind.Complete)
            ? "kill tracking is unavailable"
            : estimate.Headline;
        var color = estimate.Kind == EstimateKind.Open ? Green :
            estimate.Kind is EstimateKind.Partial or EstimateKind.UnknownFlight or EstimateKind.RangeExceeded || unavailable
                ? Amber : Vector4.One;
        ImGui.PushStyleColor(ImGuiCol.Text, color);
        ImGui.TextWrapped(headline);
        ImGui.PopStyleColor();

        if (FloorRules.Kind(floor) == FloorKind.Normal && !unavailable)
            ImGui.TextUnformatted($"{kills} kills observed{(complete ? "" : " · partial")}");

        if (plugin.Config.ShowRanges && FloorRules.NormalRange(floor) is { } range)
        {
            var text = here switch
            {
                KnownState.On => $"estimated floor total: {range.WithFlight()} kills",
                KnownState.Off => $"estimated floor total: {range} kills",
                _ => $"floor total: {range} normal / {range.WithFlight()} with Flight",
            };
            ImGui.TextDisabled(text);
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("these ranges include rare outcomes, each floor rolls its own target\nclearing every enemy can open the passage early\nthe game tells us when it opens");
        }

        ImGui.Spacing();
        ImGui.TextColored(here == KnownState.On ? Green : Pink, $"Flight here: {Label(here, "active", "off")}");
        ImGui.SameLine();
        ImGui.TextUnformatted($"  next: {Label(next, "queued", "not queued")}");
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Flight used here helps on the next floor\nunknown means there is not enough observed history yet");

        if (!complete && !simulated && ImGui.IsWindowHovered())
            ImGui.SetTooltip(t.ConfidenceNote);

        if (ImGui.BeginPopupContextWindow("PotdCounterMenu", ImGuiPopupFlags.MouseButtonRight))
        {
            if (ImGui.MenuItem("settings")) plugin.OpenSettings();
            ImGui.EndPopup();
        }
    }

    private static string Label(KnownState value, string on, string off) => value switch
    {
        KnownState.On => on,
        KnownState.Off => off,
        _ => "unknown",
    };
}
