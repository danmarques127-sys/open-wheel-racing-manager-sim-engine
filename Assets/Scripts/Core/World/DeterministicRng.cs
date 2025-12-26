using System;

namespace F1Manager.Core.World
{
    // RNG determinístico com streams separadas (calendário, rookies, eventos, etc.)
    public sealed class DeterministicRng
    {
        private readonly Random _random;

        public DeterministicRng(int seed)
        {
            _random = new Random(seed);
        }

        public int NextInt(int minInclusive, int maxExclusive) => _random.Next(minInclusive, maxExclusive);
        public float Next01() => (float)_random.NextDouble();

        // Fisher-Yates shuffle
        public void Shuffle<T>(System.Collections.Generic.IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _random.Next(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }

    public static class RngStreams
    {
        // Deriva seeds consistentes por stream
        public static int Derive(int baseSeed, string streamKey)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + baseSeed;
                hash = hash * 31 + (streamKey?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}
