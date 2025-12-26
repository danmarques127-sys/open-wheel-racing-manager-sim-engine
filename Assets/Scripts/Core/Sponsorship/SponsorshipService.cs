using System.Collections.Generic;
using F1Manager.Core.Save;

namespace F1Manager.Core.Sponsorship
{
    public class SponsorshipService
    {
        private readonly SaveData save;

        public SponsorshipService(SaveData save)
        {
            this.save = save;
        }

        public void AddOrReplaceDeal(SponsorshipDeal deal)
        {
            // substitui deal com mesmo sponsorId+teamId
            for (int i = 0; i < save.sponsorshipDeals.Count; i++)
            {
                var d = save.sponsorshipDeals[i];
                if (d == null) continue;

                if (d.teamId == deal.teamId && d.sponsorId == deal.sponsorId)
                {
                    save.sponsorshipDeals[i] = deal;
                    return;
                }
            }

            save.sponsorshipDeals.Add(deal);
        }

        public List<SponsorshipDeal> GetDealsForTeam(string teamId, int seasonYear)
        {
            var list = new List<SponsorshipDeal>();

            for (int i = 0; i < save.sponsorshipDeals.Count; i++)
            {
                var d = save.sponsorshipDeals[i];
                if (d == null) continue;

                if (d.teamId != teamId) continue;
                if (!d.IsActiveForSeason(seasonYear)) continue;

                list.Add(d);
            }

            return list;
        }
    }
}
