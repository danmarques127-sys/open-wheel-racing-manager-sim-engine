using UnityEngine;
using F1Manager.Data;

namespace F1Manager.Core.Sim
{
    public static class PerformanceModel
    {
        // Retorna um "rating final" pro simulador (quanto maior melhor).
        // Hoje simples. Amanhã você pluga upgrades, reliability, track profile, clima, pneus, etc.
        public static float ComputeDriverRacePace(DriverData driver, TeamData team, TrackData track)
        {
            if (driver == null || team == null) return 0f;

            // Ajuste conforme seus campos reais do DriverData/TeamData.
            // Como eu não tenho seus atributos exatos aqui, faço um fallback:
            float driverSkill = 50f;
            float carStrength = 50f;

            // Se você tiver campos, substitui:
            // driverSkill = driver.overall;
            // carStrength = team.carPerformance;

            float trackFactor = track != null ? 1.0f : 1.0f;

            return (driverSkill * 0.55f + carStrength * 0.45f) * trackFactor;
        }
    }
}
