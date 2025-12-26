using System;
using System.Collections.Generic;
using System.Linq;
using F1Manager.Core.Randomness;
using F1Manager.Core.World;

namespace F1Manager.Core.Generation
{
    public sealed class DriverRegenGenerator
    {
        private readonly GenerationRuleset _rules;

        public DriverRegenGenerator(GenerationRuleset rules)
        {
            _rules = rules;
        }

        public List<DriverState> GenerateRookies(RandomStream rng, int season, IReadOnlyCollection<string> existingDriverIds)
        {
            int count = rng.RangeInt(_rules.minRookies, _rules.maxRookies + 1);
            var rookies = new List<DriverState>(count);

            for (int i = 0; i < count; i++)
            {
                var rookie = GenerateOne(rng, season, existingDriverIds, rookies);
                rookies.Add(rookie);
            }

            EnforceCohortConstraints(rng, rookies);
            return rookies;
        }

        private DriverState GenerateOne(RandomStream rng, int season, IReadOnlyCollection<string> existingIds, List<DriverState> cohort)
        {
            string driverId = GenerateUniqueId(rng, existingIds, cohort);
            string nationality = WeightedPickNationality(rng);
            string fullName = GenerateName(rng);

            int age = rng.RangeInt(_rules.minAge, _rules.maxAge + 1);

            float baseSkill = Clamp01(rng.NextGaussian(_rules.meanBaseSkill, _rules.stdBaseSkill));
            float potential = Clamp01(rng.NextGaussian(_rules.meanPotential, _rules.stdPotential));

            if (rng.Chance(_rules.superstarChance))
                potential = Math.Min(_rules.maxPotential, Math.Max(potential, 0.90f));

            if (rng.Chance(_rules.dudChance))
                baseSkill = Math.Min(baseSkill, 0.45f);

            baseSkill = Math.Min(baseSkill, _rules.maxInitialSkill);
            potential = Math.Min(potential, _rules.maxPotential);
            potential = Math.Max(potential, baseSkill + 0.05f);

            int peakAge = rng.RangeInt(26, 33);

            float pace = JitterAround(rng, baseSkill, 0.08f);
            float racecraft = JitterAround(rng, baseSkill, 0.10f);
            float consistency = JitterAround(rng, baseSkill, 0.12f);
            float wetSkill = JitterAround(rng, baseSkill, 0.14f);
            float tyreMgmt = JitterAround(rng, baseSkill, 0.10f);
            float starts = JitterAround(rng, baseSkill, 0.12f);

            float aggression = Clamp01(rng.NextGaussian(0.55f, 0.20f));
            float pressure = Clamp01(rng.NextGaussian(0.55f, 0.18f));
            float temperament = Clamp01(rng.NextGaussian(0.45f, 0.22f));
            float professionalism = Clamp01(rng.NextGaussian(0.60f, 0.16f));

            ApplyArchetype(rng,
                ref pace, ref racecraft, ref consistency, ref wetSkill, ref tyreMgmt, ref starts,
                ref aggression, ref pressure, ref temperament, ref professionalism);

            return new DriverState
            {
                driverId = driverId,
                fullName = fullName,
                nationality = nationality,
                age = age,
                peakAge = peakAge,
                potential = potential,
                currentSkill = baseSkill,

                pace = pace,
                racecraft = racecraft,
                consistency = consistency,
                wetSkill = wetSkill,
                tyreMgmt = tyreMgmt,
                starts = starts,

                aggression = aggression,
                pressure = pressure,
                temperament = temperament,
                professionalism = professionalism,

                isGenerated = true,
                debutSeason = season
            };
        }

        private void EnforceCohortConstraints(RandomStream rng, List<DriverState> rookies)
        {
            // No máximo 1 “absurdo” por safra
            var elites = rookies.Where(d => d.potential >= 0.94f).OrderByDescending(d => d.potential).ToList();
            for (int i = 1; i < elites.Count; i++)
                elites[i].potential = 0.92f + rng.RangeFloat(0f, 0.015f);

            // Evitar muitos rookies já muito fortes
            var highInitial = rookies.Where(d => d.currentSkill >= 0.75f).ToList();
            for (int i = 2; i < highInitial.Count; i++)
                highInitial[i].currentSkill = 0.72f + rng.RangeFloat(0f, 0.02f);
        }

        private static float JitterAround(RandomStream rng, float center, float std)
            => Clamp01(rng.NextGaussian(center, std));

        private static void ApplyArchetype(
            RandomStream rng,
            ref float pace, ref float racecraft, ref float consistency, ref float wetSkill, ref float tyreMgmt, ref float starts,
            ref float aggression, ref float pressure, ref float temperament, ref float professionalism)
        {
            int archetype = rng.RangeInt(0, 6);
            switch (archetype)
            {
                case 1: // Hotlapper
                    pace = Clamp01(pace + 0.08f);
                    consistency = Clamp01(consistency - 0.05f);
                    break;

                case 2: // Rain master
                    wetSkill = Clamp01(wetSkill + 0.12f);
                    break;

                case 3: // Tyre whisperer
                    tyreMgmt = Clamp01(tyreMgmt + 0.10f);
                    pace = Clamp01(pace - 0.03f);
                    break;

                case 4: // Aggressive racer
                    aggression = Clamp01(aggression + 0.15f);
                    temperament = Clamp01(temperament + 0.08f);
                    racecraft = Clamp01(racecraft + 0.06f);
                    consistency = Clamp01(consistency - 0.06f);
                    break;

                case 5: // Ice-cold pro
                    pressure = Clamp01(pressure + 0.12f);
                    professionalism = Clamp01(professionalism + 0.10f);
                    temperament = Clamp01(temperament - 0.10f);
                    break;
            }
        }

        private string WeightedPickNationality(RandomStream rng)
        {
            if (_rules.nationalityWeights == null || _rules.nationalityWeights.Count == 0)
                return "Unknown";

            float total = 0f;
            for (int i = 0; i < _rules.nationalityWeights.Count; i++)
                total += Math.Max(0f, _rules.nationalityWeights[i].weight);

            float roll = rng.RangeFloat(0f, total);
            float acc = 0f;

            for (int i = 0; i < _rules.nationalityWeights.Count; i++)
            {
                acc += Math.Max(0f, _rules.nationalityWeights[i].weight);
                if (roll <= acc) return _rules.nationalityWeights[i].nationality;
            }

            return _rules.nationalityWeights[^1].nationality;
        }

        private string GenerateName(RandomStream rng)
        {
            string first = (_rules.firstNames != null && _rules.firstNames.Count > 0) ? rng.Pick(_rules.firstNames) : "Driver";
            string last = (_rules.lastNames != null && _rules.lastNames.Count > 0) ? rng.Pick(_rules.lastNames) : $"#{rng.RangeInt(100, 999)}";
            return $"{first} {last}";
        }

        private static string GenerateUniqueId(RandomStream rng, IReadOnlyCollection<string> existing, List<DriverState> cohort)
        {
            for (int tries = 0; tries < 2000; tries++)
            {
                string id = $"GEN_{rng.RangeInt(0, int.MaxValue):X8}_{rng.RangeInt(0, 1000):D3}";
                if (!existing.Contains(id) && cohort.All(d => d.driverId != id))
                    return id;
            }
            return $"GEN_{Guid.NewGuid():N}";
        }

        private static float Clamp01(float x) => x < 0f ? 0f : (x > 1f ? 1f : x);
    }
}
