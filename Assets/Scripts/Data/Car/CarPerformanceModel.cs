using System;
using UnityEngine;

namespace F1Manager.Data
{
    [Serializable]
    public class CarPerformanceResult
    {
        public float aeroEfficiency;
        public float mechanicalGrip;
        public float powerOutput;
        public float reliability;
        public float weight;

        public float Overall
        {
            get
            {
                // Fórmula base (ajuste depois sem quebrar nada)
                // Peso (weight) aqui funciona como “penalidade” (mais peso = pior),
                // então subtrai um pouco.
                float core = aeroEfficiency + mechanicalGrip + powerOutput;
                float rel = reliability * 0.5f;
                float massPenalty = weight * 0.25f;
                return core + rel - massPenalty;
            }
        }
    }

    public static class CarPerformanceModel
    {
        // Converte “levels” do estado em pontos aplicáveis.
        // Você pode fazer log/curva depois, mas começa linear.
        public static CarPerformanceResult Compute(
            Func<CarPartType, float> getPerformanceLevel,
            Func<CarPartType, float> getReliabilityLevel,
            Func<CarPartType, float> getWearPercent,
            Func<CarPartType, CarPartDefinition> getDefinition)
        {
            var r = new CarPerformanceResult();

            foreach (CarPartType t in (CarPartType[])Enum.GetValues(typeof(CarPartType)))
            {
                var def = getDefinition(t);
                if (def == null) continue;

                float perf = getPerformanceLevel(t);
                float rel = getReliabilityLevel(t);

                // Wear degrada tanto performance quanto reliability
                float wear = Mathf.Clamp01(getWearPercent(t) / 100f);
                float wearPerfMul = Mathf.Lerp(1f, 0.75f, wear);
                float wearRelMul = Mathf.Lerp(1f, 0.65f, wear);

                float perfApplied = perf * wearPerfMul;
                float relApplied = rel * wearRelMul;

                r.aeroEfficiency += perfApplied * def.weights.aeroEfficiency;
                r.mechanicalGrip += perfApplied * def.weights.mechanicalGrip;
                r.powerOutput += perfApplied * def.weights.powerOutput;

                r.reliability += relApplied * def.weights.reliability;

                // Weight usa perf como “delta de pacote” (pode ser refinado depois)
                r.weight += perfApplied * def.weights.weight;
            }

            return r;
        }
    }
}
