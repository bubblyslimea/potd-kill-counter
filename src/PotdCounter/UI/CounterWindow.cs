using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using PotdCounter.Core;

namespace PotdCounter.UI;

internal sealed class CounterWindow : Window, IDisposable
{
    private const float Width = 320;
    private const float Height = 232;
    private static readonly Vector4 White = new(0.97f, 0.97f, 0.99f, 1);
    private static readonly Vector4 Green = new(0.40f, 0.98f, 0.69f, 1);
    private static readonly Vector4 Amber = new(1f, 0.81f, 0.29f, 1);
    private static readonly Vector4 Lavender = new(0.88f, 0.72f, 1f, 1);
    private static readonly Vector4 Blue = new(0.40f, 0.84f, 1f, 1);
    private readonly Plugin plugin;
    private readonly HudFonts fonts;
    private float opacity = 1f;

    public CounterWindow(Plugin plugin) : base("Riri's PotD Counter###PotdCounterLive")
    {
        this.plugin = plugin;
        fonts = new(Plugin.PluginInterface.UiBuilder.FontAtlas);
        IsOpen = true;
        ShowCloseButton = false;
        RespectCloseHotkey = false;
        Position = new Vector2(30, 180);
        PositionCondition = ImGuiCond.FirstUseEver;
        SizeCondition = ImGuiCond.Always;
    }

    public override bool DrawConditions() =>
        plugin.Config.ShowOverlay && (plugin.Tracker.Visible || plugin.IsSimulated) && !plugin.Display.HideForBossCombat;

    public override void PreDraw()
    {
        Size = new Vector2(Width, Height + (plugin.IsSimulated ? 24 : 0));
        Flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse |
                ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoBackground |
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        if (plugin.Config.Locked) Flags |= ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoInputs;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
    }

    public override void PostDraw() => ImGui.PopStyleVar(2);

    public override void Draw()
    {
        var state = plugin.Display;
        var view = state.Presentation;
        opacity = view.IsBossVictory ? state.VictoryOpacity : 1f;
        var scale = ImGuiHelpers.GlobalScale;
        var origin = ImGui.GetWindowPos();
        var draw = ImGui.GetWindowDrawList();
        if (plugin.IsSimulated)
        {
            draw.AddRectFilled(origin, origin + new Vector2(Width, 22) * scale, Color(new(0.06f, 0.07f, 0.08f, 0.96f)), 5 * scale);
            Text(fonts.Small, "simulation · pretend values", origin + new Vector2(Width / 2, 2) * scale, Amber, true);
            origin.Y += 24 * scale;
        }

        var border = view.Tone == CounterTone.Ready ? Green : new Vector4(0.26f, 0.29f, 0.31f, 1);
        draw.AddRectFilled(origin, origin + new Vector2(Width, Height) * scale,
            Color(new(0.055f, 0.065f, 0.072f, plugin.Config.BackgroundOpacity)), 18 * scale);
        draw.AddRect(origin + Vector2.One * scale, origin + new Vector2(Width - 1, Height - 1) * scale,
            Color(border), 18 * scale, ImDrawFlags.None, (view.Tone == CounterTone.Ready ? 2 : 1) * scale);

        Text(fonts.Body, $"floor {state.Floor}", origin + new Vector2(18, 12) * scale, White);
        if (view.IsBossVictory)
            DrawVictory(view, origin, scale);
        else
        {
            DrawHeadline(view, origin, scale);
            Text(view.Caption.Length > 22 ? fonts.Small : fonts.Caption, view.Caption,
                origin + new Vector2(Width / 2, 108) * scale, White, true);
        }

        if (view.ShowFlight)
        {
            DrawBadge(view.Flight, origin, scale);
            // Reserved lines keep independent observations visible without resizing the card.
            var noticeY = 187f;
            if (view.NextFlightNotice is { } next)
            {
                Text(fonts.Small, next, origin + new Vector2(Width / 2, noticeY) * scale, Tone(view.NextFlightTone), true);
                noticeY += 20;
            }
            if (view.DestinationNotice is { } destination)
                Text(fonts.Small, destination, origin + new Vector2(Width / 2, noticeY) * scale, Amber, true);
        }

        if (ImGui.BeginPopupContextWindow("PotdCounterMenu", ImGuiPopupFlags.MouseButtonRight))
        {
            if (ImGui.MenuItem("settings")) plugin.OpenSettings();
            ImGui.EndPopup();
        }
    }

    private void DrawVictory(CounterPresentation view, Vector2 origin, float scale)
    {
        var font = fonts.BossLarge;
        var lineHeight = 48f;
        if (!Fits(font)) { font = fonts.BossMedium; lineHeight = 40f; }
        if (!Fits(font)) { font = fonts.BossSmall; lineHeight = 34f; }
        var lines = view.Main.Split('\n');
        var top = 40 + (96 - lines.Length * lineHeight) / 2;
        for (var i = 0; i < lines.Length; i++)
            Text(font, lines[i], origin + new Vector2(Width / 2, top + i * lineHeight) * scale, White, true);

        using (fonts.Caption.Push())
        {
            var size = ImGui.CalcTextSize(view.Caption);
            var position = origin + new Vector2((Width * scale - size.X - 30 * scale) / 2, 145 * scale);
            var draw = ImGui.GetWindowDrawList();
            draw.AddText(position, Color(Green), view.Caption);
            var check = position + new Vector2(size.X + 10 * scale, 14 * scale);
            draw.AddLine(check, check + new Vector2(6, 6) * scale, Color(Green), 3 * scale);
            draw.AddLine(check + new Vector2(6, 6) * scale, check + new Vector2(20, -10) * scale, Color(Green), 3 * scale);
        }
        if (view.DestinationNotice is { } passage)
            Text(fonts.Small, passage, origin + new Vector2(Width / 2, 193) * scale, Green, true);

        bool Fits(IFontHandle candidate)
        {
            using var scope = candidate.Push();
            return ImGui.CalcTextSize(view.Main).X <= 280 * scale;
        }
    }

    private void DrawHeadline(CounterPresentation view, Vector2 origin, float scale)
    {
        using var font = fonts.Headline.Push();
        var size = ImGui.CalcTextSize(view.Main);
        var isOpen = view.Main == "OPEN";
        var extra = isOpen ? 42 * scale : 0;
        var position = origin + new Vector2((Width * scale - size.X - extra) / 2, 32 * scale);
        ImGui.GetWindowDrawList().AddText(position, Color(Tone(view.Tone)), view.Main);
        if (!isOpen) return;
        var check = new Vector2(position.X + size.X + 13 * scale, origin.Y + 70 * scale);
        var draw = ImGui.GetWindowDrawList();
        draw.AddLine(check, check + new Vector2(9, 9) * scale, Color(Green), 5 * scale);
        draw.AddLine(check + new Vector2(9, 9) * scale, check + new Vector2(29, -14) * scale, Color(Green), 5 * scale);
    }

    private void DrawBadge(FlightBadge badge, Vector2 origin, float scale)
    {
        var draw = ImGui.GetWindowDrawList();
        var tint = Tone(badge.Tone);
        var fill = badge.Tone switch
        {
            CounterTone.Active => new Vector4(0.23f, 0.13f, 0.33f, 1),
            CounterTone.Attention => new Vector4(0.27f, 0.22f, 0.12f, 1),
            _ => new Vector4(0.17f, 0.19f, 0.22f, 1),
        };
        float textWidth;
        using (fonts.Body.Push()) textWidth = ImGui.CalcTextSize(badge.Status + "  " + badge.Detail).X;
        var width = badge.TwoLines ? 230 * scale : Math.Max(198 * scale, textWidth + 36 * scale);
        var left = origin + new Vector2((Width * scale - width) / 2, 140 * scale);
        draw.AddRectFilled(left, left + new Vector2(width, 40 * scale), Color(fill), 20 * scale);
        draw.AddRect(left, left + new Vector2(width, 40 * scale), Color(new(tint.X, tint.Y, tint.Z, 0.35f)), 20 * scale);
        if (badge.TwoLines)
        {
            Text(fonts.Small, badge.Status, origin + new Vector2(Width / 2, 140) * scale, tint, true);
            Text(fonts.Body, badge.Detail, origin + new Vector2(Width / 2, 157) * scale, tint, true);
        }
        else
        {
            Text(fonts.Body, badge.Status + "  " + badge.Detail,
                origin + new Vector2(Width / 2, 148) * scale, tint, true);
        }
    }

    private void Text(IFontHandle font, string text, Vector2 position, Vector4 color, bool centered = false)
    {
        using var scope = font.Push();
        if (centered) position.X -= ImGui.CalcTextSize(text).X / 2;
        position = new Vector2(MathF.Round(position.X), MathF.Round(position.Y));
        ImGui.GetWindowDrawList().AddText(position, Color(color), text);
    }

    private static Vector4 Tone(CounterTone tone) => tone switch
    {
        CounterTone.Ready => Green,
        CounterTone.Attention => Amber,
        CounterTone.Active => Lavender,
        CounterTone.Future => Blue,
        _ => White,
    };

    private uint Color(Vector4 color)
    {
        color.W *= opacity;
        return ImGui.ColorConvertFloat4ToU32(color);
    }
    public void Dispose() => fonts.Dispose();
}
