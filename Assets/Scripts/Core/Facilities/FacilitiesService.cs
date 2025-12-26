using UnityEngine;

namespace F1Manager.Core.Facilities
{
    public static class FacilitiesService
    {
        /// <summary>
        /// Recalcula os multiplicadores de eficiência com base nos níveis atuais.
        /// Esses números são "game design": simples, previsíveis e fáceis de balancear.
        /// </summary>
        public static void RecalculateMultipliers(FacilityState s)
        {
            if (s == null) return;

            // Garantir mínimos
            s.hqLevel = Mathf.Max(1, s.hqLevel);
            s.aeroDeptLevel = Mathf.Max(1, s.aeroDeptLevel);
            s.puDeptLevel = Mathf.Max(1, s.puDeptLevel);
            s.strategyTeamLevel = Mathf.Max(1, s.strategyTeamLevel);

            // =========================
            // R&D SPEED: quanto maior, mais rápido (weeks reduz)
            // =========================
            // HQ ajuda todo mundo um pouco + dept específico ajuda mais
            float hqBonus = 1f + (s.hqLevel - 1) * 0.03f;                // +3% por nível
            float aeroBonus = 1f + (s.aeroDeptLevel - 1) * 0.05f;        // +5% por nível
            float puBonus = 1f + (s.puDeptLevel - 1) * 0.05f;            // +5% por nível

            // R&D speed geral (híbrido)
            s.rndSpeedMultiplier = Clamp01ToRange(hqBonus * ((aeroBonus + puBonus) * 0.5f), 0.85f, 1.45f);

            // =========================
            // R&D COST: quanto menor, mais barato
            // =========================
            // HQ melhora processos e compra melhor
            float costReduction = 1f - (s.hqLevel - 1) * 0.02f;          // -2% por nível
            // limites pra não quebrar economia
            s.rndCostMultiplier = Mathf.Clamp(costReduction, 0.70f, 1.00f);

            // =========================
            // R&D RISK: quanto menor, menos chance de "known issues"
            // =========================
            float riskReduction =
                1f
                - (s.hqLevel - 1) * 0.015f
                - (s.strategyTeamLevel - 1) * 0.02f;

            s.rndRiskMultiplier = Mathf.Clamp(riskReduction, 0.70f, 1.05f);

            // =========================
            // Manufacturing: speed/cost
            // =========================
            float manufSpeed = 1f + (s.hqLevel - 1) * 0.03f;
            s.manufacturingSpeedMultiplier = Mathf.Clamp(manufSpeed, 0.85f, 1.40f);

            float manufCost = 1f - (s.hqLevel - 1) * 0.015f;
            s.manufacturingCostMultiplier = Mathf.Clamp(manufCost, 0.75f, 1.00f);
        }

        private static float Clamp01ToRange(float v, float min, float max)
        {
            return Mathf.Clamp(v, min, max);
        }
    }
}
