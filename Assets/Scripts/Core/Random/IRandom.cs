namespace F1Manager.Core.Randomness
{
    public interface IRandom
    {
        int RangeInt(int minInclusive, int maxExclusive);
        float RangeFloat(float minInclusive, float maxInclusive);
        bool Chance(float probability01);
    }
}
