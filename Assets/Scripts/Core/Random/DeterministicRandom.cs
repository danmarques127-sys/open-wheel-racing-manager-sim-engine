using System;

namespace F1Manager.Core.Randomness
{
    public sealed class DeterministicRandom : IRandom
    {
        private readonly System.Random rng;

        public DeterministicRandom(int seed)
        {
            rng = new System.Random(seed);
        }

        public int RangeInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive) return minInclusive;
            return rng.Next(minInclusive, maxExclusive);
        }

        public float RangeFloat(float minInclusive, float maxInclusive)
        {
            if (maxInclusive <= minInclusive) return minInclusive;
            double t = rng.NextDouble(); // 0..1
            return (float)(minInclusive + (maxInclusive - minInclusive) * t);
        }

        public bool Chance(float probability01)
        {
            if (probability01 <= 0f) return false;
            if (probability01 >= 1f) return true;
            return RangeFloat(0f, 1f) < probability01;
        }
    }
}
