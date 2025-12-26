using System;
using System.Collections.Generic;
using UnityEngine;

namespace F1Manager.Core.Academy
{
    [Serializable]
    public class AcademyState
    {
        // por equipe: lista de driverIds “na academia”
        public List<AcademyTeamSlot> teamAcademies = new List<AcademyTeamSlot>();
    }

    [Serializable]
    public class AcademyTeamSlot
    {
        public string teamId;
        public List<string> driverIds = new List<string>();
    }
}
