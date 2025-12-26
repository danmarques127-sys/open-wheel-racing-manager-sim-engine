using System;
using UnityEngine;

namespace F1Manager.Core.Finance
{
    [Serializable]
    public class FinanceState
    {
        [Header("Identity")]
        public string teamId;

        // =========================
        // CASH (USD) - single source of truth
        // =========================
        [Header("Cash (USD)")]
        [Tooltip("Current available budget in USD. Use long to avoid float drift.")]
        public long currentBudget = 0L;

        // =========================
        // TEAM FINANCE STATE (merged)
        // Operational knobs used by the simulation tick
        // =========================
        [Header("Weekly Budgets (USD per tick)")]
        [Tooltip("Weekly spending budget for R&D (simulation knob).")]
        public long weeklyRdSpend = 0L;

        [Tooltip("Weekly spending budget for staff/operations (simulation knob).")]
        public long weeklyStaffSpend = 0L;

        [Header("Facilities Levels")]
        [Tooltip("HQ facility level (affects fixed costs per year).")]
        public int hqLevel = 1;

        [Tooltip("Aero department level (affects fixed costs per year).")]
        public int aeroLevel = 1;

        [Tooltip("Power Unit department level (affects fixed costs per year).")]
        public int puLevel = 1;

        [Tooltip("Strategy department level (affects fixed costs per year).")]
        public int strategyLevel = 1;

        // =========================
        // INCOME YTD (USD)
        // =========================
        [Header("Income YTD (USD)")]
        public long incomeSponsors = 0L;
        public long incomePrize = 0L;
        public long incomeTVRights = 0L;
        public long incomeMerch = 0L;
        public long incomeOther = 0L;

        // =========================
        // COSTS YTD (USD)
        // =========================
        [Header("Costs YTD (USD)")]
        public long costSalaries = 0L;
        public long costUpgrades = 0L;
        public long costFacilities = 0L;
        public long costManufacturing = 0L;
        public long costOther = 0L;

        // =========================
        // API
        // =========================

        /// <summary>
        /// Adds income and records it in the appropriate YTD bucket.
        /// </summary>
        public void AddIncome(long amount, IncomeType type)
        {
            if (amount <= 0L) return;

            currentBudget += amount;

            switch (type)
            {
                case IncomeType.Sponsors: incomeSponsors += amount; break;
                case IncomeType.Prize: incomePrize += amount; break;
                case IncomeType.TVRights: incomeTVRights += amount; break;
                case IncomeType.Merch: incomeMerch += amount; break;
                case IncomeType.Other: incomeOther += amount; break;
            }
        }

        /// <summary>
        /// Tries to spend money, returns false if insufficient budget. Records spend in the appropriate YTD bucket.
        /// </summary>
        public bool TrySpend(long amount, CostType type)
        {
            if (amount <= 0L) return true;
            if (currentBudget < amount) return false;

            currentBudget -= amount;

            switch (type)
            {
                case CostType.Salaries: costSalaries += amount; break;
                case CostType.Upgrades: costUpgrades += amount; break;
                case CostType.Facilities: costFacilities += amount; break;
                case CostType.Manufacturing: costManufacturing += amount; break;
                case CostType.Other: costOther += amount; break;
            }

            return true;
        }

        /// <summary>
        /// Convenience: sets facility levels in one call (optional).
        /// </summary>
        public void SetFacilityLevels(int hq, int aero, int pu, int strategy)
        {
            hqLevel = Mathf.Max(0, hq);
            aeroLevel = Mathf.Max(0, aero);
            puLevel = Mathf.Max(0, pu);
            strategyLevel = Mathf.Max(0, strategy);
        }

        /// <summary>
        /// Resets YTD buckets (use at season start).
        /// </summary>
        public void ResetYTD()
        {
            incomeSponsors = 0L;
            incomePrize = 0L;
            incomeTVRights = 0L;
            incomeMerch = 0L;
            incomeOther = 0L;

            costSalaries = 0L;
            costUpgrades = 0L;
            costFacilities = 0L;
            costManufacturing = 0L;
            costOther = 0L;
        }
    }

    public enum IncomeType { Sponsors, Prize, TVRights, Merch, Other }
    public enum CostType { Salaries, Upgrades, Facilities, Manufacturing, Other }
}
