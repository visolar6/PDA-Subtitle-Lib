using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace PDASubtitleLib
{
    [BepInPlugin(GUID, Name, Version)]
    internal class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        internal const string GUID = "com.visolar6.pdasubtitlelib";

        internal const string Name = "PDA Subtitle Library";

        internal const string Version = "1.0.0";

        private readonly Harmony _harmony = new Harmony(GUID);

        private void Awake()
        {
            Log = Logger;
        }

        private void Start()
        {
            Log?.LogInfo("Patching hooks");
            _harmony.PatchAll();
        }
    }
}