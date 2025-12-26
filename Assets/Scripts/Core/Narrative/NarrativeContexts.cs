using System;

namespace F1Manager.Core.Narrative
{
    [Serializable]
    public class RaceContext
    {
        public int season;
        public int round;
        public string trackName;

        public bool isSprintWeekend;

        // "Sunny", "Rainy", "Mixed"
        public string weather;

        public int safetyCars;
        public int retirements;
        public bool bigCrash;
        public bool redFlag;

        public string winnerDriver;
        public string winnerTeam;

        public int winnerGrid;
        public bool wasUpset;
        public string upsetVictimTeam;

        public string headlineHook;
    }

    [Serializable]
    public class StatsContext
    {
        public int winnerWinStreak;
        public int teamWinStreak;

        public bool titleFightTight;
        public string rivalDriver;

        public bool teamInCrisis;
        public bool budgetTrouble;
        public bool internalTension;
    }

    public enum NewsCategory
    {
        PreRace,
        PostRace,
        Market,
        Regulations,
        Gossip
    }
}
