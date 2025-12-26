using System;
using System.Collections.Generic;
using System.Linq;
using F1Manager.Core.Randomness;
using F1Manager.Core.World;

namespace F1Manager.Core.Generation
{
    public sealed class TeamEntryGenerator
    {
        private readonly ExpansionRuleset _rules;

        public TeamEntryGenerator(ExpansionRuleset rules)
        {
            _rules = rules;
        }

        public TeamState TryGenerateTeam(
            RandomStream rng,
            int season,
            IReadOnlyCollection<string> existingTeamIds,
            int currentTeamCount,
            float gridHealth01)
        {
            if (currentTeamCount >= _rules.maxTeams) return null;
            if (gridHealth01 < _rules.minGridHealth) return null;
            if (!rng.Chance(_rules.entryChancePerSeason)) return null;

            string teamId = GenerateUniqueTeamId(rng, existingTeamIds);
            string country = WeightedPick(rng, _rules.countryWeights, fallback: "Unknown");
            string name = GenerateTeamName(rng);

            float budget = Clamp01(rng.NextGaussian((_rules.minBudget + _rules.maxBudget) * 0.5f, 0.10f));
            budget = Math.Clamp(budget, _rules.minBudget, _rules.maxBudget);

            float prestige = Clamp01(rng.NextGaussian(0.45f, 0.12f));
            float facility = Clamp01(rng.NextGaussian(0.40f, 0.15f));
            float carBase = Clamp01(rng.NextGaussian(0.42f, 0.10f));

            return new TeamState
            {
                teamId = teamId,
                teamName = name,
                country = country,
                prestige = prestige,
                budget = budget,
                facility = facility,
                carBase = carBase,
                isGenerated = true,
                entrySeason = season
            };
        }

        private string GenerateTeamName(RandomStream rng)
        {
            string p = (_rules.teamNamePrefixes != null && _rules.teamNamePrefixes.Count > 0) ? rng.Pick(_rules.teamNamePrefixes) : "Nova";
            string s = (_rules.teamNameSuffixes != null && _rules.teamNameSuffixes.Count > 0) ? rng.Pick(_rules.teamNameSuffixes) : "Racing";
            return $"{p} {s}";
        }

        private static string GenerateUniqueTeamId(RandomStream rng, IReadOnlyCollection<string> existing)
        {
            for (int tries = 0; tries < 2000; tries++)
            {
                string id = $"GEN_TEAM_{rng.RangeInt(0, int.MaxValue):X8}";
                if (!existing.Contains(id)) return id;
            }
            return $"GEN_TEAM_{Guid.NewGuid():N}";
        }

        private static string WeightedPick(RandomStream rng, List<GenerationRuleset.NationalityWeight> weights, string fallback)
        {
            if (weights == null || weights.Count == 0) return fallback;

            float total = 0f;
            for (int i = 0; i < weights.Count; i++) total += Math.Max(0f, weights[i].weight);

            float roll = rng.RangeFloat(0f, total);
            float acc = 0f;

            for (int i = 0; i < weights.Count; i++)
            {
                acc += Math.Max(0f, weights[i].weight);
                if (roll <= acc) return weights[i].nationality;
            }
            return weights[^1].nationality;
        }

        private static float Clamp01(float x) => x < 0f ? 0f : (x > 1f ? 1f : x);
    }
}
