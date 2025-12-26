using System;

namespace F1Manager.Core.Time
{
    [Serializable]
    public class GameTimeState
    {
        public int currentSeasonYear = 2026;
        public int currentWeek = 1; // week in season timeline

        public void AdvanceWeeks(int weeks)
        {
            if (weeks < 1) return;
            currentWeek += weeks;
        }
    }
}
