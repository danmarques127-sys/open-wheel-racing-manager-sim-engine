using UnityEngine;
using F1Manager.Core.Calendar;
using F1Manager.Data;

namespace F1Manager.Core.World
{
    [CreateAssetMenu(fileName = "RegulationsRuleset_", menuName = "F1 Manager/World/Regulations Ruleset", order = 1)]
    public class RegulationsRuleset : ScriptableObject
    {
        [Header("Identity")]
        public int seasonYear = 2026;
        public string engineEraId = "2026-hybrid-active-aero";

        [Header("Economy (future-ready)")]
        public float budgetCapMillions = 140f;
        public float costOfLivingMultiplier = 1.0f;

        [Header("Points")]
        public PointsRuleset pointsRuleset;

        [Header("Calendar")]
        public CalendarFormatRules calendarRules = new CalendarFormatRules();

        [Header("Sporting Rules (stubs)")]
        public bool parcFermeSimplified = true;
        public bool gridPenaltiesEnabled = false;

        [Header("Technical Rules (stubs)")]
        public int maxAeroUpgradesPerYear = 12;
        public int maxChassisUpgradesPerYear = 6;
        public int maxPUUpgradesPerYear = 4;

        [Header("Career")]
        public int minRetirementAge = 33;
        public int hardRetirementAge = 45;
        [Range(0f, 1f)] public float baseRetirementChancePerYear = 0.03f;
    }
}
