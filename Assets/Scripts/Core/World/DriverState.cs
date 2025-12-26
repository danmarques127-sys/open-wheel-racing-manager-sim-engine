using System;

namespace F1Manager.Core.World
{
    [Serializable]
    public class DriverState
    {
        public string driverId;
        public string fullName;
        public string nationality;

        public int age;
        public int peakAge;            // idade do auge
        public float potential;        // 0..1 (teto)
        public float currentSkill;     // 0..1 (nível atual)

        // subskills 0..1
        public float pace;
        public float racecraft;
        public float consistency;
        public float wetSkill;
        public float tyreMgmt;
        public float starts;

        // personalidade / narrativa 0..1
        public float aggression;
        public float pressure;         // lida com pressão
        public float temperament;      // pavio curto
        public float professionalism;

        // carreira
        public bool isGenerated;
        public int debutSeason;
    }
}
