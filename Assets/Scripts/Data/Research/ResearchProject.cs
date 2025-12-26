using UnityEngine;

namespace F1Manager.Data
{
    [CreateAssetMenu(fileName = "ResearchProject_", menuName = "F1 Manager/RnD/Research Project", order = 30)]
    public class ResearchProject : ScriptableObject
    {
        public string projectId; // único (ex: "RND_FW_2026_A")
        public CarPartType partType;

        [Header("Outcome")]
        [Tooltip("Expected performance level gain if successful (e.g. +1.0, +2.5)")]
        public float targetPerformanceGain = 1f;

        [Tooltip("Reliability penalty risk factor; higher increases chance of issue/penalty.")]
        [Range(0f, 1f)] public float risk = 0.2f;

        [Header("Cost & Time")]
        public float baseCost = 2500000f;
        [Min(1)] public int durationWeeks = 6;

        [Header("Requirements")]
        [Range(0, 100)] public int expertiseRequired = 20;
    }
}
