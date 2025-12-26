using System;
using System.Collections.Generic;
using UnityEngine;

namespace F1Manager.Data
{
    [Serializable]
    public class CarAttributeWeights
    {
        [Range(-2f, 2f)] public float aeroEfficiency = 0f;
        [Range(-2f, 2f)] public float mechanicalGrip = 0f;
        [Range(-2f, 2f)] public float powerOutput = 0f;
        [Range(-2f, 2f)] public float reliability = 1f;
        [Range(-2f, 2f)] public float weight = 0f;
    }

    [CreateAssetMenu(fileName = "CarPartDefinition_", menuName = "F1 Manager/Car/Car Part Definition", order = 10)]
    public class CarPartDefinition : ScriptableObject
    {
        [Header("Identity")]
        public CarPartType partType;
        public CarPartCategory category;

        [Header("FIA Provenance (Passo 1)")]
        public List<FIASection> fiaSections = new List<FIASection>();

        [Header("Performance Model Weights (Passo 3)")]
        public CarAttributeWeights weights = new CarAttributeWeights();

        [Header("Wear Behavior")]
        [Tooltip("Base wear per race-weekend in % points (0-100).")]
        [Range(0f, 20f)] public float baseWearPerRace = 2f;

        [Header("Manufacturing & Costs")]
        public float baseManufactureCost = 100000f;
        [Min(1)] public int baseManufactureWeeks = 2;

        [Header("Research")]
        [Tooltip("Base research cost multiplier for this part type.")]
        [Range(0.1f, 5f)] public float researchCostFactor = 1f;

        [Tooltip("Base research duration multiplier for this part type.")]
        [Range(0.1f, 5f)] public float researchDurationFactor = 1f;

        [Tooltip("Base risk multiplier for this part type.")]
        [Range(0.1f, 5f)] public float researchRiskFactor = 1f;

        [Header("2026 Meta Flags")]
        [Tooltip("If true, this part is especially sensitive to 2026 active aero / efficiency meta.")]
        public bool isActiveAeroSensitive = false;

        [Tooltip("If true, this part is critical in hybrid 2026 balance (energy/power).")]
        public bool isHybridCritical = false;

        [Tooltip("If true, reliability issues here are more likely to produce DNFs/penalties.")]
        public bool isDNFCritical = false;
    }
}
