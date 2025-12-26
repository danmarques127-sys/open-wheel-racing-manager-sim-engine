using UnityEngine;

namespace F1Manager.Core.Market
{
    [CreateAssetMenu(
        fileName = "DriverMarketEntry",
        menuName = "F1 Manager/Market/Driver Market Entry",
        order = 1
    )]
    public class DriverMarketEntrySO : ScriptableObject
    {
        [Header("Identity")]
        public string driverId;

        [Header("Market Estimate")]
        [Range(0f, 100f)]
        public float marketRatingEstimate = 50f;

        [Header("Contract Expectations")]
        [Min(0f)]
        public float expectedSalaryPerWeek = 0f;

        [Min(1)]
        public int expectedWeeks = 52;

        [Header("Negotiation")]
        [Range(0f, 1f)]
        public float willingnessBase01 = 0.5f;

        [Header("Flags")]
        public bool isListed = true;
    }
}
