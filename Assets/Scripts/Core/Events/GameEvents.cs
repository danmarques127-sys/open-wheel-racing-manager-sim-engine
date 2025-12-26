using System;
using F1Manager.Core.Season;

namespace F1Manager.Core.Events
{
    public static class GameEvents
    {
        public static event Action<SeasonState> OnSeasonStarted;
        public static event Action<int> OnRoundStarted;       // round number
        public static event Action<int> OnRoundFinished;      // round number
        public static event Action<SeasonState> OnStandingsUpdated;
        public static event Action OnOffseasonStarted;
        public static event Action OnOffseasonFinished;

        public static void RaiseSeasonStarted(SeasonState s) => OnSeasonStarted?.Invoke(s);
        public static void RaiseRoundStarted(int round) => OnRoundStarted?.Invoke(round);
        public static void RaiseRoundFinished(int round) => OnRoundFinished?.Invoke(round);
        public static void RaiseStandingsUpdated(SeasonState s) => OnStandingsUpdated?.Invoke(s);
        public static void RaiseOffseasonStarted() => OnOffseasonStarted?.Invoke();
        public static void RaiseOffseasonFinished() => OnOffseasonFinished?.Invoke();
    }
}
