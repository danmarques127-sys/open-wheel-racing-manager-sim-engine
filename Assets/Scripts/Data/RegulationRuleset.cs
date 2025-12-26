using System;
using System.Collections.Generic;
using UnityEngine;
using F1Manager.Data;

namespace F1Manager.Data
{
    [Serializable]
    public class PartCostMultiplier
    {
        public CarPartType partType;
        [Range(0.1f, 5f)] public float multiplier = 1f;
    }

    [Serializable]
    public class PartResearchLimits
    {
        public CarPartType partType;
        [Tooltip("Max expected performance gain per project for this part.")]
        public float maxGainPerProject = 2f;

        [Tooltip("Soft cap for total performance over a season (optional design knob).")]
        public float softSeasonCap = 10f;
    }

    [CreateAssetMenu(fileName = "RegulationRuleset_", menuName = "F1 Manager/Rules/Regulation Ruleset", order = 90)]
    public class RegulationRuleset : ScriptableObject
    {
        public int year = 2026;

        [Header("Core Regulations")]
        public float minimumWeightKg = 0f; // você preenche com valor real quando quiser
        public bool activeAeroAllowed = true;

        [Header("Economy Multipliers (per part)")]
        public List<PartCostMultiplier> manufactureCostByPart = new List<PartCostMultiplier>();

        [Header("Research Rules")]
        public float globalResearchRiskMultiplier = 1f;
        public float globalResearchCostMultiplier = 1f;
        public float globalResearchDurationMultiplier = 1f;

        public List<PartResearchLimits> researchLimitsByPart = new List<PartResearchLimits>();

        public float GetManufactureCostMultiplier(CarPartType partType)
        {
            for (int i = 0; i < manufactureCostByPart.Count; i++)
                if (manufactureCostByPart[i].partType == partType) return manufactureCostByPart[i].multiplier;
            return 1f;
        }

        public PartResearchLimits GetResearchLimits(CarPartType partType)
        {
            for (int i = 0; i < researchLimitsByPart.Count; i++)
                if (researchLimitsByPart[i].partType == partType) return researchLimitsByPart[i];
            return null;
        }
    }
}
