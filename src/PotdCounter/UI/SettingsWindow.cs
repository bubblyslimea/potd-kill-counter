using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using PotdCounter.Core;

namespace PotdCounter.UI;

internal sealed class SettingsWindow : Window
{
    private readonly Plugin plugin;
    private int preset;
    private static readonly string[] PresetNames = CounterExamples.All.Select(e => e.Label).ToArray();

    public SettingsWindow(Plugin plugin) : base("Riri's PotD Counter · settings###PotdCounterSettings")
    {
        this.plugin = plugin;
        Size = new Vector2(490, 490);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw()
    {
        var c = plugin.Config;
        var changed = ImGui.Checkbox("show the counter while in PotD", ref c.ShowOverlay);
        changed |= ImGui.Checkbox("lock its position and click through", ref c.Locked);
        changed |= ImGui.SliderFloat("background opacity", ref c.BackgroundOpacity, 0.25f, 1f, "%.2f");
        if (changed) plugin.Save();
        ImGui.TextWrapped("drag the background while unlocked\n/potd brings you back here, /potdcount works too");
        ImGui.Spacing();

        ImGui.SetNextItemOpen(c.ShowDetails, ImGuiCond.Always);
        var details = ImGui.CollapsingHeader("run details");
        if (details != c.ShowDetails) { c.ShowDetails = details; plugin.Save(); }
        if (details)
        {
            var state = plugin.Display;
            var view = state.Presentation;
            ImGui.TextUnformatted(plugin.IsSimulated ? "simulation · pretend values" : "current run");
            if (state.Floor > 0) ImGui.TextUnformatted($"floor {state.Floor}");
            if (view.Observed is { } observed) ImGui.TextUnformatted(observed);
            if (ImGui.Checkbox("include the estimated floor total", ref c.ShowRanges)) plugin.Save();
            if (c.ShowRanges && view.Range is { } range) ImGui.TextWrapped(range);
            ImGui.TextWrapped(view.Note);
            if (view.ShowFlight)
            {
                ImGui.TextUnformatted($"Flight this floor: {CounterPresentation.FlightLabel(state.Here)}");
                ImGui.TextUnformatted($"Flight next floor: {CounterPresentation.FlightLabel(state.Next, true)}");
            }
            if (!state.Complete && !plugin.IsSimulated && state.Floor > 0)
                ImGui.TextWrapped(plugin.Tracker.ConfidenceNote);
        }
        ImGui.Spacing();

        var preview = plugin.Preview;
        if (ImGui.Checkbox("try a simulation outside PotD", ref preview))
            plugin.SetPreview(preview);
        if (preview)
        {
            var p = plugin.PreviewData;
            if (ImGui.Combo("example", ref preset, PresetNames, PresetNames.Length))
                p.Use(CounterExamples.All[preset].State);
            if (ImGui.Button("reset example")) p.Use(CounterExamples.All[preset].State);
            if (ImGui.CollapsingHeader("adjust pretend values"))
            {
                ImGui.SliderInt("floor", ref p.Floor, 1, 200);
                ImGui.SliderInt("observed kills", ref p.Kills, 0, 20);
                var here = (int)p.Flight;
                if (ImGui.Combo("Flight this floor", ref here, "unknown\0inactive\0active\0")) p.Flight = (KnownState)here;
                var next = (int)p.Queued;
                if (ImGui.Combo("Flight next floor", ref next, "unknown\0not queued\0queued\0")) p.Queued = (KnownState)next;
                ImGui.Checkbox("passage is open", ref p.PassageOpen);
                ImGui.Checkbox("partial count", ref p.Partial);
                ImGui.Checkbox("kill tracking unavailable", ref p.Unavailable);
            }
            ImGui.TextWrapped("pretend values, your real run takes over inside PotD");
        }

        ImGui.Spacing();
        if (ImGui.CollapsingHeader("about the count"))
        {
            ImGui.TextWrapped("the big number is a rough range of kills left\nthe game tells us when the passage opens");
            ImGui.TextWrapped("Flight this floor is already helping you\nFlight next floor means a use is queued for the next floor");
            ImGui.TextWrapped("FIRST FLOOR: Flight Unavailable describes the incoming effect on floor 1, not your pomander inventory\nyou can still use a pomander there for the next floor");
            ImGui.TextWrapped("earlier or distant party kills may be missing if you join late or reconnect\na wider amber range keeps kills left visible, partial counts stay labelled\nunknown Flight includes both possible targets");
        }
        if (ImGui.CollapsingHeader("status"))
        {
            ImGui.TextUnformatted(plugin.ReaderStatus);
            ImGui.TextWrapped(plugin.CounterAvailable
                ? "kill tracking is ready"
                : "kill tracking is unavailable\nremaining estimates are paused, the passage status can still be shown");
        }
    }
}
