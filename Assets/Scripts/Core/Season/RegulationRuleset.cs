using UnityEngine;

namespace F1Manager.Core.Season
{
    [CreateAssetMenu(fileName = "RegulationRuleset_", menuName = "F1 Manager/Rules/Regulations Ruleset", order = 43)]
    public class RegulationRuleset : ScriptableObject
    {
        [Header("Era changes")]
        [Range(0f, 1f)] public float majorChangeChance = 0.08f;
        [Range(0f, 1f)] public float minorChangeChance = 0.55f;

        [Header("Change magnitudes")]
        [Range(0f, 0.5f)] public float minorDelta = 0.06f;
        [Range(0f, 0.8f)] public float majorDelta = 0.22f;

        [Header("Cost cap")]
        [Range(0f, 1f)] public float costCapMin = 0.35f;
        [Range(0f, 1f)] public float costCapMax = 0.80f;

        [Header("Sprint policy")]
        [Range(0f, 1f)] public float sprintPolicyChangeChance = 0.12f;
        [Min(0)] public int sprintMin = 0;
        [Min(0)] public int sprintMax = 8;
    }
}
