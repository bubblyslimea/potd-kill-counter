using Dalamud;
using Dalamud.Interface.ManagedFontAtlas;

namespace PotdCounter.UI;

// Created once, rebuilt by the host at UI-scale changes, and released on unload.
internal sealed class HudFonts : IDisposable
{
    private static readonly ushort[] Ranges = [0x20, 0xFF, 0x2013, 0x2013, 0];
    public IFontHandle Headline { get; }
    public IFontHandle Caption { get; }
    public IFontHandle Body { get; }
    public IFontHandle Small { get; }

    public HudFonts(IFontAtlas atlas)
    {
        using var batch = atlas.SuppressAutoRebuild();
        Headline = atlas.NewDelegateFontHandle(e => e.OnPreBuild(tk =>
        {
            using var stream = typeof(HudFonts).Assembly.GetManifestResourceStream("PotdCounter.Headline.ttf")
                ?? throw new InvalidOperationException("embedded headline font is missing");
            tk.Font = tk.AddFontFromStream(stream, new SafeFontConfig { SizePx = 80, GlyphRanges = Ranges }, true, "counter headline");
        }));
        Caption = BodyFont(atlas, 20);
        Body = BodyFont(atlas, 18);
        Small = BodyFont(atlas, 16);
    }

    private static IFontHandle BodyFont(IFontAtlas atlas, float size) =>
        atlas.NewDelegateFontHandle(e => e.OnPreBuild(tk =>
            tk.Font = tk.AddDalamudAssetFont(DalamudAsset.NotoSansCjkMedium,
                new SafeFontConfig { SizePt = size, GlyphRanges = Ranges })));

    public void Dispose()
    {
        Headline.Dispose();
        Caption.Dispose();
        Body.Dispose();
        Small.Dispose();
    }
}
