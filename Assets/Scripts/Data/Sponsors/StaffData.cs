using UnityEngine;

namespace F1Manager.Data
{
    public enum StaffRole
    {
        TeamPrincipal = 0,
        TechnicalDirector = 1,
        HeadOfAero = 2,
        HeadOfPU = 3,
        StrategyLead = 4
    }

    [CreateAssetMenu(fileName = "Staff_", menuName = "F1 Manager/Staff/Staff Data", order = 60)]
    public class StaffData : ScriptableObject
    {
        public string staffId;
        public string fullName;
        public StaffRole role;

        [Header("Skills (0-100)")]
        [Range(0, 100)] public int leadership = 50;
        [Range(0, 100)] public int technical = 50;
        [Range(0, 100)] public int aero = 50;
        [Range(0, 100)] public int powerUnit = 50;
        [Range(0, 100)] public int strategy = 50;

        [Header("Traits")]
        [Range(-20, 20)] public int costEfficiency = 0; // impacts costs
        [Range(-20, 20)] public int riskControl = 0;    // impacts risk
        [Range(-20, 20)] public int speedBoost = 0;     // impacts duration
    }
}
