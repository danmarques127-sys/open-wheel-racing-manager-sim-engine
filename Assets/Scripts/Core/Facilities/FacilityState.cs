using System;
using UnityEngine;

namespace F1Manager.Core.Facilities
{
    [Serializable]
    public class FacilityState
    {
        public string teamId;

        [Header("Levels")]
        public int hqLevel = 1;
        public int aeroDeptLevel = 1;
        public int puDeptLevel = 1;
        public int strategyTeamLevel = 1;

        [Header("Efficiency Multipliers")]
        public float rndSpeedMultiplier = 1f;
        public float rndCostMultiplier = 1f;
        public float rndRiskMultiplier = 1f;
        public float manufacturingSpeedMultiplier = 1f;
        public float manufacturingCostMultiplier = 1f;
    }
}
