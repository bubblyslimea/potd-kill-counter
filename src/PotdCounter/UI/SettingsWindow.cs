using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace PotdCounter.UI;

internal sealed class SettingsWindow : Window
{
    private readonly Plugin plugin;

    public SettingsWindow(Plugin plugin) : base("Riri's PotD Counter · settings###PotdCounterSettings")
    {
        this.plugin = plugin;
        Size = new Vector2(460, 440);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw()
    {
        var c = plugin.Config;
        var changed = ImGui.Checkbox("show the counter while in PotD", ref c.ShowOverlay);
        changed |= ImGui.Checkbox("lock its position and click through", ref c.Locked);
        changed |= ImGui.Checkbox("show the estimated floor total", ref c.ShowRanges);
        changed |= ImGui.SliderFloat("background opacity", ref c.BackgroundOpacity, 0.25f, 1f, "%.2f");
        if (changed) plugin.Save();
        ImGui.TextWrapped("drag the background to move it while unlocked\n/potdcount brings you back here");
        ImGui.Separator();
        ImGui.TextUnformatted(plugin.ReaderStatus);
        ImGui.TextWrapped(plugin.CounterAvailable
            ? "kill tracking is ready, this alpha still needs live PotD testing"
            : "kill tracking is unavailable\nremaining estimates are paused, the passage status can still be shown");
        ImGui.TextWrapped("earlier or distant party kills may be missing if you join late or reconnect\nFlight here becomes known after a witnessed floor change");
        ImGui.Separator();

        var preview = plugin.Preview;
        if (ImGui.Checkbox("try a simulation outside PotD", ref preview)) plugin.Preview = preview;
        if (preview)
        {
            var p = plugin.PreviewData;
            ImGui.SliderInt("floor", ref p.Floor, 1, 200);
            ImGui.SliderInt("observed kills", ref p.Kills, 0, 20);
            ImGui.Checkbox("Flight active here", ref p.Flight);
            ImGui.Checkbox("Flight queued for the next floor", ref p.Queued);
            ImGui.Checkbox("passage is open", ref p.PassageOpen);
            ImGui.Checkbox("partial count", ref p.Partial);
            ImGui.TextWrapped("these are pretend values, your real run takes over inside PotD");
        }
    }
}
