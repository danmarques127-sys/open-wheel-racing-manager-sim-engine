using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using F1Manager.Data;

namespace F1Manager.Sim
{
    /// <summary>
    /// Simulador simples de fim de semana:
    /// - TL1/TL2/TL3 (opcional)
    /// - Qualifying (define grid da corrida)
    /// - Sprint (opcional) + grid da corrida pode ou não ser afetado (config)
    /// - Race (principal)
    ///
    /// IMPORTANTE:
    /// - Este arquivo NÃO deve cuidar de pontos, standings, mercado, fim de ano, sponsors.
    /// - Ele apenas devolve "resultados de pista" para outros sistemas consumirem.
    /// </summary>
    public static class SimpleRaceSimulator
    {
        public enum SessionType
        {
            Practice1,
            Practice2,
            Practice3,
            Qualifying,
            Sprint,
            Race
        }

        public class DriverEntry
        {
            public DriverData driver;
            public TeamData team;

            // Scores por sessão
            public float p1Score;
            public float p2Score;
            public float p3Score;

            public float qualScore;
            public float sprintScore;
            public float raceScore;

            // Posições (1..N)
            public int gridPos;        // grid da corrida principal
            public int sprintGridPos;  // grid da sprint (pode ser igual ao quali ou derivado)
            public int sprintFinishPos;
            public int finishPos;      // corrida principal
        }

        public class SessionResult
        {
            public SessionType type;
            public List<DriverEntry> classification; // já ordenado
        }

        public class WeekendResult
        {
            public int seasonYear;
            public RaceWeekendData weekend;

            public List<DriverEntry> entries; // entries finais (com campos preenchidos)
            public List<SessionResult> sessions = new List<SessionResult>();
        }

        [Serializable]
        public class WeekendConfig
        {
            public bool simulatePractice = true;
            public bool simulateSprint = false;

            /// <summary>
            /// Se true: o grid da corrida principal vem do resultado do Sprint (ex.: “Sprint decides grid” - modo arcade/alternativo).
            /// Se false: grid da corrida principal fica do Qualifying (padrão).
            /// </summary>
            public bool sprintAffectsMainGrid = false;

            /// <summary>
            /// Quantos pilotos devem participar. Você pediu 22.
            /// </summary>
            public int maxGridSize = 22;
        }

        // =======================
        // PESOS / TUNING
        // =======================

        private const float QUAL_DRIVER_WEIGHT = 0.70f;
        private const float QUAL_CAR_WEIGHT    = 0.30f;

        private const float RACE_DRIVER_WEIGHT = 0.75f;
        private const float RACE_CAR_WEIGHT    = 0.25f;

        private const float SPRINT_DRIVER_WEIGHT = 0.76f;
        private const float SPRINT_CAR_WEIGHT    = 0.24f;

        private const float PRACTICE_DRIVER_WEIGHT = 0.68f;
        private const float PRACTICE_CAR_WEIGHT    = 0.32f;

        private const float GRID_INFLUENCE_RACE   = 0.08f;
        private const float GRID_INFLUENCE_SPRINT = 0.06f;

        // Ruídos
        private const float NOISE_PRACTICE_STD = 0.09f;
        private const float NOISE_QUAL_STD     = 0.06f;
        private const float NOISE_SPRINT_STD   = 0.085f;
        private const float NOISE_RACE_STD     = 0.09f;

        // ✅ Normalização única (regra definitiva)
        private static string Norm(string s) => (s ?? "").Trim().ToLowerInvariant();

        // =======================
        // API PRINCIPAL
        // =======================

        public static WeekendResult SimulateWeekend(
            RaceWeekendData weekend,
            IReadOnlyList<TeamData> teams,
            IReadOnlyList<DriverData> allDrivers,
            int seasonYear,
            WeekendConfig config = null,
            int seed = 0)
        {
            config ??= new WeekendConfig();

            var rnd = (seed == 0) ? new System.Random() : new System.Random(seed);

            // 1) Monta entries do grid (somente pilotos com contrato assinado e ativos)
            var entries = BuildEntries(teams, allDrivers, seasonYear, config.maxGridSize);

            var result = new WeekendResult
            {
                weekend = weekend,
                seasonYear = seasonYear,
                entries = entries
            };

            if (entries.Count == 0)
            {
                Debug.LogWarning("[Sim] No drivers available to simulate.");
                return result;
            }

            // 2) TLs (opcional) — bom pra “imersão” e futuramente influência leve em setup/confidence
            if (config.simulatePractice)
            {
                SimulatePracticeSession(entries, rnd, SessionType.Practice1);
                result.sessions.Add(new SessionResult
                {
                    type = SessionType.Practice1,
                    classification = entries.OrderByDescending(e => e.p1Score).ToList()
                });

                SimulatePracticeSession(entries, rnd, SessionType.Practice2);
                result.sessions.Add(new SessionResult
                {
                    type = SessionType.Practice2,
                    classification = entries.OrderByDescending(e => e.p2Score).ToList()
                });

                SimulatePracticeSession(entries, rnd, SessionType.Practice3);
                result.sessions.Add(new SessionResult
                {
                    type = SessionType.Practice3,
                    classification = entries.OrderByDescending(e => e.p3Score).ToList()
                });
            }

            // 3) QUALIFYING (define grid base)
            SimulateQualifying(entries, rnd);
            entries = entries.OrderByDescending(x => x.qualScore).ToList();
            for (int i = 0; i < entries.Count; i++)
                entries[i].gridPos = i + 1;

            result.sessions.Add(new SessionResult
            {
                type = SessionType.Qualifying,
                classification = entries.OrderBy(e => e.gridPos).ToList()
            });

            // 4) SPRINT (opcional)
            if (config.simulateSprint)
            {
                // sprintGridPos por padrão = gridPos do quali
                foreach (var e in entries)
                    e.sprintGridPos = e.gridPos;

                SimulateSprint(entries, rnd);

                // Ordena e define sprintFinishPos
                entries = entries.OrderByDescending(x => x.sprintScore).ToList();
                for (int i = 0; i < entries.Count; i++)
                    entries[i].sprintFinishPos = i + 1;

                result.sessions.Add(new SessionResult
                {
                    type = SessionType.Sprint,
                    classification = entries.OrderBy(e => e.sprintFinishPos).ToList()
                });

                // Se sprint decide o grid da corrida principal
                if (config.sprintAffectsMainGrid)
                {
                    // gridPos da corrida principal vira a posição final da sprint
                    foreach (var e in entries)
                        e.gridPos = e.sprintFinishPos;
                }

                // Reordena entries para refletir gridPos atual (se mudou)
                entries = entries.OrderBy(e => e.gridPos).ToList();
                result.entries = entries;
            }

            // 5) RACE (principal)
            SimulateRace(entries, rnd);

            entries = entries.OrderByDescending(x => x.raceScore).ToList();
            for (int i = 0; i < entries.Count; i++)
                entries[i].finishPos = i + 1;

            result.sessions.Add(new SessionResult
            {
                type = SessionType.Race,
                classification = entries.OrderBy(e => e.finishPos).ToList()
            });

            result.entries = entries;
            return result;
        }

        // =======================
        // BUILD ENTRIES
        // =======================

        private static List<DriverEntry> BuildEntries(
            IReadOnlyList<TeamData> teams,
            IReadOnlyList<DriverData> allDrivers,
            int seasonYear,
            int maxGridSize)
        {
            // Regra simples: pilotos com contrato Signed, teamId válido, ativos
            var drivers = allDrivers
                .Where(d => d != null)
                .Where(d => d.contract != null)
                .Where(d => d.contract.status == ContractStatus.Signed)
                .Where(d => !string.IsNullOrWhiteSpace(d.contract.teamId))
                .Where(d => d.careerStatus == DriverCareerStatus.Active)
                .Where(d => seasonYear >= d.contract.contractStartYear && seasonYear <= d.contract.contractEndYear)
                .ToList();

            // ✅ Dicionário com chave normalizada + avisos se houver duplicidade/ID sujo
            var teamById = new Dictionary<string, TeamData>();
            foreach (var t in teams)
            {
                if (t == null) continue;
                if (string.IsNullOrWhiteSpace(t.teamId)) continue;

                string key = Norm(t.teamId);

                if (teamById.ContainsKey(key))
                {
                    Debug.LogWarning($"[Sim] Duplicate TeamId detected after normalization: '{key}'. " +
                                     $"Keeping first, ignoring '{t.name}'.");
                    continue;
                }

                teamById[key] = t;
            }

            var entries = new List<DriverEntry>();

            foreach (var d in drivers)
            {
                string driverTeamKey = Norm(d.contract.teamId);

                if (!teamById.TryGetValue(driverTeamKey, out var team))
                {
                    Debug.LogWarning($"[Sim] Team not found for driver '{d.name}' (driverTeamId='{d.contract.teamId}' -> '{driverTeamKey}').");
                    continue;
                }

                entries.Add(new DriverEntry
                {
                    driver = d,
                    team = team
                });
            }

            // Segurança: se passar do grid alvo, corta pelos melhores overall
            if (entries.Count > maxGridSize)
            {
                entries = entries
                    .OrderByDescending(e => e.driver.overall)
                    .Take(maxGridSize)
                    .ToList();
            }

            return entries;
        }

        // =======================
        // SESSIONS
        // =======================

        private static void SimulatePracticeSession(List<DriverEntry> entries, System.Random rnd, SessionType type)
        {
            foreach (var e in entries)
            {
                float car = GetCarPerformance01(e.team);
                float drv = GetPracticeDriver01(e.driver);
                float noise = NextGaussian(rnd, mean: 0f, stdDev: NOISE_PRACTICE_STD);

                float score = Mathf.Clamp01((drv * PRACTICE_DRIVER_WEIGHT) + (car * PRACTICE_CAR_WEIGHT) + noise);

                switch (type)
                {
                    case SessionType.Practice1: e.p1Score = score; break;
                    case SessionType.Practice2: e.p2Score = score; break;
                    case SessionType.Practice3: e.p3Score = score; break;
                }
            }
        }

        private static void SimulateQualifying(List<DriverEntry> entries, System.Random rnd)
        {
            foreach (var e in entries)
            {
                float car = GetCarPerformance01(e.team);
                float drv = GetQualDriver01(e.driver);
                float noise = NextGaussian(rnd, mean: 0f, stdDev: NOISE_QUAL_STD);

                e.qualScore = Mathf.Clamp01((drv * QUAL_DRIVER_WEIGHT) + (car * QUAL_CAR_WEIGHT) + noise);
            }
        }

        private static void SimulateSprint(List<DriverEntry> entries, System.Random rnd)
        {
            int n = entries.Count;

            foreach (var e in entries)
            {
                float car = GetCarPerformance01(e.team);
                float drv = GetRaceDriver01(e.driver); // sprint usa perfil parecido com corrida

                float gridBonus = (1f - ((e.sprintGridPos - 1f) / Mathf.Max(1f, n - 1f))) * GRID_INFLUENCE_SPRINT;
                float noise = NextGaussian(rnd, mean: 0f, stdDev: NOISE_SPRINT_STD);

                e.sprintScore = Mathf.Clamp01((drv * SPRINT_DRIVER_WEIGHT) + (car * SPRINT_CAR_WEIGHT) + gridBonus + noise);
            }
        }

        private static void SimulateRace(List<DriverEntry> entries, System.Random rnd)
        {
            int n = entries.Count;

            foreach (var e in entries)
            {
                float car = GetCarPerformance01(e.team);
                float drv = GetRaceDriver01(e.driver);

                float gridBonus = (1f - ((e.gridPos - 1f) / Mathf.Max(1f, n - 1f))) * GRID_INFLUENCE_RACE;
                float noise = NextGaussian(rnd, mean: 0f, stdDev: NOISE_RACE_STD);

                e.raceScore = Mathf.Clamp01((drv * RACE_DRIVER_WEIGHT) + (car * RACE_CAR_WEIGHT) + gridBonus + noise);
            }
        }

        // =======================
        // SCORING HELPERS (0..1)
        // =======================

        private static float GetPracticeDriver01(DriverData d)
        {
            // TL = consistência + (qualifying/racePace blend) + um pouco de tireManagement
            float q  = d.qualifying / 100f;
            float rp = d.racePace / 100f;
            float c  = d.consistency / 100f;
            float tm = d.tireManagement / 100f;

            return Mathf.Clamp01((q * 0.30f) + (rp * 0.35f) + (c * 0.25f) + (tm * 0.10f));
        }

        private static float GetQualDriver01(DriverData d)
        {
            // qual = qualifying + um pouco de consistência
            float q = d.qualifying / 100f;
            float c = d.consistency / 100f;
            return Mathf.Clamp01((q * 0.85f) + (c * 0.15f));
        }

        private static float GetRaceDriver01(DriverData d)
        {
            // race = racePace + consistência + tireManagement + wetSkill (pequeno)
            float rp = d.racePace / 100f;
            float c  = d.consistency / 100f;
            float tm = d.tireManagement / 100f;
            float ws = d.wetSkill / 100f;

            return Mathf.Clamp01((rp * 0.70f) + (c * 0.15f) + (tm * 0.10f) + (ws * 0.05f));
        }

        private static float GetCarPerformance01(TeamData t)
        {
            // Se os cards estiverem todos "default/zero" por algum motivo, cai pro LEGACY.
            bool cardsOk =
                t.Card_TopSpeed > 0 ||
                t.Card_Cornering > 0 ||
                t.Card_Acceleration > 0 ||
                t.Card_EnergyManagement > 0 ||
                t.Card_Reliability > 0;

            if (cardsOk)
            {
                float topSpeed    = t.Card_TopSpeed / 100f;
                float cornering   = t.Card_Cornering / 100f;
                float accel       = t.Card_Acceleration / 100f;
                float energy      = t.Card_EnergyManagement / 100f;
                float reliability = t.Card_Reliability / 100f;

                float perf =
                    (topSpeed    * 0.22f) +
                    (cornering   * 0.28f) +
                    (accel       * 0.22f) +
                    (energy      * 0.12f) +
                    (reliability * 0.16f);

                return Mathf.Clamp01(perf);
            }
            else
            {
                float aero = t.aero / 100f;
                float pu   = t.powerUnit / 100f;
                float ch   = t.chassis / 100f;
                float rel  = t.reliability / 100f;

                return Mathf.Clamp01((aero * 0.35f) + (pu * 0.35f) + (ch * 0.20f) + (rel * 0.10f));
            }
        }

        // =======================
        // RANDOM (Gaussian)
        // =======================

        private static float NextGaussian(System.Random rnd, float mean, float stdDev)
        {
            // Box-Muller
            double u1 = 1.0 - rnd.NextDouble();
            double u2 = 1.0 - rnd.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return (float)(mean + stdDev * randStdNormal);
        }
    }
}
