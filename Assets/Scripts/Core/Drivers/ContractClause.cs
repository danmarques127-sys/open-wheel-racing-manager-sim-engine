using System;
using UnityEngine;

namespace F1Manager.Core.Contracts
{
    public enum ContractClauseType
    {
        None = 0,
        Buyout = 1,
        OptionYear = 2,
        PerformanceExit = 3
    }

    [Serializable]
    public struct ContractClause
    {
        public ContractClauseType type;

        // Buyout: valor para romper
        [Min(0f)] public float buyoutCost;

        // OptionYear: +1 ano se exercida (por equipe ou piloto)
        public bool optionYearTeamControls;

        // PerformanceExit: se abaixo de X por Y semanas pode sair
        [Range(0f, 100f)] public float minPerformanceToKeepSeat;
        [Min(0)] public int weeksBelowThresholdToTrigger;
    }
}
