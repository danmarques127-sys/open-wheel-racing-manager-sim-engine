using System;
using System.Collections.Generic;
using UnityEngine;

namespace F1Manager.Data
{
    // =========================
    //  DEALS / TIERS
    // =========================
    [System.Serializable]
    public class SponsorTier
    {
        [Tooltip("Nome do tier (ex: 'Title', 'Main', 'Secondary', 'Minor')")]
        public string tierName = "Main";

        [Header("Base Deal (per year)")]
        [Tooltip("Pagamento garantido por ano (independente de performance)")]
        public long guaranteedPerYear = 20000000;

        [Header("Performance Bonus (per year)")]
        [Tooltip("Bônus por ano que depende da posição no campeonato (distribuído por ticks)")]
        public long performanceBonusPerYear = 15000000;

        [Tooltip("Se a equipe estiver no TOP X do campeonato, considera 'indo bem'")]
        [Range(1, 30)] public int topPositionThreshold = 6;

        [Tooltip("Multiplicador do bônus quando está no TOP (1.0 = normal, 1.5 = mais agressivo)")]
        [Range(0f, 5f)] public float topMultiplier = 1.0f;

        [Tooltip("Multiplicador do bônus quando está fora do TOP (0.0 a 1.0). Ex: 0.3 = recebe 30% do bônus.")]
        [Range(0f, 1f)] public float outsideTopMultiplier = 0.3f;
    }

    // =========================
    //  ORDERS / OBJECTIVES
    // =========================
    public enum SponsorOrderType
    {
        ChampionshipPosition = 0, // Ex: terminar Top 6
        Points = 1,               // Ex: fazer 250 pontos
        Podiums = 2,              // Ex: conseguir 8 pódios
        Wins = 3                  // Ex: conseguir 3 vitórias
    }

    [System.Serializable]
    public class SponsorOrder
    {
        [Tooltip("Nome curto do objetivo (aparece na UI depois)")]
        public string orderName = "Finish Top 6";

        public SponsorOrderType type = SponsorOrderType.ChampionshipPosition;

        [Header("Target")]
        [Tooltip("Alvo do objetivo. Ex: Top 6 => target=6. Points => target=250. Podiums => target=8.")]
        public int target = 6;

        [Header("Payout (per year)")]
        [Tooltip("Bônus por ano ao cumprir o objetivo (você pode pagar no fim do ano ou distribuir por ticks)")]
        public long bonusPerYear = 10000000;

        [Tooltip("Se true, paga proporcional ao progresso (ex: 50% do target => 50% do bônus). Se false, só paga se cumprir.")]
        public bool proportional = true;

        [Tooltip("Se true, esse objetivo é avaliado e pago apenas no fim do ano. Se false, pode pagar por tick (progresso).")]
        public bool endOfSeasonOnly = true;
    }

    // =========================
    //  PERFORMANCE SNAPSHOT
    //  (runtime pode criar e passar)
    // =========================
    [System.Serializable]
    public struct TeamPerformanceSnapshot
    {
        public string teamId;

        [Tooltip("Posição atual no campeonato (1 = líder)")]
        public int championshipPosition;

        [Tooltip("Pontos atuais na temporada")]
        public int points;

        [Tooltip("Pódios atuais na temporada")]
        public int podiums;

        [Tooltip("Vitórias atuais na temporada")]
        public int wins;

        public TeamPerformanceSnapshot(string teamId, int championshipPosition, int points, int podiums, int wins)
        {
            this.teamId = teamId;
            this.championshipPosition = championshipPosition;
            this.points = points;
            this.podiums = podiums;
            this.wins = wins;
        }
    }

    // =========================
    //  GENERATION SUPPORT TYPES
    //  (para SponsorGenerator)
    // =========================
    [System.Serializable]
    public struct IndustryWeight
    {
        // IMPORTANTE: usando o enum que EXISTE no seu projeto (Core.Finance)
        public F1Manager.Core.Finance.SponsorIndustry industry;

        [Range(0f, 10f)] public float weight;
    }

    // =========================
    //  RULESET
    // =========================
    [CreateAssetMenu(fileName = "SponsorRuleset_", menuName = "F1 Manager/Rules/Sponsor Ruleset", order = 11)]
    public class SponsorRuleset : ScriptableObject
    {
        [Header("Year")]
        [Min(1950)] public int year = 2026;

        // ==========================================================
        // GENERATION / MARKET (necessário pro SponsorGenerator)
        // ==========================================================
        [Header("Generation: New Brands per Season")]
        [Min(0)] public int minNewBrandsPerSeason = 6;
        [Min(0)] public int maxNewBrandsPerSeason = 12;

        [Header("Generation: Deal Length (years)")]
        [Min(1)] public int minYears = 1;
        [Min(1)] public int maxYears = 3;

        [Header("Generation: Money Multipliers")]
        [Min(0f)] public float basePayMultiplier = 1.0f;
        [Min(0f)] public float bonusMultiplier = 1.0f;

        [Header("Generation: Countries")]
        public List<string> countries = new List<string>()
        {
            "USA","UK","Germany","Italy","France","Japan","Netherlands","Spain","Brazil","UAE","Switzerland","Canada","Australia"
        };

        [Header("Generation: Name Parts")]
        public List<string> namePrefixes = new List<string>()
        {
            "Nova","Apex","Orion","Vertex","Titan","Pulse","Helix","Summit","Prime","Eclipse","Atlas","Zenith","Hyper","Nex"
        };

        public List<string> nameSuffixes = new List<string>()
        {
            "Group","Works","Labs","Energy","Motors","Capital","Dynamics","Industries","Systems","Holdings","Foods","Mobile","Sports"
        };

        [Header("Generation: Industry Weights")]
        public List<IndustryWeight> industryWeights = new List<IndustryWeight>()
        {
            // Use SOMENTE os enums que existem no seu SponsorIndustry (Core.Finance)
            new IndustryWeight { industry = F1Manager.Core.Finance.SponsorIndustry.ConsumerGoods, weight = 1.0f },
            new IndustryWeight { industry = F1Manager.Core.Finance.SponsorIndustry.Finance,       weight = 0.9f },

            // REMOVIDOS: Technology e Aviation (não existem no seu enum atual)
            // Se quiser depois, adicione esses valores no enum SponsorIndustry e reintroduza aqui.

            new IndustryWeight { industry = F1Manager.Core.Finance.SponsorIndustry.Energy,        weight = 0.7f },
            new IndustryWeight { industry = F1Manager.Core.Finance.SponsorIndustry.Automotive,    weight = 0.6f },
            new IndustryWeight { industry = F1Manager.Core.Finance.SponsorIndustry.Telecom,       weight = 0.6f },
        };

        // =========================
        // ECONOMY: DEALS / TIERS
        // =========================
        [Header("Deals / Tiers")]
        public SponsorTier[] tiers;

        [Header("Sponsor Orders / Objectives")]
        public SponsorOrder[] orders;

        // -------------------------
        // DEALS (GARANTIDO)
        // -------------------------
        public long GetGuaranteedPerYearTotal()
        {
            long sum = 0;
            if (tiers == null) return 0;

            for (int i = 0; i < tiers.Length; i++)
            {
                var t = tiers[i];
                if (t == null) continue;
                if (t.guaranteedPerYear < 0) continue;
                sum += t.guaranteedPerYear;
            }

            return sum;
        }

        // -------------------------
        // DEALS (BÔNUS PERFORMANCE)
        // -------------------------
        public long GetPerformancePerYearTotal(int teamChampionshipPosition)
        {
            long sum = 0;
            if (tiers == null) return 0;

            int pos = Mathf.Max(1, teamChampionshipPosition);

            for (int i = 0; i < tiers.Length; i++)
            {
                var t = tiers[i];
                if (t == null) continue;

                long baseBonus = Math.Max(0L, t.performanceBonusPerYear);
                bool inTop = pos <= Mathf.Max(1, t.topPositionThreshold);

                float mul = inTop
                    ? Mathf.Max(0f, t.topMultiplier)
                    : Mathf.Clamp01(t.outsideTopMultiplier);

                long applied = (long)(baseBonus * mul);
                if (applied < 0) applied = 0;

                sum += applied;
            }

            return sum;
        }

        // -------------------------
        // ORDERS / OBJECTIVES
        // (BÔNUS EXTRA)
        // -------------------------
        public long GetOrdersBonusPerYear(TeamPerformanceSnapshot snapshot, bool includeEndOfSeasonOnly = true, bool includeProgressive = true)
        {
            long sum = 0;
            if (orders == null) return 0;

            for (int i = 0; i < orders.Length; i++)
            {
                var o = orders[i];
                if (o == null) continue;
                if (o.bonusPerYear <= 0) continue;

                if (o.endOfSeasonOnly && !includeEndOfSeasonOnly) continue;
                if (!o.endOfSeasonOnly && !includeProgressive) continue;

                float progress01 = EvaluateOrderProgress01(o, snapshot);

                if (!o.proportional)
                {
                    if (progress01 >= 1f)
                        sum += o.bonusPerYear;
                }
                else
                {
                    long applied = (long)(o.bonusPerYear * Mathf.Clamp01(progress01));
                    if (applied < 0) applied = 0;
                    sum += applied;
                }
            }

            return sum;
        }

        public float EvaluateOrderProgress01(SponsorOrder order, TeamPerformanceSnapshot snapshot)
        {
            int target = Mathf.Max(1, order.target);

            switch (order.type)
            {
                case SponsorOrderType.ChampionshipPosition:
                {
                    int pos = Mathf.Max(1, snapshot.championshipPosition);
                    return (pos <= target) ? 1f : 0f;
                }

                case SponsorOrderType.Points:
                {
                    int v = Mathf.Max(0, snapshot.points);
                    return Mathf.Clamp01((float)v / target);
                }

                case SponsorOrderType.Podiums:
                {
                    int v = Mathf.Max(0, snapshot.podiums);
                    return Mathf.Clamp01((float)v / target);
                }

                case SponsorOrderType.Wins:
                {
                    int v = Mathf.Max(0, snapshot.wins);
                    return Mathf.Clamp01((float)v / target);
                }
            }

            return 0f;
        }

        // -------------------------
        // TOTAL
        // -------------------------
        public long GetTotalSponsorPerYear(TeamPerformanceSnapshot snapshot, bool includeOrders = true)
        {
            long guaranteed = GetGuaranteedPerYearTotal();
            long perf = GetPerformancePerYearTotal(snapshot.championshipPosition);

            if (!includeOrders)
                return guaranteed + perf;

            long ordersBonus = GetOrdersBonusPerYear(snapshot, includeEndOfSeasonOnly: true, includeProgressive: true);
            return guaranteed + perf + ordersBonus;
        }

        private void OnValidate()
        {
            if (year < 1950) year = 1950;

            if (minNewBrandsPerSeason < 0) minNewBrandsPerSeason = 0;
            if (maxNewBrandsPerSeason < minNewBrandsPerSeason) maxNewBrandsPerSeason = minNewBrandsPerSeason;

            if (minYears < 1) minYears = 1;
            if (maxYears < minYears) maxYears = minYears;

            if (basePayMultiplier < 0f) basePayMultiplier = 0f;
            if (bonusMultiplier < 0f) bonusMultiplier = 0f;

            if (orders != null)
            {
                for (int i = 0; i < orders.Length; i++)
                {
                    var o = orders[i];
                    if (o == null) continue;
                    if (o.target < 1) o.target = 1;
                    if (o.bonusPerYear < 0) o.bonusPerYear = 0;
                }
            }

            if (tiers != null)
            {
                for (int i = 0; i < tiers.Length; i++)
                {
                    var t = tiers[i];
                    if (t == null) continue;
                    if (t.guaranteedPerYear < 0) t.guaranteedPerYear = 0;
                    if (t.performanceBonusPerYear < 0) t.performanceBonusPerYear = 0;
                    if (t.topPositionThreshold < 1) t.topPositionThreshold = 1;
                }
            }

            if (countries != null)
            {
                for (int i = countries.Count - 1; i >= 0; i--)
                {
                    if (string.IsNullOrWhiteSpace(countries[i])) countries.RemoveAt(i);
                }
            }

            if (namePrefixes != null)
            {
                for (int i = namePrefixes.Count - 1; i >= 0; i--)
                {
                    if (string.IsNullOrWhiteSpace(namePrefixes[i])) namePrefixes.RemoveAt(i);
                }
            }

            if (nameSuffixes != null)
            {
                for (int i = nameSuffixes.Count - 1; i >= 0; i--)
                {
                    if (string.IsNullOrWhiteSpace(nameSuffixes[i])) nameSuffixes.RemoveAt(i);
                }
            }

            if (industryWeights != null)
            {
                for (int i = 0; i < industryWeights.Count; i++)
                {
                    if (industryWeights[i].weight < 0f)
                    {
                        var iw = industryWeights[i];
                        iw.weight = 0f;
                        industryWeights[i] = iw;
                    }
                }
            }
        }
    }
}
