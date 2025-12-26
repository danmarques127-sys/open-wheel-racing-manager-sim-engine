using System;
using UnityEngine;

namespace F1Manager.Core.Contracts
{
    public enum ContractStatus
    {
        FreeAgent = 0,
        Signed = 1
    }

    [Serializable]
    public struct DriverContractState
    {
        public ContractStatus status;

        // equipe atual
        public string teamId;

        // salário e duração
        [Min(0f)] public float salaryPerWeek;
        [Min(0)] public int weeksRemaining;

        // cláusulas simples
        public ContractClause clause;

        public bool IsActive =>
            status == ContractStatus.Signed &&
            !string.IsNullOrEmpty(teamId) &&
            weeksRemaining > 0;

        public static DriverContractState FreeAgent()
        {
            return new DriverContractState
            {
                status = ContractStatus.FreeAgent,
                teamId = "",
                salaryPerWeek = 0f,
                weeksRemaining = 0,
                clause = new ContractClause { type = ContractClauseType.None }
            };
        }
    }
}
