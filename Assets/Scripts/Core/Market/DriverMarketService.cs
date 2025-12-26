using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using F1Manager.Core.Randomness;
using F1Manager.Core.Drivers;
using F1Manager.Core.Contracts;

namespace F1Manager.Core.Market
{
    public sealed class DriverMarketService
    {
        private readonly DriverMarketState state;
        private readonly IRandom rng;

        // regras simples (depois você pluga com RulesProvider)
        private const int WeeksPerSeason = 52;

        public DriverMarketService(DriverMarketState state, IRandom rng)
        {
            this.state = state ?? throw new ArgumentNullException(nameof(state));
            this.rng = rng ?? throw new ArgumentNullException(nameof(rng));
        }

        // =========================
        // INIT / GENERATION
        // =========================

        public void InitializeForSeason(int seasonYear, int initialF2Count = 24, int initialF3Count = 30, int initialF4Count = 30)
        {
            // Se já tem drivers, não recria (preserva save)
            if (state.drivers != null && state.drivers.Count > 0)
            {
                RebuildMarketEntries();
                return;
            }

            state.drivers = state.drivers ?? new List<DriverState>();
            state.marketEntries = state.marketEntries ?? new List<DriverMarketEntry>();

            // Cria pool inicial (F2/F3/F4 + alguns free agents “veteranos”)
            for (int i = 0; i < initialF2Count; i++)
                state.drivers.Add(GenerateYoungDriver(DriverTier.F2));

            for (int i = 0; i < initialF3Count; i++)
                state.drivers.Add(GenerateYoungDriver(DriverTier.F3));

            for (int i = 0; i < initialF4Count; i++)
                state.drivers.Add(GenerateYoungDriver(DriverTier.F4));

            // Alguns free agents experientes (sem equipe)
            for (int i = 0; i < 10; i++)
                state.drivers.Add(GenerateVeteranFreeAgent());

            RebuildMarketEntries();
        }

        private void RebuildMarketEntries()
        {
            state.marketEntries.Clear();

            foreach (var d in state.drivers)
            {
                if (d == null) continue;

                // Só lista se for free agent
                if (d.IsFreeAgent)
                {
                    state.marketEntries.Add(BuildEntryForDriver(d));
                }
            }
        }

        private DriverMarketEntry BuildEntryForDriver(DriverState d)
        {
            float visible = Mathf.Clamp(d.stats.OverallVisible, 0f, 100f);

            // salário e duração estimados (bem simples agora)
            float salary = Mathf.Clamp(visible * 1500f + rng.RangeFloat(-5000f, 5000f), 2000f, 250000f);
            int weeks = Mathf.Clamp(rng.RangeInt(WeeksPerSeason / 2, WeeksPerSeason * 2), 10, WeeksPerSeason * 3);

            return new DriverMarketEntry
            {
                driverId = d.driverId,
                marketRatingEstimate = visible,
                expectedSalaryPerWeek = salary,
                expectedWeeks = weeks,
                willingnessBase01 = Mathf.Clamp01(0.30f + (d.stats.morale / 200f) + rng.RangeFloat(-0.10f, 0.10f)),
                isListed = true
            };
        }

        // =========================
        // WEEKLY / YEARLY PROGRESSION
        // =========================

        public void AdvanceWeek()
        {
            // reduzir weeksRemaining dos contratos
            foreach (var d in state.drivers)
            {
                if (d == null) continue;

                if (d.contract.IsActive)
                {
                    d.contract.weeksRemaining -= 1;
                    if (d.contract.weeksRemaining <= 0)
                    {
                        // contrato acabou → vira free agent
                        d.contract = DriverContractState.FreeAgent();
                    }
                }

                // moral drift leve
                d.stats.morale = Mathf.Clamp(d.stats.morale + rng.RangeFloat(-1.0f, 1.0f), 0f, 100f);
            }

            // garantir que free agents estejam listados
            RebuildMarketEntries();
        }

        public void EndSeasonAndAdvanceYear()
        {
            foreach (var d in state.drivers)
            {
                if (d == null) continue;

                d.age += 1;

                // evolução anual: depende de potencial + workEthic + idade
                float ageFactor =
                    d.age <= 20 ? 1.20f :
                    d.age <= 25 ? 1.00f :
                    d.age <= 30 ? 0.60f :
                    d.age <= 35 ? 0.20f :
                    -0.30f;

                float growth = (d.stats.potential / 100f) * (0.6f + d.stats.workEthic / 200f) * ageFactor;
                float delta = growth * rng.RangeFloat(0.5f, 1.2f) * 4.0f; // 0..~5

                // distribui crescimento/declínio
                d.stats.pace = Mathf.Clamp(d.stats.pace + delta * 0.30f, 0f, 100f);
                d.stats.consistency = Mathf.Clamp(d.stats.consistency + delta * 0.25f, 0f, 100f);
                d.stats.racecraft = Mathf.Clamp(d.stats.racecraft + delta * 0.20f, 0f, 100f);
                d.stats.tyreMgmt = Mathf.Clamp(d.stats.tyreMgmt + delta * 0.15f, 0f, 100f);
                d.stats.wetSkill = Mathf.Clamp(d.stats.wetSkill + delta * 0.10f, 0f, 100f);

                // “aposentadoria” simples
                if (d.age >= 38 && rng.Chance(0.20f + (d.age - 38) * 0.10f))
                {
                    // remove do mundo (ou você pode mover pra “Hall of Fame” depois)
                    d.contract = DriverContractState.FreeAgent();
                    d.tier = DriverTier.Regen; // marca como “fora” (depois você decide)
                    d.stats.morale = 0f;
                }

                d.Clamp();
            }

            // repõe pool com regen
            EnsureMinimumFreeAgents(minCount: 40);

            RebuildMarketEntries();
        }

        public void EnsureMinimumFreeAgents(int minCount)
        {
            int freeAgents = state.drivers.Count(d => d != null && d.IsFreeAgent);
            int missing = Mathf.Max(0, minCount - freeAgents);

            for (int i = 0; i < missing; i++)
            {
                var regen = GenerateRegenDriver();
                state.drivers.Add(regen);
            }
        }

        // =========================
        // SIGN / RELEASE
        // =========================

        public bool CanSign(string driverId)
        {
            var d = state.GetDriver(driverId);
            return d != null && d.IsFreeAgent;
        }

        public bool SignDriver(string teamId, string driverId, float salaryPerWeek, int weeks, ContractClause clause)
        {
            if (string.IsNullOrEmpty(teamId)) return false;

            var d = state.GetDriver(driverId);
            if (d == null) return false;
            if (!d.IsFreeAgent) return false;

            d.contract = new DriverContractState
            {
                status = ContractStatus.Signed,
                teamId = teamId,
                salaryPerWeek = Mathf.Max(0f, salaryPerWeek),
                weeksRemaining = Mathf.Max(1, weeks),
                clause = clause
            };

            // Ao assinar, moral sobe um pouco (depende)
            d.stats.morale = Mathf.Clamp(d.stats.morale + rng.RangeFloat(2f, 8f), 0f, 100f);

            RebuildMarketEntries();
            return true;
        }

        public bool ReleaseDriver(string driverId)
        {
            var d = state.GetDriver(driverId);
            if (d == null) return false;

            // vira free agent
            d.contract = DriverContractState.FreeAgent();
            d.stats.morale = Mathf.Clamp(d.stats.morale - rng.RangeFloat(5f, 15f), 0f, 100f);

            RebuildMarketEntries();
            return true;
        }

        // =========================
        // DEBUG HELPERS
        // =========================

        public void DebugPrintTopFreeAgents(int count = 10)
        {
            var list = state.marketEntries
                .Where(e => e != null && e.isListed)
                .OrderByDescending(e => e.marketRatingEstimate)
                .Take(count)
                .ToList();

            Debug.Log($"[Market] Top {list.Count} Free Agents:");
            foreach (var e in list)
            {
                var d = state.GetDriver(e.driverId);
                if (d == null) continue;
                Debug.Log($" - {d.FullName} ({d.tier}) age {d.age} | est {e.marketRatingEstimate:0.0} | salary/w {e.expectedSalaryPerWeek:0} | weeks {e.expectedWeeks}");
            }
        }

        public void DebugPrintTeamContracts(string teamId)
        {
            var list = state.drivers
                .Where(d => d != null && d.contract.IsActive && d.contract.teamId == teamId)
                .OrderByDescending(d => d.stats.OverallVisible)
                .ToList();

            Debug.Log($"[Contracts] Team {teamId} contracts: {list.Count}");
            foreach (var d in list)
            {
                Debug.Log($" - {d.FullName} | vis {d.stats.OverallVisible:0.0} | weeks {d.contract.weeksRemaining} | salary/w {d.contract.salaryPerWeek:0}");
            }
        }

        // =========================
        // GENERATORS
        // =========================

        private DriverState GenerateYoungDriver(DriverTier tier)
        {
            int age = tier switch
            {
                DriverTier.F2 => rng.RangeInt(18, 24),
                DriverTier.F3 => rng.RangeInt(16, 22),
                DriverTier.F4 => rng.RangeInt(15, 21),
                _ => rng.RangeInt(16, 24)
            };

            float baseSkill = tier switch
            {
                DriverTier.F2 => rng.RangeFloat(55f, 78f),
                DriverTier.F3 => rng.RangeFloat(45f, 70f),
                DriverTier.F4 => rng.RangeFloat(35f, 65f),
                _ => rng.RangeFloat(40f, 75f)
            };

            var stats = new DriverStats
            {
                pace = baseSkill + rng.RangeFloat(-4f, 4f),
                consistency = baseSkill + rng.RangeFloat(-6f, 6f),
                racecraft = baseSkill + rng.RangeFloat(-6f, 6f),
                tyreMgmt = baseSkill + rng.RangeFloat(-8f, 8f),
                wetSkill = baseSkill + rng.RangeFloat(-10f, 10f),
                starts = baseSkill + rng.RangeFloat(-10f, 10f),
                morale = rng.RangeFloat(45f, 70f),

                potential = rng.RangeFloat(60f, 98f),
                adaptability = rng.RangeFloat(40f, 90f),
                workEthic = rng.RangeFloat(35f, 95f)
            };
            stats.ClampAll();

            var name = NameGen();
            return new DriverState
            {
                driverId = NewDriverId("drv"),
                firstName = name.first,
                lastName = name.last,
                nationality = name.nat,
                age = age,
                tier = tier,
                stats = stats,
                contract = DriverContractState.FreeAgent(),
                careerWins = 0,
                careerPodiums = 0,
                careerPoles = 0,
                careerTitles = 0
            };
        }

        private DriverState GenerateVeteranFreeAgent()
        {
            int age = rng.RangeInt(26, 36);
            float baseSkill = rng.RangeFloat(58f, 85f);

            var stats = new DriverStats
            {
                pace = baseSkill + rng.RangeFloat(-3f, 3f),
                consistency = baseSkill + rng.RangeFloat(-4f, 4f),
                racecraft = baseSkill + rng.RangeFloat(-4f, 4f),
                tyreMgmt = baseSkill + rng.RangeFloat(-6f, 6f),
                wetSkill = baseSkill + rng.RangeFloat(-6f, 6f),
                starts = baseSkill + rng.RangeFloat(-6f, 6f),
                morale = rng.RangeFloat(40f, 65f),

                potential = rng.RangeFloat(40f, 80f),
                adaptability = rng.RangeFloat(45f, 85f),
                workEthic = rng.RangeFloat(35f, 80f)
            };
            stats.ClampAll();

            var name = NameGen();
            return new DriverState
            {
                driverId = NewDriverId("vfa"),
                firstName = name.first,
                lastName = name.last,
                nationality = name.nat,
                age = age,
                tier = DriverTier.Regen, // “veterano fora do grid” (você pode criar outro tier depois)
                stats = stats,
                contract = DriverContractState.FreeAgent()
            };
        }

        private DriverState GenerateRegenDriver()
        {
            int age = rng.RangeInt(17, 21);
            float baseSkill = rng.RangeFloat(42f, 68f);

            var stats = new DriverStats
            {
                pace = baseSkill + rng.RangeFloat(-6f, 6f),
                consistency = baseSkill + rng.RangeFloat(-8f, 8f),
                racecraft = baseSkill + rng.RangeFloat(-8f, 8f),
                tyreMgmt = baseSkill + rng.RangeFloat(-10f, 10f),
                wetSkill = baseSkill + rng.RangeFloat(-12f, 12f),
                starts = baseSkill + rng.RangeFloat(-12f, 12f),
                morale = rng.RangeFloat(45f, 75f),

                potential = rng.RangeFloat(70f, 99f),
                adaptability = rng.RangeFloat(35f, 95f),
                workEthic = rng.RangeFloat(40f, 98f)
            };
            stats.ClampAll();

            var name = NameGen();
            return new DriverState
            {
                driverId = NewDriverId("reg"),
                firstName = name.first,
                lastName = name.last,
                nationality = name.nat,
                age = age,
                tier = DriverTier.Regen,
                stats = stats,
                contract = DriverContractState.FreeAgent()
            };
        }

        private string NewDriverId(string prefix)
        {
            state.regenCounter += 1;
            return $"{prefix}_{state.regenCounter:000000}";
        }

        private (string first, string last, string nat) NameGen()
        {
            // bem simples por enquanto (você pode trocar por listas grandes depois)
            string[] first = { "Alex", "Luca", "Noah", "Leo", "Mateo", "Enzo", "Theo", "Max", "Arthur", "Daniel", "Gabriel", "Victor" };
            string[] last = { "Silva", "Rossi", "Bennett", "Klein", "Novak", "Moreau", "Sato", "Almeida", "Costa", "Schmidt", "Pereira", "Martins" };
            string[] nat = { "BRA", "ITA", "GBR", "DEU", "FRA", "JPN", "ESP", "PRT", "NLD", "AUS" };

            return (first[rng.RangeInt(0, first.Length)],
                    last[rng.RangeInt(0, last.Length)],
                    nat[rng.RangeInt(0, nat.Length)]);
        }
    }
}
