using System;
using System.Linq;
using UnityEngine;
using F1Manager.Core.Market;
using F1Manager.Core.Randomness;

namespace F1Manager.Core.Academy
{
    public sealed class AcademyService
    {
        private readonly AcademyState academy;
        private readonly DriverMarketState market;
        private readonly IRandom rng;

        public AcademyService(AcademyState academy, DriverMarketState market, IRandom rng)
        {
            this.academy = academy ?? throw new ArgumentNullException(nameof(academy));
            this.market = market ?? throw new ArgumentNullException(nameof(market));
            this.rng = rng ?? throw new ArgumentNullException(nameof(rng));
        }

        public void InitializeForSeason()
        {
            academy.teamAcademies = academy.teamAcademies ?? new System.Collections.Generic.List<AcademyTeamSlot>();
        }

        public void AdvanceWeek()
        {
            // treino semanal simples: moral + micro evolução (jovens)
            foreach (var team in academy.teamAcademies)
            {
                if (team == null) continue;

                foreach (var driverId in team.driverIds)
                {
                    var d = market.GetDriver(driverId);
                    if (d == null) continue;

                    // foco em jovens: se idade <= 23 ganha micro boost
                    if (d.age <= 23)
                    {
                        float boost = rng.RangeFloat(0.02f, 0.10f) * (d.stats.workEthic / 100f);
                        d.stats.pace = Mathf.Clamp(d.stats.pace + boost, 0f, 100f);
                        d.stats.consistency = Mathf.Clamp(d.stats.consistency + boost * 0.8f, 0f, 100f);
                        d.stats.racecraft = Mathf.Clamp(d.stats.racecraft + boost * 0.7f, 0f, 100f);
                    }

                    d.stats.morale = Mathf.Clamp(d.stats.morale + rng.RangeFloat(0.1f, 0.6f), 0f, 100f);
                }
            }
        }

        public void AddToAcademy(string teamId, string driverId)
        {
            if (string.IsNullOrEmpty(teamId) || string.IsNullOrEmpty(driverId)) return;

            var slot = academy.teamAcademies.FirstOrDefault(t => t != null && t.teamId == teamId);
            if (slot == null)
            {
                slot = new AcademyTeamSlot { teamId = teamId };
                academy.teamAcademies.Add(slot);
            }

            if (!slot.driverIds.Contains(driverId))
                slot.driverIds.Add(driverId);
        }

        public void RemoveFromAcademy(string teamId, string driverId)
        {
            var slot = academy.teamAcademies.FirstOrDefault(t => t != null && t.teamId == teamId);
            if (slot == null) return;
            slot.driverIds.Remove(driverId);
        }
    }
}
