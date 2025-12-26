using System;
using System.Collections.Generic;

namespace F1Manager.Core.Dev
{
    public enum DevArea
    {
        Aero,
        Chassis,
        PowerUnit,
        Reliability
    }

    [Serializable]
    public class DevItem
    {
        public DevArea area;
        public int startDay;
        public int durationDays;
        public float progress01;

        public bool IsComplete => durationDays > 0 && progress01 >= 1f;
    }

    [Serializable]
    public class DevelopmentQueue
    {
        public List<DevItem> items = new List<DevItem>();

        public void Add(DevArea area, int startDay, int durationDays)
        {
            items.Add(new DevItem
            {
                area = area,
                startDay = startDay,
                durationDays = Math.Max(1, durationDays),
                progress01 = 0f
            });
        }

        public void AdvanceDays(int currentDay)
        {
            foreach (var it in items)
            {
                if (it.IsComplete) continue;
                int elapsed = Math.Max(0, currentDay - it.startDay);
                it.progress01 = it.durationDays <= 0 ? 1f : Math.Clamp((float)elapsed / it.durationDays, 0f, 1f);
            }
        }
    }
}
