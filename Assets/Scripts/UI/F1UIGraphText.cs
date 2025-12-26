using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace F1Manager.UI
{
    public static class F1UIGraphText
    {
        // Ex.: entradas (name, value). Faz barra ASCII.
        public static string BarChart(string title, IEnumerable<(string name, int value)> data, int maxBars = 10, int barWidth = 20)
        {
            var list = data?.ToList() ?? new List<(string name, int value)>();
            if (list.Count == 0) return $"{title}\n(no data)";

            list = list.OrderByDescending(x => x.value).Take(maxBars).ToList();
            int max = Math.Max(1, list.Max(x => x.value));

            var sb = new StringBuilder();
            sb.AppendLine(title);
            sb.AppendLine(new string('=', Math.Min(60, Math.Max(10, title.Length))));

            foreach (var (name, value) in list)
            {
                int len = (int)Math.Round((value / (double)max) * barWidth);
                len = Math.Clamp(len, 0, barWidth);
                sb.AppendLine($"{name.PadRight(16).Substring(0, 16)} | {new string('█', len).PadRight(barWidth)} {value}");
            }

            return sb.ToString();
        }
    }
}
