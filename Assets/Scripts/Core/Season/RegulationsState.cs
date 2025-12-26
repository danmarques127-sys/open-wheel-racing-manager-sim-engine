using System;

namespace F1Manager.Core.Season
{
    [Serializable]
    public class RegulationsState
    {
        public int season;

        public float aeroWeight;         // 0..1
        public float powerWeight;        // 0..1
        public float reliabilityWeight;  // 0..1

        public float costCap;            // escala interna 0..1
        public bool sprintWeekendEnabled;
        public int sprintWeekendsPerSeason;

        public float changeVolatility;   // 0..1
    }
}
