using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
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
}

internal sealed class CounterWindow : Window
{
    private static readonly Vector4 Pink = new(0.98f, 0.72f, 0.82f, 1);
    private static readonly Vector4 Green = new(0.52f, 0.90f, 0.72f, 1);
    private static readonly Vector4 Amber = new(1f, 0.81f, 0.46f, 1);
    private static readonly Vector4 Muted = new(0.72f, 0.71f, 0.76f, 1);
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
            MinimumSize = new Vector2(320, 0),
            MaximumSize = new Vector2(320, 900),
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
        var scale = ImGuiHelpers.GlobalScale;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(16, 14) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 10 * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(10, 5) * scale);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.10f, 0.10f, 0.13f, 1));
        ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0.30f, 0.27f, 0.32f, 1));
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.96f, 0.95f, 0.97f, 1));
    }

    public override void PostDraw()
    {
        ImGui.PopStyleColor(3);
        ImGui.PopStyleVar(3);
    }

    public override void Draw()
    {
        var t = plugin.Tracker;
        var simulated = !t.Visible && plugin.Preview && plugin.CanPreview;
        var demo = plugin.PreviewData;
        var floor = simulated ? demo.Floor : t.Floor;
        var kills = simulated ? demo.Kills : t.Kills;
        var here = simulated ? demo.Flight : t.FlightHere;
        var next = simulated ? demo.Queued : t.FlightNext;
        var complete = simulated ? !demo.Partial : t.CountComplete;
        var available = simulated ? !demo.Unavailable : plugin.CounterAvailable;
        var view = CounterPresentation.Create(floor, kills, here,
            simulated ? demo.PassageOpen : t.PassageOpen, complete, available);

        if (simulated) ImGui.TextColored(Amber, "simulation · pretend values");

        var left = ImGui.GetCursorPosX();
        var width = ImGui.GetContentRegionAvail().X;
        var floorText = $"floor {floor}";
        ImGui.TextColored(Pink, floorText);
        if (view.Observed is { } observed)
        {
            var right = left + width - ImGui.CalcTextSize(observed).X;
            if (right > left + ImGui.CalcTextSize(floorText).X + 10 * ImGuiHelpers.GlobalScale)
                ImGui.SameLine(right);
            ImGui.TextColored(Muted, observed);
        }

        ImGui.Spacing();
        var color = view.Tone switch
        {
            CounterTone.Ready => Green,
            CounterTone.Attention => Amber,
            _ => Vector4.One,
        };
        ImGui.PushStyleColor(ImGuiCol.Text, color);
        ImGui.SetWindowFontScale(view.IsEstimate ? 2f : 1.35f);
        ImGui.TextWrapped(view.Main);
        ImGui.SetWindowFontScale(1f);
        ImGui.PopStyleColor();
        ImGui.TextColored(Muted, view.Caption);

        if (view.ShowFlight)
        {
            ImGui.Spacing();
            ImGui.Separator();
            if (ImGui.BeginTable("FlightState", 2, ImGuiTableFlags.SizingStretchSame | ImGuiTableFlags.NoSavedSettings))
            {
                ImGui.TableNextColumn();
                ImGui.TextColored(Muted, "Flight this floor");
                ImGui.TextColored(FlightColor(here), CounterPresentation.FlightLabel(here));
                ImGui.TableNextColumn();
                ImGui.TextColored(Muted, "Flight next floor");
                ImGui.TextColored(FlightColor(next), CounterPresentation.FlightLabel(next, true));
                ImGui.EndTable();
            }
        }

        ImGui.Spacing();
        ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
        ImGui.PushStyleColor(ImGuiCol.Text, Pink);
        if (ImGui.SmallButton(plugin.Config.ShowDetails ? "less detail" : "details"))
        {
            plugin.Config.ShowDetails = !plugin.Config.ShowDetails;
            plugin.Save();
        }
        ImGui.PopStyleColor(2);
        if (plugin.Config.ShowDetails)
        {
            if (plugin.Config.ShowRanges && view.Range is { } range) ImGui.TextWrapped(range);
            ImGui.TextWrapped(view.Note);
            if (view.ShowFlight)
                ImGui.TextWrapped("Flight used here helps on the next floor");
            if (!complete && !simulated && available && FloorRules.Kind(floor) == FloorKind.Normal)
                ImGui.TextWrapped(t.ConfidenceNote);
        }

        if (ImGui.BeginPopupContextWindow("PotdCounterMenu", ImGuiPopupFlags.MouseButtonRight))
        {
            if (ImGui.MenuItem("settings")) plugin.OpenSettings();
            ImGui.EndPopup();
        }
    }

    private static Vector4 FlightColor(KnownState state) => state switch
    {
        KnownState.On => Green,
        KnownState.Unknown => Amber,
        _ => Muted,
    };
}
