using System;
using UnityEngine;
using F1Manager.Core.Facilities;
using F1Manager.Core.Finance;
using F1Manager.Core.Manufacturing;
using F1Manager.Core.Save;
using F1Manager.Core.Rules;
using F1Manager.Data;

namespace F1Manager.Core.RnD
{
    public class RnDService
    {
        private readonly SaveData save;
        private readonly RulesProvider rules;
        private readonly FinanceService finance;
        private readonly ManufacturingService manufacturing;

        public RnDService(SaveData save, RulesProvider rules, FinanceService finance, ManufacturingService manufacturing)
        {
            this.save = save;
            this.rules = rules;
            this.finance = finance;
            this.manufacturing = manufacturing;
        }

        public bool StartResearch(string teamId, string projectId)
        {
            teamId = NormalizeId(teamId);
            projectId = NormalizeId(projectId);

            var p = rules.FindProject(projectId);
            if (p == null) return false;

            var fac = save.GetFacilities(teamId);
            if (fac == null) return false;

            FacilitiesService.RecalculateMultipliers(fac);

            float cost = p.baseCost * rules.rnd.researchCostMultiplier * fac.rndCostMultiplier;
            int weeks = Mathf.CeilToInt(p.durationWeeks * rules.rnd.researchDurationMultiplier * fac.rndSpeedMultiplier);

            if (!finance.TrySpend(teamId, cost, CostType.Upgrades))
                return false;

            // 1 research ativo por equipe
            for (int i = 0; i < save.activeResearch.Count; i++)
            {
                var ar = save.activeResearch[i];

                // ✅ precisa de ActiveResearchState.teamId como string
                if (ar.IsActive && NormalizeId(ar.teamId) == teamId)
                    return false; // already researching
            }

            save.activeResearch.Add(new ActiveResearchState
            {
                teamId = teamId,
                projectId = projectId,
                weeksRemaining = Math.Max(1, weeks),
                budgetCommitted = cost,
                accumulatedRisk = p.risk * rules.rnd.researchRiskMultiplier * fac.rndRiskMultiplier
            });

            return true;
        }

        public void ProcessWeek()
        {
            for (int i = save.activeResearch.Count - 1; i >= 0; i--)
            {
                var a = save.activeResearch[i];

                if (!a.IsActive)
                {
                    save.activeResearch.RemoveAt(i);
                    continue;
                }

                a.weeksRemaining -= 1;

                if (a.weeksRemaining <= 0)
                {
                    CompleteResearch(a);
                    save.activeResearch.RemoveAt(i);
                }
                else
                {
                    save.activeResearch[i] = a; // writeback (struct)
                }
            }
        }

        private void CompleteResearch(ActiveResearchState a)
        {
            var p = rules.FindProject(a.projectId);
            if (p == null) return;

            // risco -> issue
            float issueChance = rules.rnd.issueChanceBase + (a.accumulatedRisk * rules.rnd.issueChancePerRisk);
            bool gotIssue = UnityEngine.Random.value < Clamp01(issueChance);

            float relDelta = gotIssue ? -0.5f : 0f;
            string issue = gotIssue ? $"Issue from {p.projectId}: reliability penalty" : null;

            // manufatura pós-research
            var fac = save.GetFacilities(a.teamId);
            if (fac == null) return;

            FacilitiesService.RecalculateMultipliers(fac);

            int mWeeks = Mathf.CeilToInt(2f * rules.rnd.manufacturingDurationMultiplier * fac.manufacturingSpeedMultiplier);
            float mCost = 500000f * rules.rnd.manufacturingCostMultiplier * fac.manufacturingCostMultiplier;

            if (!finance.TrySpend(a.teamId, mCost, CostType.Manufacturing))
                return;

            manufacturing.Enqueue(new ManufacturingQueueItem
            {
                teamId = a.teamId,
                partType = p.partType,
                performanceLevelDelta = p.targetPerformanceGain,
                reliabilityDelta = relDelta,
                weeksRemaining = Math.Max(1, mWeeks),
                cost = mCost,
                sourceProjectId = p.projectId,
                quantity = 1
            }, issue);
        }

        private static float Clamp01(float v)
        {
            if (v < 0f) return 0f;
            if (v > 1f) return 1f;
            return v;
        }

        private static string NormalizeId(string v)
        {
            if (string.IsNullOrWhiteSpace(v)) return string.Empty;
            return v.Trim().ToLowerInvariant();
        }
    }
}
