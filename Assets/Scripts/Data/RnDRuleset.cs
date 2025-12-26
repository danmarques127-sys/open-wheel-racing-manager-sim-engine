using UnityEngine;

namespace F1Manager.Data
{
    [CreateAssetMenu(fileName = "RnDRuleset_", menuName = "F1 Manager/Rules/RnD Ruleset", order = 81)]
    public class RnDRuleset : ScriptableObject
    {
        public int year = 2026;

        [Header("Research")]
        [Tooltip("Global multiplier for research duration (1=normal)")]
        public float researchDurationMultiplier = 1f;

        [Tooltip("Global multiplier for research cost (1=normal)")]
        public float researchCostMultiplier = 1f;

        [Tooltip("Global multiplier for risk (1=normal)")]
        public float researchRiskMultiplier = 1f;

        [Header("Manufacturing")]
        public float manufacturingDurationMultiplier = 1f;
        public float manufacturingCostMultiplier = 1f;

        [Header("Issue generation")]
        [Range(0f, 1f)] public float issueChanceBase = 0.15f;
        [Range(0f, 1f)] public float issueChancePerRisk = 0.35f;
    }
}
