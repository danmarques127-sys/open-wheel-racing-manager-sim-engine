using F1Manager.Core.Save;
using F1Manager.Core.Finance;
using F1Manager.Data;

namespace F1Manager.Sim.Race
{
    public class SeasonPrizeService
    {
        private readonly SaveData save;
        private readonly PrizeDistributionRuleset prize;

        public SeasonPrizeService(
            SaveData save,
            PrizeDistributionRuleset prize)
        {
            this.save = save;
            this.prize = prize;
        }

        // paga prize de acordo com resultado final de construtores (1..N)
        // teamIdsByConstructorsPosition => array de TEAM IDs (string), em ordem de posição
        public void PaySeasonPrize(string[] teamIdsByConstructorsPosition)
        {
            if (teamIdsByConstructorsPosition == null || teamIdsByConstructorsPosition.Length == 0)
                return;

            for (int i = 0; i < teamIdsByConstructorsPosition.Length; i++)
            {
                string teamId = teamIdsByConstructorsPosition[i];
                if (string.IsNullOrWhiteSpace(teamId)) continue;

                int pos = i + 1;

                // PrizeDistributionRuleset atual retorna PRIZE em USD como long
                long amountUsd = (prize != null)
                    ? prize.GetPrizeForPositionUSD(pos)
                    : 0L;

                if (amountUsd <= 0L) continue;

                var finance = save.GetFinance(teamId);
                if (finance == null) continue;

                // Seu FinanceState.AddIncome usa LONG
                finance.AddIncome(amountUsd, IncomeType.Prize);
            }
        }
    }
}
