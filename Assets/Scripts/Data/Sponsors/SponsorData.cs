using System.Collections.Generic;
using UnityEngine;

namespace F1Manager.Data
{
    public enum SponsorCategory
    {
        Tech = 0,
        Oil = 1,
        Retail = 2,
        Finance = 3,
        Automotive = 4,
        Airlines = 5,
        Energy = 6,
        Luxury = 7,
        Other = 99
    }

    [CreateAssetMenu(fileName = "Sponsor_", menuName = "F1 Manager/Sponsors/Sponsor Data", order = 50)]
    public class SponsorData : ScriptableObject
    {
        public string sponsorId;
        public string sponsorName;
        public SponsorCategory category;

        [Header("Brand / Market")]
        [Tooltip("How valuable is the brand in marketing terms (0-100).")]
        [Range(0, 100)] public int brandValue = 50;

        [Header("Performance Expectations")]
        [Tooltip("0=accepts backmarkers, 100=only wants top teams.")]
        [Range(0, 100)] public int performanceDemand = 60;

        [Header("Preferences")]
        public List<string> preferredCountries = new List<string>(); // ISO or human-readable (ex: "UK", "USA", "BR")
    }
}

