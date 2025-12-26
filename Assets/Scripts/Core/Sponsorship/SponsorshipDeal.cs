using System;
using System.Collections.Generic;
using UnityEngine;

namespace F1Manager.Core.Sponsorship
{
    [Serializable]
    public class SponsorshipDeal
    {
        public string sponsorId;
        public string teamId;

        public float valuePerRace = 0f;
        public float valuePerYear = 0f;

        [Header("Bonuses")]
        public float bonusPerPodium = 0f;
        public float bonusPerWin = 0f;
        public float bonusPerTop10 = 0f;

        [Header("Penalties")]
        public float penaltyPerDNF = 0f;
        public float scandalPenalty = 0f;

        [Header("Term")]
        public int startSeasonYear;
        public int endSeasonYear;

        [Header("Objectives / Notes")]
        public List<string> objectives = new List<string>();

        public bool IsActiveForSeason(int year) => year >= startSeasonYear && year <= endSeasonYear;
    }
}
