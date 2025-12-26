using System;
using UnityEngine;

namespace F1Manager.Core.Drivers
{
    [Serializable]
    public struct DriverStats
    {
        [Range(0f, 100f)] public float pace;
        [Range(0f, 100f)] public float consistency;
        [Range(0f, 100f)] public float racecraft;
        [Range(0f, 100f)] public float tyreMgmt;
        [Range(0f, 100f)] public float wetSkill;
        [Range(0f, 100f)] public float starts;
        [Range(0f, 100f)] public float morale; // influencia performance e risco

        // Atributos "ocultos" (scouting descobre)
        [Range(0f, 100f)] public float potential;      // teto de crescimento
        [Range(0f, 100f)] public float adaptability;   // muda de equipe/carro
        [Range(0f, 100f)] public float workEthic;      // evolução mais rápida

        public float OverallVisible =>
            (pace * 0.28f) +
            (consistency * 0.18f) +
            (racecraft * 0.16f) +
            (tyreMgmt * 0.12f) +
            (wetSkill * 0.10f) +
            (starts * 0.08f) +
            (morale * 0.08f);

        public float OverallTrue =>
            OverallVisible +
            (potential * 0.06f) +
            (adaptability * 0.04f) +
            (workEthic * 0.04f);

        public static float Clamp01To100(float v) => Mathf.Clamp(v, 0f, 100f);

        public void ClampAll()
        {
            pace = Clamp01To100(pace);
            consistency = Clamp01To100(consistency);
            racecraft = Clamp01To100(racecraft);
            tyreMgmt = Clamp01To100(tyreMgmt);
            wetSkill = Clamp01To100(wetSkill);
            starts = Clamp01To100(starts);
            morale = Clamp01To100(morale);

            potential = Clamp01To100(potential);
            adaptability = Clamp01To100(adaptability);
            workEthic = Clamp01To100(workEthic);
        }
    }
}
