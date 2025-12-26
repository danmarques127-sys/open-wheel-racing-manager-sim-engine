using System;
using System.Collections.Generic;
using UnityEngine;

namespace F1Manager.Data
{
    [Serializable]
    public class FIASectionParts
    {
        public FIASection section;
        public List<CarPartType> partTypes = new List<CarPartType>();
    }

    [CreateAssetMenu(fileName = "CarSystemMap_", menuName = "F1 Manager/Car/Car System Map (FIA)", order = 5)]
    public class CarSystemMap : ScriptableObject
    {
        [Header("FIA Sections → Part Types")]
        public List<FIASectionParts> mapping = new List<FIASectionParts>();

        public IReadOnlyList<CarPartType> GetPartsForSection(FIASection section)
        {
            for (int i = 0; i < mapping.Count; i++)
            {
                if (mapping[i] != null && mapping[i].section == section)
                    return mapping[i].partTypes;
            }
            return Array.Empty<CarPartType>();
        }

        public bool IsPartInSection(CarPartType partType, FIASection section)
        {
            var parts = GetPartsForSection(section);
            for (int i = 0; i < parts.Count; i++)
                if (parts[i] == partType) return true;
            return false;
        }
    }
}
