using System;

namespace F1Manager.Core.World
{
    [Serializable]
    public class TeamState
    {
        public string teamId;
        public string teamName;
        public string country;

        public float prestige;     // 0..1
        public float budget;       // escala interna
        public float facility;     // 0..1
        public float carBase;      // 0..1

        public bool isGenerated;
        public int entrySeason;
    }
}
