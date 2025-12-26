using System.Collections.Generic;
using UnityEngine;

namespace F1Manager.Core.Randomness
{
    [CreateAssetMenu(fileName = "ExpansionRuleset_", menuName = "F1 Manager/Rules/Expansion Ruleset", order = 41)]
    public class ExpansionRuleset : ScriptableObject
    {
        [Min(10)] public int maxTeams = 11;
        [Range(0f, 1f)] public float entryChancePerSeason = 0.12f;

        [Header("Constraints")]
        [Range(0f, 1f)] public float minGridHealth = 0.45f;
        [Range(0f, 1f)] public float minBudget = 0.35f;
        [Range(0f, 1f)] public float maxBudget = 0.75f;

        [Header("Name pools")]
        public List<string> teamNamePrefixes = new();
        public List<string> teamNameSuffixes = new();

        [Header("Country weights")]
        public List<GenerationRuleset.NationalityWeight> countryWeights = new();
    }
}
