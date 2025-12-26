using F1Manager.Data;

namespace F1Manager.Core.Finance
{
    public class SponsorsManager
    {
        private readonly SponsorRuleset sponsorRuleset;
        private readonly EconomyRuleset economyRuleset;

        public SponsorsManager(SponsorRuleset sponsorRuleset, EconomyRuleset economyRuleset)
        {
            this.sponsorRuleset = sponsorRuleset;
            this.economyRuleset = economyRuleset;
        }

        /// <summary>
        /// Total sponsor income per year (USD) using tiers + performance + orders.
        /// </summary>
        public long GetSponsorIncomePerYear(TeamPerformanceSnapshot perf, bool includeOrders = true)
        {
            if (sponsorRuleset == null) return 0L;

            // Uses the updated SponsorRuleset logic (tiers + orders)
            return sponsorRuleset.GetTotalSponsorPerYear(perf, includeOrders);
        }

        /// <summary>
        /// Sponsor income per tick (weekly by default).
        /// </summary>
        public long GetSponsorIncomePerTick(TeamPerformanceSnapshot perf, bool includeOrders = true)
        {
            if (economyRuleset == null) return 0L;

            long perYear = GetSponsorIncomePerYear(perf, includeOrders);
            return economyRuleset.ToTickAmount(perYear);
        }
    }
}
