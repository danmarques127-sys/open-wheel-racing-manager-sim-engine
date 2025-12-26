using System;
using System.Collections.Generic;
using UnityEngine;

namespace F1Manager.Data
{
    public enum TrackType
    {
        Street = 0,
        Permanent = 1,
        Mixed = 2
    }

    // “Ultrapassagem 2026” (substitui DRS) — escolha o que seu jogo vai usar.
    public enum OvertakeAidType
    {
        None = 0,
        ActiveAero = 1,     // asa/elementos ativos
        PushToPass = 2,     // boost por “cargas” (like IndyCar)
        ERSBurst = 3,       // descarga extra (modo ataque)
        Hybrid = 4          // combinações
    }

    // Formato do fim de semana
    public enum WeekendFormat
    {
        Standard = 0,       // FP1 FP2 FP3 Q R
        Sprint = 1          // Sprint weekend (você define a lógica do seu jogo)
    }

    // Compostos do jogo (pode expandir para C1..C5 depois, se quiser)
    public enum TireCompound
    {
        Hard = 0,
        Medium = 1,
        Soft = 2,
        Intermediate = 3,
        Wet = 4
    }

    // Severidade geral de desgaste na pista (útil pra UI/AI rápida)
    public enum TireWearLevel
    {
        Low = 0,
        Medium = 1,
        High = 2
    }

    // Perfil por composto, por pista (vida útil e comportamento)
    [Serializable]
    public class TireCompoundProfile
    {
        public TireCompound compound = TireCompound.Medium;

        [Header("Stint Life (laps)")]
        [Min(1)] public int expectedLifeLaps = 18;          // vida “média” em voltas
        [Min(0)] public int lifeVarianceLaps = 2;           // variação (AI + aleatoriedade)

        [Header("Performance")]
        public float paceDeltaSec = 0.0f;                   // delta vs referência (ex: Medium = 0)
        [Range(0.1f, 3.0f)] public float warmupRate = 1.0f; // 1 = normal, >1 aquece rápido
        [Range(0.1f, 3.0f)] public float degradationRate = 1.0f; // 1 = normal, >1 degrada rápido

        [Header("Weather Compatibility")]
        [Range(0, 100)] public int minRainPercent = 0;      // chuva mínima recomendada
        [Range(0, 100)] public int maxRainPercent = 100;    // chuva máxima recomendada
    }

    [CreateAssetMenu(fileName = "Track_", menuName = "F1 Manager/Data/Track Data", order = 30)]
    public class TrackData : ScriptableObject
    {
        [Header("Identity")]
        public string trackId;       // ex: "bahrain"
        public string trackName;     // ex: "Bahrain International Circuit"
        public string country;       // ex: "Bahrain"
        public string city;          // opcional (ex: "Sakhir")
        public string timezoneId;    // opcional (ex: "Asia/Bahrain")

        [Header("Layout")]
        public TrackType type = TrackType.Permanent;
        [Min(0.1f)] public float lapLengthKm = 5.4f;
        [Min(1)] public int laps = 57;

        [Header("Weekend")]
        public WeekendFormat weekendFormat = WeekendFormat.Standard;
        public bool hasSprint => weekendFormat == WeekendFormat.Sprint;

        [Tooltip("Se quiser, use isso pra simular sprint como corrida curta (p.ex. 1/3 do GP).")]
        [Min(0)] public int sprintLapsOverride = 0; // 0 = auto/ignorar

        [Header("Race Characteristics")]
        public TireWearLevel tireWear = TireWearLevel.Medium;

        [Range(0, 100)] public int rainChance = 10;
        [Range(0, 100)] public int safetyCarChance = 25;
        [Range(0, 100)] public int vscChance = 20;
        [Range(0, 100)] public int redFlagChance = 3;

        [Header("Pit Lane")]
        [Min(0f)] public float pitLaneTimeLossSec = 22f;    // tempo perdido num pit (entrada+stop+saída)
        [Min(1.5f)] public float pitStopBaseSec = 2.2f;     // tempo base do pit-stop (troca de pneus)
        [Min(0f)] public float pitStopVarSec = 0.35f;       // variação (erros/execução)

        [Header("Overtaking (2026+)")]
        public OvertakeAidType overtakeAid = OvertakeAidType.ActiveAero;

        [Tooltip("0 = quase impossível; 1 = fácil. Afeta chance de ultrapassagem e quão forte precisa ser o ataque.")]
        [Range(0f, 1f)] public float overtakingEase = 0.45f;

        [Tooltip("Quão potente é a ajuda de ultrapassagem nesta pista (efeito médio).")]
        [Range(0f, 2f)] public float overtakeAidStrength = 1.0f;

        [Tooltip("Quantos pontos fortes de ultrapassagem (zones) existem. Não é DRS; são 'opportunity zones'.")]
        [Min(0)] public int overtakeZones = 2;

        [Header("Difficulty / Driver Skill")]
        [Range(1, 10)] public int difficulty = 5; // 1 = fácil, 10 = muito difícil

        [Header("Track Surface & Evolution")]
        [Range(0.5f, 1.5f)] public float asphaltAbrasiveness = 1.0f; // >1 desgasta mais
        [Range(0.5f, 1.5f)] public float trackGripBase = 1.0f;       // grip base
        [Range(0f, 1.5f)] public float trackEvolutionRate = 0.8f;    // quanto a pista “emborracha”
        [Range(0f, 1.5f)] public float kerbAggression = 0.7f;        // risco de dano/instabilidade

        [Header("Car Sensitivities (meta do setup)")]
        [Range(0f, 1f)] public float powerSensitivity = 0.55f; // importância do motor/retas
        [Range(0f, 1f)] public float aeroSensitivity = 0.65f;  // importância de aero
        [Range(0f, 1f)] public float brakeSeverity = 0.55f;    // estresse de freio
        [Range(0f, 1f)] public float tyreEnergy = 0.60f;       // quão fácil “superaquecer”
        [Range(0f, 1f)] public float coolingDifficulty = 0.50f;// resfriamento do carro (calor + altitude)

        [Header("Environment")]
        [Range(-50f, 60f)] public float avgAirTempC = 28f;
        [Range(-10f, 60f)] public float trackTempBiasC = 10f; // pista geralmente mais quente que o ar
        [Min(0f)] public float altitudeMeters = 0f;            // afeta potência/cooling (se você quiser)

        [Header("Geometry / Flavor (para UI, mídia e sim)")]
        [Min(0)] public int corners = 15;
        [Min(0)] public int slowCorners = 5;
        [Min(0)] public int mediumCorners = 7;
        [Min(0)] public int fastCorners = 3;
        [Min(0f)] public float longestStraightMeters = 1100f;
        [Range(0f, 1f)] public float bumpiness = 0.35f;        // rua costuma ser maior

        [Header("Tire Profiles (per compound, per track)")]
        public List<TireCompoundProfile> tireProfiles = new List<TireCompoundProfile>()
        {
            new TireCompoundProfile { compound = TireCompound.Hard, expectedLifeLaps = 24, lifeVarianceLaps = 2, paceDeltaSec = +0.60f, warmupRate = 0.85f, degradationRate = 0.85f, minRainPercent = 0,  maxRainPercent = 20 },
            new TireCompoundProfile { compound = TireCompound.Medium, expectedLifeLaps = 18, lifeVarianceLaps = 2, paceDeltaSec = 0.00f,  warmupRate = 1.00f, degradationRate = 1.00f, minRainPercent = 0,  maxRainPercent = 20 },
            new TireCompoundProfile { compound = TireCompound.Soft, expectedLifeLaps = 12, lifeVarianceLaps = 2, paceDeltaSec = -0.55f, warmupRate = 1.25f, degradationRate = 1.25f, minRainPercent = 0,  maxRainPercent = 15 },
            new TireCompoundProfile { compound = TireCompound.Intermediate, expectedLifeLaps = 16, lifeVarianceLaps = 3, paceDeltaSec = +1.10f, warmupRate = 1.10f, degradationRate = 1.05f, minRainPercent = 15, maxRainPercent = 70 },
            new TireCompoundProfile { compound = TireCompound.Wet, expectedLifeLaps = 14, lifeVarianceLaps = 3, paceDeltaSec = +2.10f, warmupRate = 1.05f, degradationRate = 1.10f, minRainPercent = 55, maxRainPercent = 100 }
        };

        // Distância total do GP
        public float TotalRaceDistanceKm => lapLengthKm * laps;

        // Se quiser um padrão automático pra sprint (ex: ~1/3 da corrida)
        public int GetSprintLapsDefault()
        {
            if (!hasSprint) return 0;
            if (sprintLapsOverride > 0) return sprintLapsOverride;
            return Mathf.Clamp(Mathf.RoundToInt(laps * 0.33f), 10, Mathf.Max(10, laps - 1));
        }

        // Pegue um perfil do composto de pneu (útil pra estratégia)
        public TireCompoundProfile GetTireProfile(TireCompound compound)
        {
            for (int i = 0; i < tireProfiles.Count; i++)
            {
                if (tireProfiles[i] != null && tireProfiles[i].compound == compound)
                    return tireProfiles[i];
            }
            return null;
        }

        // Exemplo simples: vida efetiva ajustada pela pista (abrasividade + tyreEnergy + wearLevel)
        public int EstimateTireLifeLaps(TireCompound compound, float driverSmoothness01 = 0.5f)
        {
            var p = GetTireProfile(compound);
            if (p == null) return 0;

            float wearLevelMul = (tireWear == TireWearLevel.Low) ? 0.90f : (tireWear == TireWearLevel.High ? 1.15f : 1.0f);

            // “Smoothness”: 1.0 = muito suave (menos desgaste), 0.0 = agressivo (mais desgaste)
            float smoothMul = Mathf.Lerp(1.10f, 0.90f, Mathf.Clamp01(driverSmoothness01));

            float surfaceMul = asphaltAbrasiveness;
            float heatMul = Mathf.Lerp(0.95f, 1.10f, tyreEnergy); // tyreEnergy alto = mais risco de degradação

            float totalMul = wearLevelMul * surfaceMul * heatMul;

            // degradação do composto também pesa
            totalMul *= Mathf.Lerp(0.90f, 1.25f, Mathf.Clamp01(p.degradationRate - 1.0f + 0.5f));

            float life = p.expectedLifeLaps * smoothMul / Mathf.Max(0.2f, totalMul);
            return Mathf.Max(1, Mathf.RoundToInt(life));
        }

        public void Validate()
        {
            trackId = (trackId ?? "").Trim();
            trackName = (trackName ?? "").Trim();
            country = (country ?? "").Trim();
            city = (city ?? "").Trim();
            timezoneId = (timezoneId ?? "").Trim();

            lapLengthKm = Mathf.Max(0.1f, lapLengthKm);
            laps = Mathf.Max(1, laps);

            difficulty = Mathf.Clamp(difficulty, 1, 10);

            rainChance = Mathf.Clamp(rainChance, 0, 100);
            safetyCarChance = Mathf.Clamp(safetyCarChance, 0, 100);
            vscChance = Mathf.Clamp(vscChance, 0, 100);
            redFlagChance = Mathf.Clamp(redFlagChance, 0, 100);

            pitLaneTimeLossSec = Mathf.Max(0f, pitLaneTimeLossSec);
            pitStopBaseSec = Mathf.Max(1.5f, pitStopBaseSec);
            pitStopVarSec = Mathf.Max(0f, pitStopVarSec);

            overtakeZones = Mathf.Max(0, overtakeZones);
            overtakingEase = Mathf.Clamp01(overtakingEase);
            overtakeAidStrength = Mathf.Clamp(overtakeAidStrength, 0f, 2f);

            asphaltAbrasiveness = Mathf.Clamp(asphaltAbrasiveness, 0.5f, 1.5f);
            trackGripBase = Mathf.Clamp(trackGripBase, 0.5f, 1.5f);
            trackEvolutionRate = Mathf.Clamp(trackEvolutionRate, 0f, 1.5f);
            kerbAggression = Mathf.Clamp(kerbAggression, 0f, 1.5f);

            powerSensitivity = Mathf.Clamp01(powerSensitivity);
            aeroSensitivity = Mathf.Clamp01(aeroSensitivity);
            brakeSeverity = Mathf.Clamp01(brakeSeverity);
            tyreEnergy = Mathf.Clamp01(tyreEnergy);
            coolingDifficulty = Mathf.Clamp01(coolingDifficulty);

            altitudeMeters = Mathf.Max(0f, altitudeMeters);

            corners = Mathf.Max(0, corners);
            slowCorners = Mathf.Max(0, slowCorners);
            mediumCorners = Mathf.Max(0, mediumCorners);
            fastCorners = Mathf.Max(0, fastCorners);
            longestStraightMeters = Mathf.Max(0f, longestStraightMeters);
            bumpiness = Mathf.Clamp01(bumpiness);

            sprintLapsOverride = Mathf.Max(0, sprintLapsOverride);

            // Garante que existe pelo menos 1 profile por composto (sem duplicar)
            EnsureDefaultTireProfiles();

            // Sanitiza profiles
            for (int i = 0; i < tireProfiles.Count; i++)
            {
                var p = tireProfiles[i];
                if (p == null) continue;

                p.expectedLifeLaps = Mathf.Max(1, p.expectedLifeLaps);
                p.lifeVarianceLaps = Mathf.Max(0, p.lifeVarianceLaps);
                p.warmupRate = Mathf.Clamp(p.warmupRate, 0.1f, 3.0f);
                p.degradationRate = Mathf.Clamp(p.degradationRate, 0.1f, 3.0f);
                p.minRainPercent = Mathf.Clamp(p.minRainPercent, 0, 100);
                p.maxRainPercent = Mathf.Clamp(p.maxRainPercent, 0, 100);

                if (p.maxRainPercent < p.minRainPercent)
                    p.maxRainPercent = p.minRainPercent;
            }
        }

        private void EnsureDefaultTireProfiles()
        {
            if (tireProfiles == null) tireProfiles = new List<TireCompoundProfile>();

            // Marca existentes
            bool hasHard = false, hasMed = false, hasSoft = false, hasInter = false, hasWet = false;
            for (int i = 0; i < tireProfiles.Count; i++)
            {
                if (tireProfiles[i] == null) continue;
                switch (tireProfiles[i].compound)
                {
                    case TireCompound.Hard: hasHard = true; break;
                    case TireCompound.Medium: hasMed = true; break;
                    case TireCompound.Soft: hasSoft = true; break;
                    case TireCompound.Intermediate: hasInter = true; break;
                    case TireCompound.Wet: hasWet = true; break;
                }
            }

            if (!hasHard) tireProfiles.Add(new TireCompoundProfile { compound = TireCompound.Hard, expectedLifeLaps = 24, lifeVarianceLaps = 2, paceDeltaSec = +0.60f, warmupRate = 0.85f, degradationRate = 0.85f, minRainPercent = 0, maxRainPercent = 20 });
            if (!hasMed) tireProfiles.Add(new TireCompoundProfile { compound = TireCompound.Medium, expectedLifeLaps = 18, lifeVarianceLaps = 2, paceDeltaSec = 0.00f, warmupRate = 1.00f, degradationRate = 1.00f, minRainPercent = 0, maxRainPercent = 20 });
            if (!hasSoft) tireProfiles.Add(new TireCompoundProfile { compound = TireCompound.Soft, expectedLifeLaps = 12, lifeVarianceLaps = 2, paceDeltaSec = -0.55f, warmupRate = 1.25f, degradationRate = 1.25f, minRainPercent = 0, maxRainPercent = 15 });
            if (!hasInter) tireProfiles.Add(new TireCompoundProfile { compound = TireCompound.Intermediate, expectedLifeLaps = 16, lifeVarianceLaps = 3, paceDeltaSec = +1.10f, warmupRate = 1.10f, degradationRate = 1.05f, minRainPercent = 15, maxRainPercent = 70 });
            if (!hasWet) tireProfiles.Add(new TireCompoundProfile { compound = TireCompound.Wet, expectedLifeLaps = 14, lifeVarianceLaps = 3, paceDeltaSec = +2.10f, warmupRate = 1.05f, degradationRate = 1.10f, minRainPercent = 55, maxRainPercent = 100 });
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            Validate();
        }
#endif
    }
}
