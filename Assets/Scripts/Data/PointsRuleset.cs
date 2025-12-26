using System.Collections.Generic;
using UnityEngine;

namespace F1Manager.Data
{
    [CreateAssetMenu(fileName = "PointsRuleset_", menuName = "F1 Manager/Rules/Points Ruleset", order = 50)]
    public class PointsRuleset : ScriptableObject
    {
        [Header("Name")]
        public string rulesetName = "F1 2026 (default)";

        [Header("Race Points (position -> points)")]
        [Tooltip("Index 0 = P1, index 1 = P2, ...")]
        public List<int> racePoints = new List<int> { 25, 18, 15, 12, 10, 8, 6, 4, 2, 1 };

        [Header("Sprint Points (position -> points)")]
        [Tooltip("Index 0 = P1, index 1 = P2, ...")]
        public List<int> sprintPoints = new List<int> { 8, 7, 6, 5, 4, 3, 2, 1 };

        [Header("Fastest Lap")]
        public bool fastestLapEnabled = true;
        public int fastestLapBonus = 1;

        [Tooltip("Se true, bônus de volta mais rápida só conta se piloto terminar no TOP N (padrão F1: top 10).")]
        public bool fastestLapTopNRequired = true;
        [Min(1)] public int fastestLapTopN = 10;

        public int GetRacePointsForPosition(int position)
        {
            if (position < 1) return 0;
            int idx = position - 1;
            if (idx < 0 || idx >= racePoints.Count) return 0;
            return Mathf.Max(0, racePoints[idx]);
        }

        public int GetSprintPointsForPosition(int position)
        {
            if (position < 1) return 0;
            int idx = position - 1;
            if (idx < 0 || idx >= sprintPoints.Count) return 0;
            return Mathf.Max(0, sprintPoints[idx]);
        }
    }
}
