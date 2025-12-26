using System;
using System.Collections.Generic;
using UnityEngine;

namespace F1Manager.Core.Randomness
{
    [CreateAssetMenu(fileName = "GenerationRuleset_", menuName = "F1 Manager/Rules/Generation Ruleset", order = 40)]
    public class GenerationRuleset : ScriptableObject
    {
        [Header("Rookies per season")]
        [Min(0)] public int minRookies = 3;
        [Min(0)] public int maxRookies = 8;

        [Header("Age")]
        [Min(15)] public int minAge = 18;
        [Min(15)] public int maxAge = 24;

        [Header("Skill distributions (Gaussian)")]
        [Range(0f, 1f)] public float meanBaseSkill = 0.55f;
        [Range(0f, 1f)] public float stdBaseSkill = 0.12f;

        [Range(0f, 1f)] public float meanPotential = 0.65f;
        [Range(0f, 1f)] public float stdPotential = 0.15f;

        [Header("Caps / constraints")]
        [Range(0f, 1f)] public float maxInitialSkill = 0.78f;   // rookie não nasce elite
        [Range(0f, 1f)] public float maxPotential = 0.98f;      // raríssimo
        [Range(0f, 1f)] public float superstarChance = 0.06f;   // chance de “fenômeno” na safra
        [Range(0f, 1f)] public float dudChance = 0.08f;         // chance de um “ruim” na safra

        [Header("Nationality weights")]
        public List<NationalityWeight> nationalityWeights = new();

        [Serializable]
        public class NationalityWeight
        {
            public string nationality = "British";
            [Min(0f)] public float weight = 1f;
        }

        [Header("Name pools (simple)")]
        public List<string> firstNames = new();
        public List<string> lastNames = new();
    }
}
