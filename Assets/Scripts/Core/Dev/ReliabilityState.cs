using System;

namespace F1Manager.Core.Dev
{
    [Serializable]
    public class ReliabilityState
    {
        // 0..1 (1 = perfeito)
        public float engine = 1f;
        public float gearbox = 1f;
        public float aero = 1f;

        public void ApplyWear(float wear01)
        {
            wear01 = Math.Clamp(wear01, 0f, 1f);
            engine = Math.Clamp(engine - wear01, 0f, 1f);
            gearbox = Math.Clamp(gearbox - wear01 * 0.8f, 0f, 1f);
            aero = Math.Clamp(aero - wear01 * 0.6f, 0f, 1f);
        }
    }
}
