using System;
using UnityEngine;

namespace F1Manager.Core.Market
{
    [Serializable]
    public class DriverMarketEntry
    {
        public string driverId;

        // mercado “vê” um overall aproximado (scouting melhora isso)
        [Range(0f, 100f)]
        public float marketRatingEstimate = 50f;

        // expectativa de salário e duração
        [Min(0f)]
        public float expectedSalaryPerWeek = 0f;

        [Min(1)]
        public int expectedWeeks = 52;

        // chance de aceitar (base)
        [Range(0f, 1f)]
        public float willingnessBase01 = 0.5f;

        public bool isListed = true;
    }
}
