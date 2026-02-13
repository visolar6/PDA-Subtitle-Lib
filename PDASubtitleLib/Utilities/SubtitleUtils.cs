using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using PDASubtitleLib.Handlers;

namespace PDASubtitleLib.Utilities
{
    public static class SubtitleUtils
    {
        public static readonly string LanguageKeyPrefix = "PDASubtitleLib_";

        private static readonly Regex SrtTime = new Regex(
            @"(?<h1>\d{2}):(?<m1>\d{2}):(?<s1>\d{2}),(?<ms1>\d{3})\s*-->\s*(?<h2>\d{2}):(?<m2>\d{2}):(?<s2>\d{2}),(?<ms2>\d{3})",
            RegexOptions.Compiled);

        public class Entry
        {
            public float start;
            public float end;
            public string text = string.Empty;
        }

        public static bool TryLoadSrt(string path, out List<Entry> entries)
        {
            entries = new List<Entry>();
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return false;

            try
            {
                var lines = File.ReadAllLines(path, Encoding.UTF8);
                int i = 0;
                while (i < lines.Length)
                {
                    // skip index line if numeric
                    if (int.TryParse(lines[i].Trim(), out _)) i++;
                    if (i >= lines.Length) break;

                    var m = SrtTime.Match(lines[i]);
                    if (!m.Success)
                    {
                        i++;
                        continue;
                    }
                    i++;

                    float start = ToSeconds(m, 1);
                    float end = ToSeconds(m, 2);

                    var sb = new StringBuilder();
                    while (i < lines.Length && !string.IsNullOrWhiteSpace(lines[i]))
                    {
                        if (sb.Length > 0) sb.AppendLine();
                        sb.Append(lines[i]);
                        i++;
                    }
                    // skip blank separator
                    while (i < lines.Length && string.IsNullOrWhiteSpace(lines[i])) i++;

                    entries.Add(new Entry { start = start, end = end, text = sb.ToString() });
                }

                return entries.Count > 0;
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning($"Subtitle parse failed: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get the full language key for a given base key (filename without extension).
        /// This is used for the PDA log text, not subtitles.
        /// </summary>
        /// <param name="key">The base key (filename without extension)</param>
        /// <returns>The full language key for the PDA log text</returns>
        public static string GetLanguageKey(string key)
        {
            return LanguageKeyPrefix + key;
        }

        /// <summary>
        /// Get the language key for a specific line index of a given base key (filename without extension).
        /// This is used for individual subtitle lines.
        /// </summary>
        /// <param name="key">The base key (filename without extension)</param>
        /// <param name="idx">The line index (0-based)</param>
        /// <returns>The language key for the specified subtitles line</returns>
        public static string GetLanguageKey(string key, int idx)
        {
            return $"{LanguageKeyPrefix}{key}_{idx}";
        }

        public static string StripLanguageKeyPrefix(string key)
        {
            if (key.StartsWith(LanguageKeyPrefix))
            {
                key = key.Substring(LanguageKeyPrefix.Length);
            }

            return key;
        }

        public static List<Entry> GetSubtitleLinesByKey(string key)
        {
            // strip prefix from key
            if (key.StartsWith(LanguageKeyPrefix))
            {
                key = key.Substring(LanguageKeyPrefix.Length);
            }
            var srtFile = Path.Combine(SubtitlesHandler.SubtitlesFolderPath, $"{key}.srt");
            return GetSubtitleLinesByFile(srtFile);
        }

        public static List<Entry> GetSubtitleLinesByFile(string srtFile)
        {
            if (TryLoadSrt(srtFile, out var entries))
            {
                // Convert entries to array of Entry
                var lines = new List<Entry>();
                foreach (var e in entries)
                {
                    lines.Add(new Entry { start = e.start, end = e.end, text = e.text });
                }
                return lines;
            }
            else
            {
                Plugin.Log.LogWarning($"Failed to load SRT file: {srtFile}");
                return new List<Entry>();
            }
        }

        public static float ToSeconds(Match m, int which)
        {
            int h = int.Parse(m.Groups[$"h{which}"].Value, CultureInfo.InvariantCulture);
            int mi = int.Parse(m.Groups[$"m{which}"].Value, CultureInfo.InvariantCulture);
            int s = int.Parse(m.Groups[$"s{which}"].Value, CultureInfo.InvariantCulture);
            int ms = int.Parse(m.Groups[$"ms{which}"].Value, CultureInfo.InvariantCulture);
            return h * 3600f + mi * 60f + s + ms / 1000f;
        }
    }
}
