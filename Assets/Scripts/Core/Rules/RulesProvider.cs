using UnityEngine;
using F1Manager.Data;

namespace F1Manager.Core.Rules
{
    public class RulesProvider
    {
        public EconomyRuleset economy;
        public RnDRuleset rnd;
        public PrizeDistributionRuleset prize;

        public CarPartBalancePreset carPartBalance;

        public ResearchProject[] allResearchProjects;

        public ResearchProject FindProject(string projectId)
        {
            if (allResearchProjects == null) return null;
            for (int i = 0; i < allResearchProjects.Length; i++)
            {
                if (allResearchProjects[i] != null && allResearchProjects[i].projectId == projectId)
                    return allResearchProjects[i];
            }
            return null;
        }
    }
}
