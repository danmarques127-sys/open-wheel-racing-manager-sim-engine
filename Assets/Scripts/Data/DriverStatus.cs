namespace F1Manager.Data
{
    // =========================
    // LEGACY (se você ainda usa em UI antiga)
    // Mistura papel + agente livre. Mantemos por compatibilidade.
    // =========================
    public enum DriverStatus
    {
        Starter = 0,     // Titular
        Reserve = 1,     // Reserva
        FreeAgent = 2    // Sem equipe
    }

    // =========================
    // NOVO: Papel do piloto DENTRO de uma equipe
    // (não inclui FreeAgent)
    // =========================
    public enum DriverRole
    {
        Starter = 0,   // Titular
        Reserve = 1,   // Reserva
        Test = 2       // Test / Simulador
    }

    // =========================
    // NOVO: Situação geral da carreira
    // =========================
    public enum DriverCareerStatus
    {
        Active = 0,
        Injured = 1,
        Suspended = 2,
        Retired = 3
    }

    // =========================
    // Opcional: disponibilidade (redundante, mas pode ajudar UI)
    // Se você quiser manter, OK. Se não usa, pode apagar depois.
    // =========================
    public enum DriverAvailability
    {
        Contracted = 0,
        FreeAgent = 1
    }
}
