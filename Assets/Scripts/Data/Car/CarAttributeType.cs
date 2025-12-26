namespace F1Manager.Data
{
    // O “modelo universal” que TODAS as peças afetam (com pesos diferentes).
    public enum CarAttributeType
    {
        AeroEfficiency = 0,   // downforce/drag tradeoff
        MechanicalGrip = 1,   // chassis/suspension/brake behavior
        PowerOutput = 2,      // ICE + hybrid
        Reliability = 3,      // DNF chance / failures / degradation
        Weight = 4            // mass delta (meta)
    }
}
