using System;

namespace F1Manager.Core.Finance
{
    public enum SponsorIndustry
    {
        Tech,
        Finance,
        Energy,
        Automotive,
        Retail,
        Telecom,
        Gaming,
        Logistics,
        ConsumerGoods
    }

    [Serializable]
    public class SponsorBrand
    {
        public string sponsorId;
        public string name;
        public SponsorIndustry industry;
        public string country;

        public float brandPrestige; // 0..1
        public float risk;          // 0..1 (mais risco = mais chance de sair cedo)
        public bool isGenerated;
    }

    [Serializable]
    public class SponsorDeal
    {
        public string dealId;
        public string sponsorId;
        public string teamId;

        public int startSeason;
        public int endSeason;

        public float basePayment;        // escala interna
        public float bonusPerPoint;      // escala interna
        public int targetConstructorsPos;

        public float breakChancePerSeason;
    }
}
