using Dalamud.Configuration;

namespace PotdCounter;

[Serializable]
public sealed class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;
    public bool ShowOverlay = true;
    public bool Locked;
    public bool ShowRanges = true;
    public bool ShowDetails;
    public float BackgroundOpacity = 0.88f;
}
