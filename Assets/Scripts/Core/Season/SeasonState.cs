using System;
using System.Collections.Generic;

namespace F1Manager.Core.Season
{
    [Serializable]
    public class SeasonState
    {
        public int seasonYear;
        public int currentRound = 1;
        public bool seasonRunning = false;

        // Seed do "mundo" nesse save
        public int worldSeed;

        // Dias corridos
        public SeasonClock clock = new SeasonClock();

        // Calendário gerado (IDs das pistas + flags)
        public List<RoundEntry> calendar = new List<RoundEntry>();

        // Resultados por round (stub expandível)
        public List<RoundResult> results = new List<RoundResult>();

        // Standings (stub simples: pontos por driver/team id)
        public Dictionary<string, int> driverPoints = new Dictionary<string, int>();
        public Dictionary<string, int> teamPoints = new Dictionary<string, int>();

        public int TotalRounds => calendar?.Count ?? 0;

        public RoundEntry? GetCurrentRoundEntry()
        {
            if (calendar == null || calendar.Count == 0) return null;
            int idx = currentRound - 1;
            if (idx < 0 || idx >= calendar.Count) return null;
            return calendar[idx];
        }

        public bool IsSeasonFinished()
        {
            return TotalRounds > 0 && currentRound > TotalRounds;
        }
    }

    [Serializable]
    public struct RoundEntry
    {
        public int round;
        public string trackId;
        public int startDay;        // dia do início do fim de semana
        public bool hasSprint;
    }

    [Serializable]
    public class RoundResult
    {
        public int round;
        public string trackId;

        // placeholders: você liga isso ao seu SimpleRaceSimulator depois
        public List<string> qualiOrderDriverIds = new List<string>();
        public List<string> sprintOrderDriverIds = new List<string>();
        public List<string> raceOrderDriverIds = new List<string>();
    }
}
