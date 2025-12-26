using System;
using System.Linq;
using UnityEngine;
using F1Manager.Core.Market;
using F1Manager.Core.Randomness;

namespace F1Manager.Core.Scouting
{
    public sealed class ScoutingService
    {
        private readonly ScoutingState scouting;
        private readonly DriverMarketState market;
        private readonly IRandom rng;

        public ScoutingService(ScoutingState scouting, DriverMarketState market, IRandom rng)
        {
            this.scouting = scouting ?? throw new ArgumentNullException(nameof(scouting));
            this.market = market ?? throw new ArgumentNullException(nameof(market));
            this.rng = rng ?? throw new ArgumentNullException(nameof(rng));
        }

        public void AdvanceWeek()
        {
            // progresso semanal: aumenta reveal e destrava flags
            foreach (var team in scouting.teams)
            {
                if (team == null) continue;

                foreach (var e in team.entries)
                {
                    if (e == null) continue;

                    e.reveal01 = Mathf.Clamp01(e.reveal01 + rng.RangeFloat(0.05f, 0.12f));

                    // thresholds de reveal
                    if (e.reveal01 >= 0.35f) e.revealedWorkEthic = true;
                    if (e.reveal01 >= 0.60f) e.revealedAdaptability = true;
                    if (e.reveal01 >= 0.85f) e.revealedPotential = true;

                    // quando scouting progride, você pode atualizar market estimate
                    var driver = market.GetDriver(e.driverId);
                    var entry = market.GetEntry(e.driverId);
                    if (driver != null && entry != null)
                    {
                        // melhora estimativa: vai aproximando do true overall conforme reveal01
                        float visible = driver.stats.OverallVisible;
                        float trueOverall = driver.stats.OverallTrue;
                        float improved = Mathf.Lerp(visible, trueOverall, e.reveal01);
                        entry.marketRatingEstimate = Mathf.Clamp(improved, 0f, 100f);
                    }
                }
            }
        }

        public void StartScouting(string teamId, string driverId)
        {
            if (string.IsNullOrEmpty(teamId) || string.IsNullOrEmpty(driverId)) return;

            var team = scouting.teams.FirstOrDefault(t => t != null && t.teamId == teamId);
            if (team == null)
            {
                team = new ScoutingTeamProgress { teamId = teamId };
                scouting.teams.Add(team);
            }

            var entry = team.entries.FirstOrDefault(x => x != null && x.driverId == driverId);
            if (entry == null)
            {
                team.entries.Add(new ScoutingProgressEntry
                {
                    driverId = driverId,
                    reveal01 = 0f,
                    revealedPotential = false,
                    revealedAdaptability = false,
                    revealedWorkEthic = false
                });
            }
        }

        public ScoutingProgressEntry GetScoutingInfo(string teamId, string driverId)
        {
            var team = scouting.teams.FirstOrDefault(t => t != null && t.teamId == teamId);
            if (team == null) return null;
            return team.entries.FirstOrDefault(e => e != null && e.driverId == driverId);
        }
    }
}
