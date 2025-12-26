using System;
using System.Collections.Generic;
using UnityEngine;

using F1Manager.Core.Car;
using F1Manager.Core.Facilities;
using F1Manager.Core.Finance;
using F1Manager.Core.Manufacturing;
using F1Manager.Core.RnD;
using F1Manager.Core.Sponsorship;
using F1Manager.Core.Time;

// NEW: Driver Market / Academy / Scouting (persisted)
using F1Manager.Core.Market;
using F1Manager.Core.Academy;
using F1Manager.Core.Scouting;

namespace F1Manager.Core.Save
{
    [Serializable]
    public class SaveData
    {
        public string saveId = "slot1";
        public string createdAtIso = DateTime.UtcNow.ToString("o");

        public GameTimeState time = new GameTimeState();

        // Per team states (persisted)
        public List<TeamCarState> carByTeam = new List<TeamCarState>();
        public List<FinanceState> financeByTeam = new List<FinanceState>();
        public List<FacilityState> facilitiesByTeam = new List<FacilityState>();

        // Deals and queues (persisted)
        public List<SponsorshipDeal> sponsorshipDeals = new List<SponsorshipDeal>();
        public List<ActiveResearchState> activeResearch = new List<ActiveResearchState>();
        public List<ManufacturingQueueItem> manufacturingQueue = new List<ManufacturingQueueItem>();

        // ============================================================
        // NEW: Driver systems (persisted)
        // ============================================================

        [Header("Driver Systems (Market/Academy/Scouting)")]
        public DriverMarketState driverMarket = new DriverMarketState();
        public AcademyState academy = new AcademyState();
        public ScoutingState scouting = new ScoutingState();

        // ============================================================
        // Helpers (Find-or-Create) — ALWAYS returns a valid state
        // ============================================================

        public TeamCarState GetCar(string teamId)
        {
            string key = NormalizeTeamId(teamId);
            int specYear = GetCurrentSpecYear();

            // find
            for (int i = 0; i < carByTeam.Count; i++)
            {
                var c = carByTeam[i];
                if (c == null) continue;

                if (NormalizeTeamId(c.teamId) == key)
                {
                    c.teamId = key;                  // normalize stored id
                    c.EnsureAllPartsExist(specYear); // ✅ matches signature
                    return c;
                }
            }

            // create
            var created = new TeamCarState { teamId = key };
            created.EnsureAllPartsExist(specYear);    // ✅ matches signature
            carByTeam.Add(created);
            return created;
        }

        public FinanceState GetFinance(string teamId)
        {
            string key = NormalizeTeamId(teamId);

            // find
            for (int i = 0; i < financeByTeam.Count; i++)
            {
                var f = financeByTeam[i];
                if (f == null) continue;

                if (NormalizeTeamId(f.teamId) == key)
                {
                    f.teamId = key; // normalize stored id
                    return f;
                }
            }

            // create
            var created = new FinanceState { teamId = key };
            financeByTeam.Add(created);
            return created;
        }

        public FacilityState GetFacilities(string teamId)
        {
            string key = NormalizeTeamId(teamId);

            // find
            for (int i = 0; i < facilitiesByTeam.Count; i++)
            {
                var f = facilitiesByTeam[i];
                if (f == null) continue;

                if (NormalizeTeamId(f.teamId) == key)
                {
                    f.teamId = key; // normalize stored id
                    return f;
                }
            }

            // create
            var created = new FacilityState { teamId = key };
            facilitiesByTeam.Add(created);
            return created;
        }

        /// <summary>
        /// Use isso quando criar um save novo e quiser bootstrap para todos os times.
        /// (Opcional, mas ajuda a evitar estados faltando.)
        /// </summary>
        public void EnsureTeamExists(string teamId)
        {
            GetFinance(teamId);
            GetFacilities(teamId);
            GetCar(teamId);
        }

        // ============================================================
        // NEW: Driver systems helpers (always safe)
        // ============================================================

        /// <summary>
        /// Garante que os estados do mercado/academia/scouting existam (não-null)
        /// e que listas internas estejam instanciadas.
        /// Pode chamar no load ou no new game.
        /// </summary>
        public void EnsureDriverSystems()
        {
            if (driverMarket == null) driverMarket = new DriverMarketState();
            if (academy == null) academy = new AcademyState();
            if (scouting == null) scouting = new ScoutingState();

            // defensivo (caso serialização antiga traga null nas listas internas)
            if (driverMarket.drivers == null)
                driverMarket.drivers = new List<F1Manager.Core.Drivers.DriverState>();

            // ✅ força o tipo correto (evita colisão com algum DriverMarketEntry antigo/global)
            if (driverMarket.marketEntries == null)
                driverMarket.marketEntries = new List<F1Manager.Core.Market.DriverMarketEntry>();

            if (academy.teamAcademies == null)
                academy.teamAcademies = new List<AcademyTeamSlot>();

            if (scouting.teams == null)
                scouting.teams = new List<ScoutingTeamProgress>();
        }

        // ============================================================
        // Internal
        // ============================================================

        private int GetCurrentSpecYear()
        {
            // Por enquanto, fallback seguro.
            // Se seu GameTimeState tiver ano/temporada, troque aqui depois.
            return 2026;
        }

        private static string NormalizeTeamId(string teamId)
        {
            return string.IsNullOrWhiteSpace(teamId)
                ? "unknown"
                : teamId.Trim().ToLowerInvariant();
        }
    }
}
