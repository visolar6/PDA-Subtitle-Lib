using Nautilus.Options.Attributes;

namespace PDASubtitleLib
{
    [Menu("PDA Subtitle Library")]
    internal class Options : Nautilus.Json.ConfigFile
    {
        // [Toggle(Label = "Debug", Tooltip = "Enable debug mode for testing and troubleshooting. This will log additional information to the console and attempt to load a test subtitle and audio file on game start.")]
        // public bool Debug = false;
    }
}
