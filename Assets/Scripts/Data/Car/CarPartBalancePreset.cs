using System.Collections.Generic;
using UnityEngine;

namespace F1Manager.Data
{
    [CreateAssetMenu(fileName = "CarPartBalancePreset_", menuName = "F1 Manager/Car/Car Part Balance Preset", order = 11)]
    public class CarPartBalancePreset : ScriptableObject
    {
        public List<CarPartDefinition> definitions = new List<CarPartDefinition>();

        public CarPartDefinition Get(CarPartType type)
        {
            for (int i = 0; i < definitions.Count; i++)
            {
                if (definitions[i] != null && definitions[i].partType == type)
                    return definitions[i];
            }
            return null;
        }
    }
}
