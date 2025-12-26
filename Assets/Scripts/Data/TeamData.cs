using UnityEngine;

[CreateAssetMenu(fileName = "Team_", menuName = "F1 Manager/Data/Team", order = 1)]
public class TeamData : ScriptableObject
{
    [Header("Identity")]
    public string teamId;                 // ex: "mercedes"
    public string displayName;            // ex: "Mercedes"
    public string shortName;              // ex: "MER"
    public Color primaryColor = Color.white;

    [Header("2026 Budget / Staff (simplificado)")]
    public int startingBudgetMillions = 200;
    public int hqLevel = 1;
    public int aeroDepartment = 1;
    public int powerUnitDept = 1;
    public int strategyTeam = 1;

    // ============================================================
    // LEGACY BASELINES (mantidos para compatibilidade)
    // ============================================================
    [Header("Car baseline (LEGACY 0..100) - mantido para compatibilidade")]
    [Range(0, 100)] public int aero = 50;
    [Range(0, 100)] public int powerUnit = 50;
    [Range(0, 100)] public int chassis = 50;
    [Range(0, 100)] public int reliability = 50;

    // ============================================================
    // 2026 CAR SPEC (DETAILED)
    // ============================================================

    [Header("2026 Car Spec - Aero (0..100)")]
    public Aero2026 aero2026 = new Aero2026();

    [Header("2026 Car Spec - Power Unit / ERS / MOM (0..100)")]
    public PowerUnit2026 powerUnit2026 = new PowerUnit2026();

    [Header("2026 Car Spec - Chassis & Mechanical (0..100)")]
    public Chassis2026 chassis2026 = new Chassis2026();

    [Header("2026 Car Spec - Cooling / Thermal / Systems (0..100)")]
    public Systems2026 systems2026 = new Systems2026();

    [Header("2026 Car Spec - Reliability by subsystem (0..100)")]
    public Reliability2026 reliability2026 = new Reliability2026();

    [Header("Meta")]
    public string country;
    public string baseCity;

    // ============================================================
    // OPTIONAL: DERIVED "CARD" STATS (read-only helpers)
    // Você pode usar isso pra UI (cards modernos).
    // ============================================================

    public int Card_AeroEfficiency => Mathf.RoundToInt(
        (aero2026.aeroEfficiency +
         aero2026.dragEfficiency +
         aero2026.activeAeroEfficiency) / 3f
    );

    public int Card_TopSpeed => Mathf.RoundToInt(
        (aero2026.dragEfficiency * 0.45f) +
        (powerUnit2026.icePower * 0.25f) +
        (powerUnit2026.mguKPower * 0.25f) +
        (powerUnit2026.energyDeployRate * 0.05f)
    );

    public int Card_Acceleration => Mathf.RoundToInt(
        (powerUnit2026.mguKPower * 0.45f) +
        (powerUnit2026.energyDeployRate * 0.25f) +
        (powerUnit2026.icePower * 0.20f) +
        (chassis2026.mechanicalGrip * 0.10f)
    );

    public int Card_Cornering => Mathf.RoundToInt(
        (aero2026.downforceLowSpeed * 0.35f) +
        (aero2026.downforceMediumSpeed * 0.30f) +
        (aero2026.downforceHighSpeed * 0.15f) +
        (chassis2026.mechanicalGrip * 0.20f)
    );

    public int Card_EnergyManagement => Mathf.RoundToInt(
        (powerUnit2026.energyManagementAI * 0.30f) +
        (powerUnit2026.energyHarvestRate * 0.25f) +
        (powerUnit2026.energyDeployRate * 0.20f) +
        (powerUnit2026.energyStorageCapacity * 0.15f) +
        (systems2026.coolingEfficiency * 0.10f)
    );

    public int Card_Reliability => Mathf.RoundToInt(
        (reliability2026.overallReliability * 0.40f) +
        (reliability2026.ersReliability * 0.20f) +
        (reliability2026.iceReliability * 0.15f) +
        (reliability2026.activeAeroReliability * 0.10f) +
        (systems2026.coolingReliability * 0.15f)
    );

    // ============================================================
    // DATA BLOCKS
    // ============================================================

    [System.Serializable]
    public class Aero2026
    {
        [Header("Downforce split by speed band")]
        [Range(0, 100)] public int downforceLowSpeed = 50;
        [Range(0, 100)] public int downforceMediumSpeed = 50;
        [Range(0, 100)] public int downforceHighSpeed = 50;

        [Header("Drag & Efficiency")]
        [Tooltip("Quanto menor o arrasto efetivo, melhor. Aqui usamos 0..100 como 'eficiência de drag' (maior = melhor).")]
        [Range(0, 100)] public int dragEfficiency = 50;

        [Tooltip("Aero efficiency geral: downforce útil por custo de drag (maior = melhor).")]
        [Range(0, 100)] public int aeroEfficiency = 50;

        [Header("Dirty Air")]
        [Tooltip("Resistência à perda de performance seguindo outro carro (maior = melhor).")]
        [Range(0, 100)] public int dirtyAirResistance = 50;

        [Header("Active Aerodynamics (2026)")]
        [Tooltip("Quão bem o sistema de aero ativa entrega ganho real (maior = melhor).")]
        [Range(0, 100)] public int activeAeroEfficiency = 50;

        [Tooltip("Quão rápido o carro responde a mudanças de modo (maior = melhor).")]
        [Range(0, 100)] public int activeAeroResponseTime = 50;

        [Tooltip("Quão bem o carro mantém estabilidade durante transições de aero ativa (maior = melhor).")]
        [Range(0, 100)] public int activeAeroStability = 50;
    }

    [System.Serializable]
    public class PowerUnit2026
    {
        [Header("ICE (Internal Combustion Engine)")]
        [Range(0, 100)] public int icePower = 50;
        [Range(0, 100)] public int iceEfficiency = 50;

        [Tooltip("Quanto menor o consumo, melhor. Aqui usamos 0..100 como 'eficiência de combustível' (maior = melhor).")]
        [Range(0, 100)] public int fuelEfficiency = 50;

        [Header("MGU-K (Electric Power)")]
        [Range(0, 100)] public int mguKPower = 50;
        [Range(0, 100)] public int mguKEfficiency = 50;

        [Tooltip("Controle térmico do elétrico (maior = melhor).")]
        [Range(0, 100)] public int mguKHeatControl = 50;

        [Header("ERS / Energy (2026)")]
        [Tooltip("Taxa de colheita/recuperação (maior = melhor).")]
        [Range(0, 100)] public int energyHarvestRate = 50;

        [Tooltip("Taxa de entrega/uso (maior = melhor).")]
        [Range(0, 100)] public int energyDeployRate = 50;

        [Tooltip("Capacidade útil do armazenamento (maior = melhor).")]
        [Range(0, 100)] public int energyStorageCapacity = 50;

        [Tooltip("Qualidade da lógica/IA que decide como usar energia (maior = melhor).")]
        [Range(0, 100)] public int energyManagementAI = 50;

        [Header("MOM / Mode-of-Operation Management")]
        [Tooltip("Quão flexível é trocar modos (Attack/Defend/Save/Push) (maior = melhor).")]
        [Range(0, 100)] public int ersModeFlexibility = 50;

        [Tooltip("Velocidade/qualidade de troca de modo (maior = melhor).")]
        [Range(0, 100)] public int modeSwitchingSpeed = 50;

        [Tooltip("Sincronia piloto-carro ao trocar modos (maior = melhor).")]
        [Range(0, 100)] public int driverCarSynchronization = 50;
    }

    [System.Serializable]
    public class Chassis2026
    {
        [Header("Mechanical")]
        [Range(0, 100)] public int mechanicalGrip = 50;

        [Tooltip("Qualidade de suspensão / absorção / consistência (maior = melhor).")]
        [Range(0, 100)] public int suspensionQuality = 50;

        [Tooltip("Rigidez estrutural do chassi (maior = melhor até certo ponto).")]
        [Range(0, 100)] public int chassisStiffness = 50;

        [Tooltip("Distribuição/pacote de peso (maior = melhor).")]
        [Range(0, 100)] public int weightDistribution = 50;

        [Header("Stability")]
        [Tooltip("Estabilidade em frenagem e entrada de curva (maior = melhor).")]
        [Range(0, 100)] public int brakingStability = 50;

        [Tooltip("Tração na saída (maior = melhor).")]
        [Range(0, 100)] public int traction = 50;
    }

    [System.Serializable]
    public class Systems2026
    {
        [Header("Cooling & Thermal")]
        [Tooltip("Eficiência geral de resfriamento (maior = melhor).")]
        [Range(0, 100)] public int coolingEfficiency = 50;

        [Tooltip("Robustez do sistema de cooling (maior = melhor).")]
        [Range(0, 100)] public int coolingReliability = 50;

        [Tooltip("Controle térmico do conjunto (ICE + elétrico + eletrônica) (maior = melhor).")]
        [Range(0, 100)] public int thermalControl = 50;

        [Header("Control Systems")]
        [Tooltip("Qualidade de sensores/controle (aero ativa + ERS + integração) (maior = melhor).")]
        [Range(0, 100)] public int controlSystems = 50;
    }

    [System.Serializable]
    public class Reliability2026
    {
        [Header("Subsystem Reliability")]
        [Range(0, 100)] public int overallReliability = 50;

        [Range(0, 100)] public int iceReliability = 50;
        [Range(0, 100)] public int mguKReliability = 50;
        [Range(0, 100)] public int ersReliability = 50;

        [Range(0, 100)] public int activeAeroReliability = 50;
        [Range(0, 100)] public int gearboxReliability = 50;

        [Tooltip("Eletrônica/ECU/wiring (maior = melhor).")]
        [Range(0, 100)] public int electronicsReliability = 50;
    }
}
