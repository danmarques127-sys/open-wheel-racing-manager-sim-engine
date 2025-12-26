using System.Collections.Generic;
using UnityEngine;

namespace F1Manager.Data
{
    [CreateAssetMenu(fileName = "PrizeDistributionRuleset_", menuName = "F1 Manager/Rules/Prize Distribution", order = 82)]
    public class PrizeDistributionRuleset : ScriptableObject
    {
        [Header("Year")]
        public int year = 2026;

        [Header("Prize money by constructors position (Millions USD / season)")]
        [Tooltip("Index 0 = P1, 1 = P2, ... For 2026+ with 11 teams.")]
        public List<int> prizeMoneyMillions = new List<int>()
        {
            // 2026+ (11 teams) - values you provided
            175, 164, 152, 141, 130, 119, 107, 96, 85, 75, 65
        };

        /// <summary>
        /// Returns prize money in USD (not millions), using long for precision.
        /// </summary>
        public long GetPrizeForPositionUSD(int constructorsPos1Based)
        {
            int idx = constructorsPos1Based - 1;
            if (idx < 0 || idx >= prizeMoneyMillions.Count) return 0L;

            int millions = Mathf.Max(0, prizeMoneyMillions[idx]);
            return millions * 1_000_000L;
        }

        /// <summary>
        /// Returns prize money in "millions USD" (useful for UI/debug).
        /// </summary>
        public int GetPrizeMillionsForPosition(int constructorsPos1Based)
        {
            int idx = constructorsPos1Based - 1;
            if (idx < 0 || idx >= prizeMoneyMillions.Count) return 0;
            return Mathf.Max(0, prizeMoneyMillions[idx]);
        }

        private void OnValidate()
        {
            if (year < 1950) year = 1950;

            // Ensure list exists
            if (prizeMoneyMillions == null)
                prizeMoneyMillions = new List<int>();

            // If list is empty, set defaults (prevents "0 teams" issues)
            if (prizeMoneyMillions.Count == 0)
            {
                SetDefaults2026();
                return;
            }

            // Clamp negatives
            for (int i = 0; i < prizeMoneyMillions.Count; i++)
            {
                if (prizeMoneyMillions[i] < 0) prizeMoneyMillions[i] = 0;
            }
        }

        [ContextMenu("Set Defaults (2026+ / 11 teams)")]
        public void SetDefaults2026()
        {
            year = 2026;
            prizeMoneyMillions = new List<int>()
            {
                175, 164, 152, 141, 130, 119, 107, 96, 85, 75, 65
            };

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
