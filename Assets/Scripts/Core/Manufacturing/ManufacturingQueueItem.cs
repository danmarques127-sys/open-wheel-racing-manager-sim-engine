using System;
using UnityEngine;
using F1Manager.Data;

namespace F1Manager.Core.Manufacturing
{
    [Serializable]
    public class ManufacturingQueueItem
    {
        public string teamId;
        public CarPartType partType;

        [Header("Target Spec")]
        public float performanceLevelDelta;   // ganho aplicado ao part
        public float reliabilityDelta;        // pode ser negativo

        [Header("Timing")]
        public int weeksRemaining;

        [Header("Cost")]
        public float cost;

        [Header("Meta")]
        public string sourceProjectId; // opcional
        public int quantity = 1;

        public bool IsDone => weeksRemaining <= 0;
    }
}
