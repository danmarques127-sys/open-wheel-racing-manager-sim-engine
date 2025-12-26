using UnityEngine;
using F1Manager.Data;
using F1Manager.Core.Car;

namespace F1Manager.Core.Car
{
    public static class TeamCarInitializer
    {
        // Converte 0..100 do TeamData em “levels” por peça.
        // Esses numbers são a BASE; upgrades alteram depois.
        public static TeamCarState BuildFromTeamData(TeamData team, int specYear = 2026)
        {
            var car = new TeamCarState { teamId = team.teamId };
            car.EnsureAllPartsExist(specYear);

            // ------------- Helpers -------------
            float A(float v) => Mathf.Clamp(v, 0f, 100f); // clamp 0..100
            void SetPerf(CarPartType t, float perf)
            {
                var p = car.GetPart(t);
                if (p == null) return;
                p.performanceLevel = A(perf);
            }
            void SetRel(CarPartType t, float rel)
            {
                var p = car.GetPart(t);
                if (p == null) return;
                p.reliabilityLevel = A(rel);
            }

            // ------------- Derivados por domínio -------------
            // AERO core (0..100)
            float aeroCore =
                (team.aero2026.aeroEfficiency +
                 team.aero2026.dragEfficiency +
                 team.aero2026.downforceLowSpeed +
                 team.aero2026.downforceMediumSpeed +
                 team.aero2026.downforceHighSpeed) / 5f;

            float aeroActive =
                (team.aero2026.activeAeroEfficiency +
                 team.aero2026.activeAeroResponseTime +
                 team.aero2026.activeAeroStability) / 3f;

            float dirtyAir = team.aero2026.dirtyAirResistance;

            // PU core
            float puIceCore = (team.powerUnit2026.icePower + team.powerUnit2026.iceEfficiency + team.powerUnit2026.fuelEfficiency) / 3f;
            float puMGUKCore = (team.powerUnit2026.mguKPower + team.powerUnit2026.mguKEfficiency + team.powerUnit2026.mguKHeatControl) / 3f;
            float puEnergyCore =
                (team.powerUnit2026.energyHarvestRate +
                 team.powerUnit2026.energyDeployRate +
                 team.powerUnit2026.energyStorageCapacity +
                 team.powerUnit2026.energyManagementAI) / 4f;

            // CHASSIS core
            float chassisCore =
                (team.chassis2026.mechanicalGrip +
                 team.chassis2026.suspensionQuality +
                 team.chassis2026.chassisStiffness +
                 team.chassis2026.weightDistribution +
                 team.chassis2026.brakingStability +
                 team.chassis2026.traction) / 6f;

            // SYSTEMS core
            float coolingCore =
                (team.systems2026.coolingEfficiency +
                 team.systems2026.thermalControl +
                 team.systems2026.controlSystems) / 3f;

            // RELIABILITY
            float relOverall = team.reliability2026.overallReliability;
            float relERS = team.reliability2026.ersReliability;
            float relICE = team.reliability2026.iceReliability;
            float relMGUK = team.reliability2026.mguKReliability;
            float relAA = team.reliability2026.activeAeroReliability;
            float relGB = team.reliability2026.gearboxReliability;
            float relElec = team.reliability2026.electronicsReliability;
            float relCooling = team.systems2026.coolingReliability;

            // ------------- Mapeamento para peças -------------
            // AERO pieces
            SetPerf(CarPartType.FrontWing, (aeroCore * 0.55f) + (dirtyAir * 0.25f) + (aeroActive * 0.20f));
            SetPerf(CarPartType.RearWing,  (aeroCore * 0.60f) + (team.aero2026.dragEfficiency * 0.25f) + (aeroActive * 0.15f));
            SetPerf(CarPartType.Floor,     (aeroCore * 0.75f) + (team.aero2026.downforceMediumSpeed * 0.15f) + (dirtyAir * 0.10f));
            SetPerf(CarPartType.Diffuser,  (aeroCore * 0.65f) + (team.aero2026.downforceHighSpeed * 0.20f) + (team.aero2026.dragEfficiency * 0.15f));
            SetPerf(CarPartType.SidepodsCoolingAero, (aeroCore * 0.40f) + (coolingCore * 0.60f));

            SetRel(CarPartType.FrontWing, relOverall * 0.70f + relAA * 0.30f);
            SetRel(CarPartType.RearWing,  relOverall * 0.70f + relAA * 0.30f);
            SetRel(CarPartType.Floor,     relOverall);
            SetRel(CarPartType.Diffuser,  relOverall);
            SetRel(CarPartType.SidepodsCoolingAero, relCooling * 0.60f + relOverall * 0.40f);

            // CHASSIS pieces
            SetPerf(CarPartType.Chassis,    chassisCore * 0.75f + team.chassis2026.chassisStiffness * 0.25f);
            SetPerf(CarPartType.Suspension, chassisCore * 0.50f + team.chassis2026.suspensionQuality * 0.50f);
            SetPerf(CarPartType.Brakes,     chassisCore * 0.40f + team.chassis2026.brakingStability * 0.60f);
            SetPerf(CarPartType.Steering,   chassisCore * 0.60f + team.chassis2026.mechanicalGrip * 0.40f);
            SetPerf(CarPartType.Gearbox,    chassisCore * 0.25f + puIceCore * 0.35f + puMGUKCore * 0.40f);

            SetRel(CarPartType.Chassis,    relOverall);
            SetRel(CarPartType.Suspension, relOverall);
            SetRel(CarPartType.Brakes,     relOverall);
            SetRel(CarPartType.Steering,   relOverall);
            SetRel(CarPartType.Gearbox,    relGB * 0.70f + relOverall * 0.30f);

            // PU / HYBRID pieces
            SetPerf(CarPartType.ICE,               puIceCore);
            SetPerf(CarPartType.MGU_K,             puMGUKCore);
            SetPerf(CarPartType.EnergyStore,       puEnergyCore * 0.75f + team.powerUnit2026.energyStorageCapacity * 0.25f);
            SetPerf(CarPartType.ControlElectronics,puEnergyCore * 0.55f + team.powerUnit2026.energyManagementAI * 0.45f);
            SetPerf(CarPartType.Turbo,             puIceCore * 0.55f + team.powerUnit2026.icePower * 0.45f);
            SetPerf(CarPartType.ERS_Cooling,       coolingCore * 0.75f + team.powerUnit2026.mguKHeatControl * 0.25f);

            SetRel(CarPartType.ICE,               relICE * 0.70f + relOverall * 0.30f);
            SetRel(CarPartType.Turbo,             relICE * 0.55f + relOverall * 0.45f);
            SetRel(CarPartType.MGU_K,             relMGUK * 0.70f + relERS * 0.30f);
            SetRel(CarPartType.EnergyStore,       relERS * 0.70f + relElec * 0.30f);
            SetRel(CarPartType.ControlElectronics,relElec * 0.75f + relERS * 0.25f);
            SetRel(CarPartType.ERS_Cooling,       relCooling * 0.70f + relERS * 0.30f);

            // SYSTEMS pieces
            SetPerf(CarPartType.Cooling,      coolingCore);
            SetPerf(CarPartType.Electronics,  team.systems2026.controlSystems);
            SetPerf(CarPartType.Hydraulics,   chassisCore * 0.40f + relOverall * 0.60f);

            SetRel(CarPartType.Cooling,     relCooling);
            SetRel(CarPartType.Electronics, relElec);
            SetRel(CarPartType.Hydraulics,  relOverall);

            // SAFETY (não é “performance” real, mas mantém compatível)
            SetPerf(CarPartType.SafetyCell, 50f);
            SetPerf(CarPartType.FireSystem, 50f);
            SetPerf(CarPartType.Lights, 50f);

            SetRel(CarPartType.SafetyCell, 80f);
            SetRel(CarPartType.FireSystem, 80f);
            SetRel(CarPartType.Lights, 80f);

            return car;
        }
    }
}
