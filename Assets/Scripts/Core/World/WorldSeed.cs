using System;

namespace F1Manager.Core.World
{
    [Serializable]
    public struct WorldSeed
    {
        public int seed;

        public WorldSeed(int seed)
        {
            this.seed = seed;
        }

        public static WorldSeed FromInt(int seed) => new WorldSeed(seed);

        public override string ToString() => seed.ToString();
    }
}
