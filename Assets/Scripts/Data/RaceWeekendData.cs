using System;
using System.Collections.Generic;
using UnityEngine;

namespace F1Manager.Data
{
    // Tipos de sessão do fim de semana
    public enum WeekendSessionType
    {
        FP1 = 0,
        FP2 = 1,
        FP3 = 2,

        Qualifying = 10,
        Q1 = 11,
        Q2 = 12,
        Q3 = 13,

        SprintQualifying = 20,   // Sprint Shootout / Sprint Quali
        SQ1 = 21,
        SQ2 = 22,
        SQ3 = 23,

        Sprint = 30,
        Race = 40
    }

    // Um “slot” de sessão (para calendário/ordem).
    // Por enquanto a gente NÃO precisa de horário real (pode vir depois).
    [Serializable]
    public class WeekendSessionSlot
    {
        public WeekendSessionType type = WeekendSessionType.FP1;

        [Header("Optional Metadata (future use)")]
        [Min(1)] public int day = 1;          // 1=Fri, 2=Sat, 3=Sun (convenção simples)
        [Min(0)] public int orderInDay = 0;   // ordem do dia (0..N)

        [Tooltip("Se false, a sessão existe mas não será usada por simulação (ex: placeholder).")]
        public bool enabled = true;

        public WeekendSessionSlot() { }

        public WeekendSessionSlot(WeekendSessionType t, int d, int o, bool en = true)
        {
            type = t;
            day = Mathf.Max(1, d);
            orderInDay = Mathf.Max(0, o);
            enabled = en;
        }
    }

    [Serializable]
    public class RaceWeekendData
    {
        [Header("Order")]
        [Min(1)] public int round = 1;

        [Header("Track Reference")]
        public TrackData track;

        // =========================
        // SESSIONS (auto-generated)
        // =========================
        [Header("Weekend Sessions (Auto)")]
        [Tooltip("Esta lista é gerada automaticamente com base no TrackData.weekendFormat. " +
                 "Você pode visualizar/ajustar depois, mas o padrão vem do track.")]
        public List<WeekendSessionSlot> sessions = new List<WeekendSessionSlot>();

        // =========================
        // AUTO (vem 100% do TrackData)
        // =========================
        public bool HasTrack => track != null;

        public float RaceDistanceKm => track != null ? track.TotalRaceDistanceKm : 0f;

        public bool HasSprint => track != null && track.hasSprint;

        public int SprintLaps => track != null ? track.GetSprintLapsDefault() : 0;

        public string TrackId => track != null ? track.trackId : "";
        public string TrackName => track != null ? track.trackName : "(No Track)";
        public string Country => track != null ? track.country : "";
        public string City => track != null ? track.city : "";
        public TrackType TrackType => track != null ? track.type : TrackType.Permanent;

        public float LapLengthKm => track != null ? track.lapLengthKm : 0f;
        public int Laps => track != null ? track.laps : 0;

        public int Corners => track != null ? track.corners : 0;
        public int SlowCorners => track != null ? track.slowCorners : 0;
        public int MediumCorners => track != null ? track.mediumCorners : 0;
        public int FastCorners => track != null ? track.fastCorners : 0;
        public float LongestStraightMeters => track != null ? track.longestStraightMeters : 0f;

        public TireWearLevel TireWear => track != null ? track.tireWear : TireWearLevel.Medium;

        public int RainChance => track != null ? track.rainChance : 0;
        public int SafetyCarChance => track != null ? track.safetyCarChance : 0;
        public int VscChance => track != null ? track.vscChance : 0;
        public int RedFlagChance => track != null ? track.redFlagChance : 0;

        public float PitLaneTimeLossSec => track != null ? track.pitLaneTimeLossSec : 0f;
        public float PitStopBaseSec => track != null ? track.pitStopBaseSec : 0f;
        public float PitStopVarSec => track != null ? track.pitStopVarSec : 0f;

        public OvertakeAidType OvertakeAid => track != null ? track.overtakeAid : OvertakeAidType.None;
        public float OvertakingEase => track != null ? track.overtakingEase : 0f;
        public float OvertakeAidStrength => track != null ? track.overtakeAidStrength : 1f;
        public int OvertakeZones => track != null ? track.overtakeZones : 0;

        public int Difficulty => track != null ? track.difficulty : 1;

        // =========================
        // SESSION HELPERS
        // =========================

        public bool HasSession(WeekendSessionType type)
        {
            if (sessions == null) return false;
            for (int i = 0; i < sessions.Count; i++)
            {
                if (sessions[i] != null && sessions[i].enabled && sessions[i].type == type)
                    return true;
            }
            return false;
        }

        public IReadOnlyList<WeekendSessionSlot> SessionsOrdered
        {
            get
            {
                if (sessions == null) return Array.Empty<WeekendSessionSlot>();
                // ordena por dia e ordem
                sessions.Sort((a, b) =>
                {
                    if (a == null && b == null) return 0;
                    if (a == null) return 1;
                    if (b == null) return -1;

                    int d = a.day.CompareTo(b.day);
                    if (d != 0) return d;
                    return a.orderInDay.CompareTo(b.orderInDay);
                });
                return sessions;
            }
        }

        // =========================
        // SYNC (Track -> Weekend)
        // =========================

        public void SyncSessionsWithTrack(bool forceRebuild = false)
        {
            if (track == null)
            {
                if (sessions == null) sessions = new List<WeekendSessionSlot>();
                return;
            }

            // Se a lista está vazia ou se forceRebuild=true, recria padrão do track
            bool needsBuild = forceRebuild || sessions == null || sessions.Count == 0;

            // Se não está vazia, mas o formato do track mudou, também reconstruímos
            // (regra simples: Sprint deve ter FP1 + SQ* + Sprint + Q* + Race; Standard deve ter FP1 FP2 FP3 Q* Race)
            if (!needsBuild)
            {
                bool hasFP2 = HasSession(WeekendSessionType.FP2);
                bool hasFP3 = HasSession(WeekendSessionType.FP3);
                bool hasSprintSlot = HasSession(WeekendSessionType.Sprint);

                if (track.weekendFormat == WeekendFormat.Sprint)
                {
                    // Sprint: não pode ter FP2/FP3, deve ter Sprint
                    if (hasFP2 || hasFP3 || !hasSprintSlot) needsBuild = true;
                }
                else
                {
                    // Standard: deve ter FP2/FP3 e não deve ter Sprint
                    if (!hasFP2 || !hasFP3 || hasSprintSlot) needsBuild = true;
                }
            }

            if (!needsBuild) return;

            sessions = BuildDefaultSessionsForTrack(track);
        }

        private static List<WeekendSessionSlot> BuildDefaultSessionsForTrack(TrackData t)
        {
            var list = new List<WeekendSessionSlot>();

            if (t == null) return list;

            if (t.weekendFormat == WeekendFormat.Sprint)
            {
                // ✅ Sprint weekend (regra do seu jogo):
                // Day 1 (Fri): FP1, SprintQualifying (SQ1/SQ2/SQ3)
                // Day 2 (Sat): Sprint
                // Day 3 (Sun): Qualifying (Q1/Q2/Q3), Race
                list.Add(new WeekendSessionSlot(WeekendSessionType.FP1, 1, 0));
                list.Add(new WeekendSessionSlot(WeekendSessionType.SprintQualifying, 1, 1));
                list.Add(new WeekendSessionSlot(WeekendSessionType.SQ1, 1, 2));
                list.Add(new WeekendSessionSlot(WeekendSessionType.SQ2, 1, 3));
                list.Add(new WeekendSessionSlot(WeekendSessionType.SQ3, 1, 4));

                list.Add(new WeekendSessionSlot(WeekendSessionType.Sprint, 2, 0));

                list.Add(new WeekendSessionSlot(WeekendSessionType.Qualifying, 3, 0));
                list.Add(new WeekendSessionSlot(WeekendSessionType.Q1, 3, 1));
                list.Add(new WeekendSessionSlot(WeekendSessionType.Q2, 3, 2));
                list.Add(new WeekendSessionSlot(WeekendSessionType.Q3, 3, 3));

                list.Add(new WeekendSessionSlot(WeekendSessionType.Race, 3, 4));
            }
            else
            {
                // ✅ Standard weekend:
                // Day 1 (Fri): FP1, FP2
                // Day 2 (Sat): FP3, Qualifying (Q1/Q2/Q3)
                // Day 3 (Sun): Race
                list.Add(new WeekendSessionSlot(WeekendSessionType.FP1, 1, 0));
                list.Add(new WeekendSessionSlot(WeekendSessionType.FP2, 1, 1));

                list.Add(new WeekendSessionSlot(WeekendSessionType.FP3, 2, 0));
                list.Add(new WeekendSessionSlot(WeekendSessionType.Qualifying, 2, 1));
                list.Add(new WeekendSessionSlot(WeekendSessionType.Q1, 2, 2));
                list.Add(new WeekendSessionSlot(WeekendSessionType.Q2, 2, 3));
                list.Add(new WeekendSessionSlot(WeekendSessionType.Q3, 2, 4));

                list.Add(new WeekendSessionSlot(WeekendSessionType.Race, 3, 0));
            }

            return list;
        }

        public void Validate()
        {
            round = Mathf.Max(1, round);
            if (track != null) track.Validate();

            // Garante sessions coerentes com o track
            SyncSessionsWithTrack(forceRebuild: false);

            // Sanitiza sessions
            if (sessions == null) sessions = new List<WeekendSessionSlot>();
            for (int i = 0; i < sessions.Count; i++)
            {
                if (sessions[i] == null) continue;
                sessions[i].day = Mathf.Max(1, sessions[i].day);
                sessions[i].orderInDay = Mathf.Max(0, sessions[i].orderInDay);
            }
        }
    }
}
