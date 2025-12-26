using System;
using System.Security.Cryptography;
using System.Text;

namespace F1Manager.Core.Randomness
{
    /// <summary>
    /// RNG determinístico por "stream": mesma seed + mesmo streamKey => mesma sequência.
    /// </summary>
    public sealed class RandomService
    {
        private readonly int _baseSeed;

        public RandomService(int baseSeed)
        {
            _baseSeed = baseSeed;
        }

        public RandomStream GetStream(string streamKey)
        {
            int seed = HashToInt(_baseSeed, streamKey);
            return new RandomStream(seed);
        }

        private static int HashToInt(int baseSeed, string key)
        {
            using var sha = SHA256.Create();
            string input = $"{baseSeed}|{key}";
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            int value = BitConverter.ToInt32(bytes, 0);

            // garantir positivo (System.Random aceita int >= 0)
            if (value == int.MinValue) value = int.MaxValue;
            value = Math.Abs(value);
            return value;
        }
    }

    public sealed class RandomStream
    {
        private readonly System.Random _rng;

        public int Seed { get; }

        public RandomStream(int seed)
        {
            Seed = seed;
            _rng = new System.Random(seed);
        }

        public int RangeInt(int minInclusive, int maxExclusive) => _rng.Next(minInclusive, maxExclusive);

        public float RangeFloat(float minInclusive, float maxInclusive)
            => (float)(_rng.NextDouble() * (maxInclusive - minInclusive) + minInclusive);

        public bool Chance(float probability01)
            => RangeFloat(0f, 1f) < probability01;

        public T Pick<T>(System.Collections.Generic.IReadOnlyList<T> list)
        {
            if (list == null || list.Count == 0) throw new ArgumentException("List vazia");
            return list[RangeInt(0, list.Count)];
        }

        public float NextGaussian(float mean = 0f, float stdDev = 1f)
        {
            // Box–Muller
            double u1 = 1.0 - _rng.NextDouble();
            double u2 = 1.0 - _rng.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return (float)(mean + stdDev * randStdNormal);
        }

        public float Clamp01(float x) => x < 0f ? 0f : (x > 1f ? 1f : x);
    }
}
