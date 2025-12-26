using System;
using F1Manager.Core.Sponsorship;
using F1Manager.Core.Save;

namespace F1Manager.Core.Finance
{
    public class FinanceService
    {
        private readonly SaveData save;

        public FinanceService(SaveData save)
        {
            this.save = save;
        }

        public void ApplySponsorIncomePerRace(int seasonYear, string teamId)
        {
            var f = save.GetFinance(teamId);
            if (f == null) return;

            long totalUsd = 0L;

            for (int i = 0; i < save.sponsorshipDeals.Count; i++)
            {
                var d = save.sponsorshipDeals[i];
                if (d == null) continue;

                // Funciona se d.teamId for int OU string
                if (d.teamId.ToString() != teamId) continue;

                if (!d.IsActiveForSeason(seasonYear)) continue;

                // valuePerRace está como float -> converter para long (USD)
                // Regra: arredonda para o inteiro mais próximo (evita truncar)
                if (d.valuePerRace > 0f)
                    totalUsd += (long)Math.Round(d.valuePerRace);
            }

            if (totalUsd > 0L)
                f.AddIncome(totalUsd, IncomeType.Sponsors);
        }

        public void ApplyMerchIncomePerRace(string teamId, float merchPerRace)
        {
            var f = save.GetFinance(teamId);
            if (f == null) return;

            if (merchPerRace <= 0f) return;

            // merchPerRace está float -> converter para long (USD)
            long merchUsd = (long)Math.Round(merchPerRace);

            if (merchUsd > 0L)
                f.AddIncome(merchUsd, IncomeType.Merch);
        }

        public bool TrySpend(string teamId, float amount, CostType type)
        {
            var f = save.GetFinance(teamId);
            if (f == null) return false;

            if (amount <= 0f) return true;

            // amount float -> converter para long (USD)
            long amountUsd = (long)Math.Round(amount);

            if (amountUsd <= 0L) return true;

            // Se o seu FinanceState.TrySpend já virou long:
            return f.TrySpend(amountUsd, type);

            // Se ainda for float (caso apareça erro), troque por:
            // return f.TrySpend((float)amountUsd, type);
        }
    }
}
