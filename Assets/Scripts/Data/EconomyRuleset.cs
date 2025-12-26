using UnityEngine;

namespace F1Manager.Data
{
    [CreateAssetMenu(fileName = "EconomyRuleset_", menuName = "F1 Manager/Rules/Economy Ruleset", order = 10)]
    public class EconomyRuleset : ScriptableObject
    {
        [Header("Year")]
        [Min(1950)] public int year = 2026;

        [Header("Merch / Base (per year)")]
        [Min(0)] public long baseMerchIncomePerYear = 250000;

        [Header("Salaries (per year)")]
        [Min(0)] public long baseTeamSalaryPerYear = 30000000;

        [Header("Facilities costs per level (per year)")]
        [Min(0)] public long hqCostPerLevelPerYear = 3000000;
        [Min(0)] public long aeroDeptCostPerLevelPerYear = 2500000;
        [Min(0)] public long puDeptCostPerLevelPerYear = 2500000;
        [Min(0)] public long strategyCostPerLevelPerYear = 1500000;

        [Header("Simulation")]
        [Min(1)] public int ticksPerYear = 52; // semanal

        public long FixedFacilitiesCostPerYear(int hqLevel, int aeroLevel, int puLevel, int strategyLevel)
        {
            hqLevel = Mathf.Max(0, hqLevel);
            aeroLevel = Mathf.Max(0, aeroLevel);
            puLevel = Mathf.Max(0, puLevel);
            strategyLevel = Mathf.Max(0, strategyLevel);

            long total = 0;
            total += (long)hqLevel * hqCostPerLevelPerYear;
            total += (long)aeroLevel * aeroDeptCostPerLevelPerYear;
            total += (long)puLevel * puDeptCostPerLevelPerYear;
            total += (long)strategyLevel * strategyCostPerLevelPerYear;
            return total;
        }

        public long FixedCostPerYear(int hqLevel, int aeroLevel, int puLevel, int strategyLevel)
        {
            return baseTeamSalaryPerYear + FixedFacilitiesCostPerYear(hqLevel, aeroLevel, puLevel, strategyLevel);
        }

        public long BaseIncomePerYear()
        {
            return baseMerchIncomePerYear;
        }

        public long ToTickAmount(long perYear)
        {
            int tpy = Mathf.Max(1, ticksPerYear);
            return perYear / tpy;
        }

        private void OnValidate()
        {
            if (year < 1950) year = 1950;
            if (ticksPerYear < 1) ticksPerYear = 1;

            baseMerchIncomePerYear = Mathf.Max(0, (int)Mathf.Min(baseMerchIncomePerYear, int.MaxValue));
            // Obs: clamp acima é só pra evitar overflow no Inspector em alguns setups;
            // se você usa long grande, pode remover.
            // Mantive conservador, mas se quiser "realista", tire esse clamp.
        }
    }
}
