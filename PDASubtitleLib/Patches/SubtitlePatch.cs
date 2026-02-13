using PDASubtitleLib.Utilities;
using HarmonyLib;

namespace PDASubtitleLib.Patches
{
    /// <summary>
    /// Prevents vanilla subtitle system from displaying message keys
    /// Our custom SubtitleScheduler will handle these instead using .srt files
    /// </summary>
    [HarmonyPatch(typeof(Subtitles))]
    internal static class SubtitlesPatch
    {
        [HarmonyPatch(nameof(Subtitles.Add))]
        [HarmonyPrefix]
        public static bool Add_Prefix(string key, object[] args = null)
        {
            if (!string.IsNullOrEmpty(key) && key.StartsWith(SubtitleUtils.LanguageKeyPrefix))
            {
                // Check for _<number> at the end
                var trimmedKey = key.Trim();
                int lastUnderscore = trimmedKey.LastIndexOf('_');
                bool isIndexed = false;
                if (lastUnderscore > SubtitleUtils.LanguageKeyPrefix.Length)
                {
                    var suffix = trimmedKey.Substring(lastUnderscore + 1);
                    isIndexed = int.TryParse(suffix, out _);
                }
                if (!isIndexed) return false;
            }
            return true;
        }
    }
}
