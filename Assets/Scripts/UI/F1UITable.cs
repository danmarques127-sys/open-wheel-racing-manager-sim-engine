using System;
using System.Collections.Generic;
using System.Text;

namespace F1Manager.UI
{
    public static class F1UITable
    {
        public static string Table(string title, params string[] lines)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(title))
            {
                sb.AppendLine(title);
                sb.AppendLine(new string('=', Math.Min(60, Math.Max(10, title.Length))));
            }

            for (int i = 0; i < lines.Length; i++)
                sb.AppendLine(lines[i] ?? "");

            return sb.ToString();
        }

        public static string Pad(string s, int width)
        {
            s ??= "";
            if (s.Length >= width) return s.Substring(0, width);
            return s.PadRight(width);
        }

        public static string Row(params (string text, int width)[] cols)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < cols.Length; i++)
            {
                sb.Append(Pad(cols[i].text, cols[i].width));
                if (i < cols.Length - 1) sb.Append("  ");
            }
            return sb.ToString();
        }

        public static string Divider(int totalWidth, char c = '-')
        {
            return new string(c, Math.Max(10, totalWidth));
        }

        public static List<string> MakeTopN<T>(IReadOnlyList<T> items, int n, Func<T, string> toLine)
        {
            var list = new List<string>();
            if (items == null) return list;

            int count = Math.Min(n, items.Count);
            for (int i = 0; i < count; i++)
                list.Add(toLine(items[i]));

            return list;
        }
    }
}
