using System;
using System.Collections.Generic;
using F1Manager.Core.Randomness;
using DataSponsorRuleset = F1Manager.Data.SponsorRuleset;

namespace F1Manager.Core.Finance
{
    public sealed class SponsorGenerator
    {
        private readonly DataSponsorRuleset _rules;

        public SponsorGenerator(DataSponsorRuleset rules)
        {
            _rules = rules;
        }

        public List<SponsorBrand> GenerateNewBrands(RandomStream rng, IReadOnlyCollection<string> existingSponsorIds, int season)
        {
            int count = rng.RangeInt(_rules.minNewBrandsPerSeason, _rules.maxNewBrandsPerSeason + 1);
            var list = new List<SponsorBrand>(count);

            for (int i = 0; i < count; i++)
            {
                string sponsorId = UniqueId(rng, existingSponsorIds, list);
                string name = GenerateBrandName(rng);
                SponsorIndustry industry = WeightedPickIndustry(rng);
                string country = (_rules.countries != null && _rules.countries.Count > 0) ? rng.Pick(_rules.countries) : "Unknown";

                float brandPrestige = Clamp01(rng.NextGaussian(0.50f, 0.18f));
                float risk = Clamp01(rng.NextGaussian(0.35f, 0.20f));

                list.Add(new SponsorBrand
                {
                    sponsorId = sponsorId,
                    name = name,
                    industry = industry,
                    country = country,
                    brandPrestige = brandPrestige,
                    risk = risk,
                    isGenerated = true
                });
            }

            return list;
        }

        public SponsorDeal ProposeDeal(RandomStream rng, SponsorBrand brand, string teamId, int season, float teamPrestige01, int lastConstructorsPos, int teamsCount)
        {
            int years = rng.RangeInt(_rules.minYears, _rules.maxYears + 1);
            int endSeason = season + years - 1;

            int targetPos = ComputeTargetPosition(rng, teamPrestige01, lastConstructorsPos, teamsCount);

            float difficulty01 = 1f - ((float)targetPos - 1f) / Math.Max(1f, teamsCount - 1f);

            float basePay = (0.20f + 0.80f * teamPrestige01)
                          * (0.35f + 0.65f * brand.brandPrestige)
                          * (0.70f + 0.60f * difficulty01);
            basePay *= _rules.basePayMultiplier;

            float bonusPerPoint = (0.0025f + 0.010f * difficulty01)
                                * (0.50f + 0.50f * brand.brandPrestige);
            bonusPerPoint *= _rules.bonusMultiplier;

            float breakChance = 0.03f + 0.20f * brand.risk;

            return new SponsorDeal
            {
                dealId = $"DEAL_{Guid.NewGuid():N}",
                sponsorId = brand.sponsorId,
                teamId = teamId,
                startSeason = season,
                endSeason = endSeason,
                basePayment = basePay,
                bonusPerPoint = bonusPerPoint,
                targetConstructorsPos = targetPos,
                breakChancePerSeason = breakChance
            };
        }

        private SponsorIndustry WeightedPickIndustry(RandomStream rng)
        {
            if (_rules.industryWeights == null || _rules.industryWeights.Count == 0)
                return SponsorIndustry.ConsumerGoods;

            float total = 0f;
            for (int i = 0; i < _rules.industryWeights.Count; i++)
                total += Math.Max(0f, _rules.industryWeights[i].weight);

            if (total <= 0f)
                return _rules.industryWeights[0].industry;

            float roll = rng.RangeFloat(0f, total);
            float acc = 0f;

            for (int i = 0; i < _rules.industryWeights.Count; i++)
            {
                acc += Math.Max(0f, _rules.industryWeights[i].weight);
                if (roll <= acc) return _rules.industryWeights[i].industry;
            }

            return _rules.industryWeights[_rules.industryWeights.Count - 1].industry;
        }

        private string GenerateBrandName(RandomStream rng)
        {
            string p = (_rules.namePrefixes != null && _rules.namePrefixes.Count > 0) ? rng.Pick(_rules.namePrefixes) : "Nova";
            string s = (_rules.nameSuffixes != null && _rules.nameSuffixes.Count > 0) ? rng.Pick(_rules.nameSuffixes) : "Group";
            return $"{p}{s}";
        }

        private static int ComputeTargetPosition(RandomStream rng, float teamPrestige01, int lastPos, int teamsCount)
        {
            int expected = Math.Clamp((int)Math.Round((1f - teamPrestige01) * (teamsCount - 1)) + 1, 1, teamsCount);

            int improve = rng.RangeInt(0, 3);
            int target = Math.Clamp(Math.Min(lastPos, expected) - improve, 1, teamsCount);

            if (teamPrestige01 < 0.30f) target = Math.Max(target, 6);
            return target;
        }

        private static string UniqueId(RandomStream rng, IReadOnlyCollection<string> existing, List<SponsorBrand> list)
        {
            bool hasExisting = existing != null && existing.Count > 0;

            for (int tries = 0; tries < 2000; tries++)
            {
                string id = $"GEN_SP_{rng.RangeInt(0, int.MaxValue):X8}";

                if (hasExisting)
                {
                    bool used = false;
                    foreach (var e in existing)
                    {
                        if (e == id) { used = true; break; }
                    }
                    if (used) continue;
                }

                bool usedInBatch = false;
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].sponsorId == id) { usedInBatch = true; break; }
                }
                if (usedInBatch) continue;

                return id;
            }

            return $"GEN_SP_{Guid.NewGuid():N}";
        }

        private static float Clamp01(float x) => x < 0f ? 0f : (x > 1f ? 1f : x);
    }
}
