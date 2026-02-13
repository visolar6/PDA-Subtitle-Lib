using System.Collections;
using System.Collections.Generic;
using System.IO;
using PDASubtitleLib.Utilities;
using Nautilus.Handlers;
using UnityEngine;
using UWE;

namespace PDASubtitleLib.Handlers
{
    public static class SubtitlesHandler
    {
        public static string SubtitlesFolderPath { get; private set; }

        /// <summary>
        /// Registers the subtitle scheduler and loads subtitles from the specified folder path. This should be called on game start (e.g. in Plugin.Start).
        /// </summary>
        /// <param name="subtitlesFolderPath">The folder path where .srt subtitle files are located. Each file should be named with the base key (e.g. "log1.srt" for key "log1").</param>
        /// <remarks>I recommend you name the accompanying audio files the same as the .srt files (e.g. "log1.ogg") and place them in the same folder for easier management, but this is not required.</remarks>
        public static void Register(string subtitlesFolderPath)
        {
            if (!Directory.Exists(subtitlesFolderPath))
            {
                throw new DirectoryNotFoundException($"Specified subtitles folder does not exist: {subtitlesFolderPath}");
            }
            SubtitlesFolderPath = subtitlesFolderPath;

            var srtFiles = Directory.GetFiles(SubtitlesFolderPath, "*.srt", SearchOption.AllDirectories);
            foreach (var srtFile in srtFiles)
            {
                var key = Path.GetFileNameWithoutExtension(srtFile);

                var lines = SubtitleUtils.GetSubtitleLinesByFile(srtFile);

                // Set full language line (concatenated)
                var fullText = string.Join(" ", lines.ConvertAll(l => l.text));
                LanguageHandler.SetLanguageLine(SubtitleUtils.GetLanguageKey(key), fullText);

                // Set per-line language lines
                for (int i = 0; i < lines.Count; i++)
                {
                    LanguageHandler.SetLanguageLine(SubtitleUtils.GetLanguageKey(key, i), lines[i].text);
                }
            }
        }

        /// <summary>
        /// Gets the language key from PDASubtitleLib required to display the correct text in the PDA log for a given base key (filename without extension).
        /// </summary>
        /// <param name="key">The base key (filename without extension)</param>
        /// <returns>The language key to use for the PDA log text</returns>
        public static string GetLanguageKey(string key)
        {
            return SubtitleUtils.GetLanguageKey(key);
        }

        /// <summary>
        /// Wrapper method for `PDALog.Add` which plays additionally plays subtitles for the given key (filename without extension) if they are loaded.
        /// </summary>
        /// <param name="key">The key corresponding to the PDA log and subtitles</param>
        public static void PDALogAdd(string key)
        {
            // Check if this log entry has already been added
            if (PDALog.Contains(key))
            {
                Plugin.Log.LogWarning($"PDA log entry for key: {key} already exists. Skipping Add.");
                return;
            }

            var lines = SubtitleUtils.GetSubtitleLinesByKey(key);
            if (lines == null || lines.Count == 0)
            {
                Plugin.Log.LogWarning($"No subtitles loaded for key: {key}");
                return;
            }

            try
            {
                PDALog.Add(key);
            }
            catch
            {
                Plugin.Log.LogWarning($"Failed to add PDA log entry for key: {key}");
            }

            CoroutineHost.StartCoroutine(PlaySubtitleLines(key, lines));
        }

        /// <summary>
        /// Coroutine to play subtitles for a given key and lines with timing. It will display each line using the appropriate language key at the correct time.
        /// </summary>
        /// <param name="baseKey">The base key (filename without extension) corresponding to the subtitles</param>
        /// <param name="lines">The array of SubtitleUtils.Entry to display</param>
        /// <returns>An IEnumerator for the coroutine</returns>
        public static IEnumerator PlaySubtitleLines(string baseKey, List<SubtitleUtils.Entry> lines)
        {
            float t0 = Time.realtimeSinceStartup;
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                float waitUntil = t0 + line.start;
                while (Time.realtimeSinceStartup < waitUntil)
                    yield return null;
                var key = SubtitleUtils.GetLanguageKey(baseKey, i);
                try
                {
                    Subtitles.Add(key);
                }
                catch
                {
                    Plugin.Log.LogWarning($"Failed to add subtitle for key: {key}");
                }
            }
        }
    }
}