namespace F1Manager.Data
{
    // Mapa “macro” inspirado na divisão do Technical Regulations (Section C).
    public enum FIASection
    {
        Aero_C3 = 0,
        Mass_C4 = 1,
        PowerUnit_C5 = 2,
        FuelSystem_C6 = 3,
        FluidsCooling_C7 = 4,
        Electrical_C8 = 5,
        Transmission_C9 = 6,
        Safety_C14 = 7,

        // Reserva / futuro (caso você queira expandir com Sporting/Financial etc.)
        Other = 99
    }
}
