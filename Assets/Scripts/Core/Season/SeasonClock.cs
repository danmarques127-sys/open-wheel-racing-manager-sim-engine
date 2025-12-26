using System;

namespace F1Manager.Core.Season
{
    [Serializable]
    public class SeasonClock
    {
        // Dia 1 = início do ano/season (você pode mapear para data real depois)
        public int day = 1;

        public void AdvanceDays(int days)
        {
            if (days <= 0) return;
            day += days;
            if (day < 1) day = 1;
        }
    }
}
