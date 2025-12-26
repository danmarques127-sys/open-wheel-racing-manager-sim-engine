using System;
using System.Text.RegularExpressions;

namespace F1Manager.Core.World
{
    public static class StableIdUtil
    {
        // Ex: "Red Bull Racing" -> "red_bull_racing"
        public static string Slugify(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "id_unknown";
            var s = input.Trim().ToLowerInvariant();
            s = Regex.Replace(s, @"[^a-z0-9]+", "_");
            s = Regex.Replace(s, @"_+", "_").Trim('_');
            return string.IsNullOrEmpty(s) ? "id_unknown" : s;
        }

        public static string EnsureNotEmpty(string id, string fallback)
        {
            if (!string.IsNullOrWhiteSpace(id)) return id;
            return string.IsNullOrWhiteSpace(fallback) ? Guid.NewGuid().ToString("N") : fallback;
        }
    }
}
