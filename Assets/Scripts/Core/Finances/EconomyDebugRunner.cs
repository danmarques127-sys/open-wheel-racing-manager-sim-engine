using UnityEngine;
using F1Manager.Data;

namespace F1Manager.Core.Finance
{
    public class EconomyManager
    {
        private readonly EconomyRuleset economy;
        private readonly SponsorsManager sponsors;

        public EconomyManager(EconomyRuleset economyRuleset, SponsorRuleset sponsorRuleset)
        {
            economy = economyRuleset;
            sponsors = new SponsorsManager(sponsorRuleset, economyRuleset);
        }

        public struct TickReport
        {
            public string teamId;
            public long incomeTick;
            public long costsTick;
            public long delta;
            public long cashAfter;

            public override string ToString()
            {
                return $"[{teamId}] income={incomeTick} costs={costsTick} delta={delta} cash={cashAfter}";
            }
        }

        /// <summary>
        /// Aplica 1 tick semanal (ou o tick definido no EconomyRuleset).
        /// Income: Merch + Sponsors
        /// Costs: Fixed (salaries+facilities) + variable budgets (R&D + Staff)
        /// </summary>
        public TickReport ApplyWeeklyTick(FinanceState finance, TeamPerformanceSnapshot perf)
        {
            if (finance == null || economy == null)
                return default;

            // ===========
            // INCOME
            // ===========
            long merchTick = economy.ToTickAmount(economy.BaseIncomePerYear());
            long sponsorTick = sponsors.GetSponsorIncomePerTick(perf, includeOrders: true);

            // registra no ledger
            finance.AddIncome(merchTick, IncomeType.Merch);
            finance.AddIncome(sponsorTick, IncomeType.Sponsors);

            long incomeTick = merchTick + sponsorTick;

            // ===========
            // FIXED COSTS (per year -> per tick)
            // ===========
            long fixedCostPerYear = economy.FixedCostPerYear(
                finance.hqLevel,
                finance.aeroLevel,
                finance.puLevel,
                finance.strategyLevel
            );

            long fixedCostTick = economy.ToTickAmount(fixedCostPerYear);

            // Você pode dividir salários vs facilities se quiser detalhar mais.
            // Por enquanto, registramos tudo como Salaries (ou Facilities).
            // Vou registrar como Salaries para manter simples.
            bool okFixed = finance.TrySpend(fixedCostTick, CostType.Salaries);

            // ===========
            // VARIABLE COSTS (weekly budgets)
            // ===========
            long rd = ClampNonNegative(finance.weeklyRdSpend);
            long staff = ClampNonNegative(finance.weeklyStaffSpend);

            bool okRd = finance.TrySpend(rd, CostType.Upgrades);
            bool okStaff = finance.TrySpend(staff, CostType.Other);

            long costsTick = fixedCostTick + rd + staff;

            // Se faltou grana, você decide a regra (penalidade / dívida / cortar gastos).
            // Aqui vamos só logar. (Depois implementamos "debt" ou "auto-cut".)
            if (!okFixed || !okRd || !okStaff)
            {
                Debug.LogWarning($"[{finance.teamId}] Not enough budget. FixedOk={okFixed} RdOk={okRd} StaffOk={okStaff} BudgetNow={finance.currentBudget}");
            }

            long delta = incomeTick - costsTick;

            return new TickReport
            {
                teamId = finance.teamId,
                incomeTick = incomeTick,
                costsTick = costsTick,
                delta = delta,
                cashAfter = finance.currentBudget
            };
        }

        /// <summary>
        /// Estima o saldo anual (income - costs) com base no estado atual (facilities + budgets).
        /// Não inclui prize money (isso é Season End).
        /// </summary>
        public long EstimateAnnualNet(FinanceState finance, TeamPerformanceSnapshot perf)
        {
            if (finance == null || economy == null) return 0;

            long incomePerYear =
                economy.BaseIncomePerYear() +
                sponsors.GetSponsorIncomePerYear(perf, includeOrders: true);

            long fixedCostPerYear = economy.FixedCostPerYear(
                finance.hqLevel,
                finance.aeroLevel,
                finance.puLevel,
                finance.strategyLevel
            );

            long variablePerYear = 0;
            variablePerYear += ClampNonNegative(finance.weeklyRdSpend) * economy.ticksPerYear;
            variablePerYear += ClampNonNegative(finance.weeklyStaffSpend) * economy.ticksPerYear;

            return incomePerYear - fixedCostPerYear - variablePerYear;
        }

        private long ClampNonNegative(long v) => v < 0 ? 0 : v;
    }
}
