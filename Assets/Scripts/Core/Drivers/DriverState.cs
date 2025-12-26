using System;
using UnityEngine;
using F1Manager.Core.Contracts;

namespace F1Manager.Core.Drivers
{
    [Serializable]
    public class DriverState
    {
        public string driverId;
        public string firstName;
        public string lastName;
        public string nationality;

        [Min(14)] public int age;
        public DriverTier tier;

        public DriverStats stats;

        // vínculo (F1/F2 etc pode estar livre/contratado)
        public DriverContractState contract;

        // metadados pro mundo (história)
        public int careerWins;
        public int careerPodiums;
        public int careerPoles;
        public int careerTitles;

        public string FullName => $"{firstName} {lastName}".Trim();

        public bool IsFreeAgent => contract.status == ContractStatus.FreeAgent;

        public void Clamp()
        {
            age = Mathf.Clamp(age, 14, 60);
            stats.ClampAll();
        }
    }
}
