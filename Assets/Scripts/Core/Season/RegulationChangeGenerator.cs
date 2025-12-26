using System;
using F1Manager.Core.Randomness;

namespace F1Manager.Core.Season
{
    public sealed class RegulationChangeGenerator
    {
        private readonly RegulationRuleset _rules;

        public RegulationChangeGenerator(RegulationRuleset rules)
        {
            _rules = rules;
        }

        public RegulationsState GenerateNext(RandomStream rng, RegulationsState prev, int season)
        {
            var next = new RegulationsState
            {
                season = season,
                aeroWeight = prev.aeroWeight,
                powerWeight = prev.powerWeight,
                reliabilityWeight = prev.reliabilityWeight,
                costCap = prev.costCap,
                sprintWeekendEnabled = prev.sprintWeekendEnabled,
                sprintWeekendsPerSeason = prev.sprintWeekendsPerSeason,
                changeVolatility = prev.changeVolatility
            };

            bool major = rng.Chance(_rules.majorChangeChance * (0.60f + 0.80f * prev.changeVolatility));
            bool minor = !major && rng.Chance(_rules.minorChangeChance);

            if (major)
            {
                ApplyWeightsShift(rng, ref next, _rules.majorDelta);
                next.changeVolatility = Math.Clamp(prev.changeVolatility + 0.15f, 0f, 1f);
            }
            else if (minor)
            {
                ApplyWeightsShift(rng, ref next, _rules.minorDelta);
                next.changeVolatility = Math.Clamp(prev.changeVolatility + 0.05f, 0f, 1f);
            }
            else
            {
                next.changeVolatility = Math.Clamp(prev.changeVolatility - 0.07f, 0f, 1f);
            }

            float costDelta = rng.NextGaussian(0f, major ? 0.05f : 0.02f);
            next.costCap = Math.Clamp(next.costCap + costDelta, _rules.costCapMin, _rules.costCapMax);

            if (rng.Chance(_rules.sprintPolicyChangeChance))
            {
                next.sprintWeekendEnabled = rng.Chance(0.75f);
                next.sprintWeekendsPerSeason = next.sprintWeekendEnabled ? rng.RangeInt(_rules.sprintMin, _rules.sprintMax + 1) : 0;
            }

            NormalizeWeights(ref next);
            return next;
        }

        private static void ApplyWeightsShift(RandomStream rng, ref RegulationsState s, float magnitude)
        {
            float da = rng.NextGaussian(0f, magnitude);
            float dp = rng.NextGaussian(0f, magnitude);
            float dr = rng.NextGaussian(0f, magnitude);

            s.aeroWeight = Clamp01(s.aeroWeight + da);
            s.powerWeight = Clamp01(s.powerWeight + dp);
            s.reliabilityWeight = Clamp01(s.reliabilityWeight + dr);
        }

        private static void NormalizeWeights(ref RegulationsState s)
        {
            float sum = s.aeroWeight + s.powerWeight + s.reliabilityWeight;
            if (sum <= 0.0001f)
            {
                s.aeroWeight = 0.34f;
                s.powerWeight = 0.33f;
                s.reliabilityWeight = 0.33f;
                return;
            }

            s.aeroWeight /= sum;
            s.powerWeight /= sum;
            s.reliabilityWeight /= sum;
        }

        private static float Clamp01(float x) => x < 0f ? 0f : (x > 1f ? 1f : x);
    }
}
