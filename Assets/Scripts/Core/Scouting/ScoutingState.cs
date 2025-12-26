using System;
using System.Collections.Generic;
using UnityEngine;

namespace F1Manager.Core.Scouting
{
    [Serializable]
    public class ScoutingState
    {
        // por equipe: progresso de scouting por driver
        public List<ScoutingTeamProgress> teams = new List<ScoutingTeamProgress>();
    }

    [Serializable]
    public class ScoutingTeamProgress
    {
        public string teamId;
        public List<ScoutingProgressEntry> entries = new List<ScoutingProgressEntry>();
    }

    [Serializable]
    public class ScoutingProgressEntry
    {
        public string driverId;

        [Range(0f, 1f)] public float reveal01; // 0..1

        // quando revela, melhora estimativa do mercado
        public bool revealedPotential;
        public bool revealedAdaptability;
        public bool revealedWorkEthic;
    }
}
