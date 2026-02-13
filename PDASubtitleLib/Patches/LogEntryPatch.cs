using System;
using HarmonyLib;
using PDASubtitleLib.Handlers;
using PDASubtitleLib.Utilities;
using UnityEngine;
using UWE;

namespace PDASubtitleLib.Patches
{
    [HarmonyPatch(typeof(uGUI_LogEntry))]
    public static class LogEntryPatch
    {
        private static Coroutine _subtitleCoroutine;

        [HarmonyPatch(nameof(uGUI_LogEntry.ToggleSound))]
        [HarmonyPrefix]
        public static void ToggleSound_Prefix(uGUI_LogEntry __instance)
        {
            if (__instance == null) return;
            var entryKeyField = AccessTools.Field(typeof(uGUI_LogEntry), "entryKey");
            if (entryKeyField == null) return;
            var entryKey = entryKeyField.GetValue(__instance) as string;
            if (string.IsNullOrEmpty(entryKey)) return;
            if (entryKey.StartsWith(SubtitleUtils.LanguageKeyPrefix))
            {
                var soundField = AccessTools.Field(typeof(uGUI_LogEntry), "sound");
                if (soundField == null) return;
                var sound = soundField.GetValue(__instance) as FMODAsset;
                if (sound == null) return;

                SoundQueue queue = PDASounds.queue;
                if (queue != null)
                {
                    if (queue.current == sound.id)
                    {
                        CoroutineHost.StopCoroutine(_subtitleCoroutine);
                        _subtitleCoroutine = null;
                    }
                    else
                    {
                        var lines = SubtitleUtils.GetSubtitleLinesByKey(entryKey);
                        if (lines == null || lines.Count == 0)
                        {
                            Plugin.Log.LogWarning($"No subtitles loaded for log entry key: {entryKey}");
                            return;
                        }

                        var baseKey = SubtitleUtils.StripLanguageKeyPrefix(entryKey);
                        _subtitleCoroutine = CoroutineHost.StartCoroutine(SubtitlesHandler.PlaySubtitleLines(baseKey, lines));
                    }
                }
            }
        }
    }
}