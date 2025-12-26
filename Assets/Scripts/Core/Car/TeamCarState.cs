using System;
using System.Collections.Generic;
using UnityEngine;
using F1Manager.Data;

namespace F1Manager.Core.Car
{
    [Serializable]
    public class CarPartLevelState
    {
        public CarPartType partType;

        [Header("Levels (manager scale)")]
        public float performanceLevel = 0f;   // valor contínuo, ex: 50.0
        public float reliabilityLevel = 0f;   // valor contínuo, ex: 50.0

        [Header("Wear / Durability")]
        [Range(0f, 100f)] public float wearPercent = 0f;

        [Header("Known Issues")]
        public List<string> knownIssues = new List<string>();

        [Header("Spec Version")]
        public int specYear = 2026;           // para mudanças de regulamento por ano
    }

    [Serializable]
    public class TeamCarState
    {
        public string teamId; // "mercedes"
        public List<CarPartLevelState> parts = new List<CarPartLevelState>();

        public CarPartLevelState GetPart(CarPartType type)
        {
            for (int i = 0; i < parts.Count; i++)
                if (parts[i].partType == type) return parts[i];
            return null;
        }

        public void EnsureAllPartsExist(int specYear)
        {
            foreach (CarPartType t in (CarPartType[])Enum.GetValues(typeof(CarPartType)))
            {
                if (GetPart(t) == null)
                {
                    parts.Add(new CarPartLevelState
                    {
                        partType = t,
                        performanceLevel = 50f,
                        reliabilityLevel = 50f,
                        wearPercent = 0f,
                        specYear = specYear
                    });
                }
            }
        }
    }
}
