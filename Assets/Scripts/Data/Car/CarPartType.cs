namespace F1Manager.Data
{
    // Partes “jogáveis” do carro (manager-friendly).
    // Mantém coerência com FIA (por seção) e com gameplay (custo/risco/ganhos).
    public enum CarPartType
    {
        // AERO
        FrontWing = 0,
        RearWing = 1,
        Floor = 2,
        Diffuser = 3,
        SidepodsCoolingAero = 4,

        // CHASSIS
        Chassis = 10,      // Monocoque/chassis concept (rigidez/weight)
        Suspension = 11,
        Brakes = 12,
        Steering = 13,     // opcional (mas útil)
        Gearbox = 14,      // Transmission assembly (pode englobar)

        // POWER UNIT / HYBRID (simplificado, mas alinhado com 2026)
        ICE = 20,
        Turbo = 21,
        MGU_K = 22,
        EnergyStore = 23,         // Battery
        ControlElectronics = 24,  // CE
        ERS_Cooling = 25,         // opcional mas MUITO útil no meta 2026

        // SYSTEMS
        Cooling = 30,     // radiators/heat exchangers (geral)
        Electronics = 31, // sensors/telemetry bundle
        Hydraulics = 32,  // opcional

        // SAFETY (você pode deixar abstrato ou detalhar)
        SafetyCell = 40,  // “survival cell” / safety structure (abstrato)
        FireSystem = 41,  // extintor/linha (abstrato)
        Lights = 42       // lights / systems required
    }
}
