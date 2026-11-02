using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using PotdCounter.Core;

namespace PotdCounter.UI;

internal sealed class SettingsWindow : Window
{
    private readonly Plugin plugin;

    public SettingsWindow(Plugin plugin) : base("Riri's PotD Counter · settings###PotdCounterSettings")
    {
        this.plugin = plugin;
        Size = new Vector2(460, 360);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw()
    {
        var c = plugin.Config;
        var changed = ImGui.Checkbox("show the counter while in PotD", ref c.ShowOverlay);
        changed |= ImGui.Checkbox("lock its position and click through", ref c.Locked);
        changed |= ImGui.Checkbox("keep details open", ref c.ShowDetails);
        if (c.ShowDetails)
            changed |= ImGui.Checkbox("include the estimated floor total", ref c.ShowRanges);
        changed |= ImGui.SliderFloat("background opacity", ref c.BackgroundOpacity, 0.25f, 1f, "%.2f");
        if (changed) plugin.Save();
        ImGui.TextWrapped("drag the background while unlocked\n/potd brings you back here, /potdcount works too");
        ImGui.Spacing();

        var preview = plugin.Preview;
        if (ImGui.Checkbox("try a simulation outside PotD", ref preview))
            plugin.SetPreview(preview);
        if (preview)
        {
            var p = plugin.PreviewData;
            ImGui.SliderInt("floor", ref p.Floor, 1, 200);
            ImGui.SliderInt("observed kills", ref p.Kills, 0, 20);
            var here = (int)p.Flight;
            if (ImGui.Combo("Flight this floor", ref here, "unknown\0off\0active\0")) p.Flight = (KnownState)here;
            var next = (int)p.Queued;
            if (ImGui.Combo("Flight next floor", ref next, "unknown\0not queued\0queued\0")) p.Queued = (KnownState)next;
            ImGui.Checkbox("passage is open", ref p.PassageOpen);
            ImGui.Checkbox("partial count", ref p.Partial);
            ImGui.Checkbox("kill tracking unavailable", ref p.Unavailable);
            ImGui.TextWrapped("pretend values, your real run takes over inside PotD");
        }

        ImGui.Spacing();
        if (ImGui.CollapsingHeader("about the count"))
        {
            ImGui.TextWrapped("the big number is a rough range of kills left\nthe game tells us when the passage opens");
            ImGui.TextWrapped("Flight this floor is already helping you\nFlight next floor means a use is queued for the next floor");
            ImGui.TextWrapped("earlier or distant party kills may be missing if you join late or reconnect\nunknown counts stay labelled, even with details closed");
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
