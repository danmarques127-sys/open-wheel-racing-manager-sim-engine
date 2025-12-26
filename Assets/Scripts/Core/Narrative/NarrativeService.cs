using System;
using System.Collections.Generic;
using F1Manager.Core.Randomness;

namespace F1Manager.Core.Narrative
{
    public sealed class NarrativeService
    {
        private readonly NarrativeLibrary _lib;

        public NarrativeService(NarrativeLibrary lib)
        {
            _lib = lib;
        }

        public List<string> GenerateNews(RandomStream rng, NewsCategory category, RaceContext race, StatsContext stats)
        {
            return category switch
            {
                NewsCategory.PreRace => GeneratePreRace(rng, race, stats),
                NewsCategory.PostRace => GeneratePostRace(rng, race, stats),
                NewsCategory.Market => GenerateMarket(rng, race, stats),
                NewsCategory.Regulations => GenerateRegulations(rng, race, stats),
                NewsCategory.Gossip => GenerateGossip(rng, race, stats),
                _ => new List<string>()
            };
        }

        private List<string> GeneratePreRace(RandomStream rng, RaceContext race, StatsContext stats)
        {
            var outList = new List<string>();

            string mood = stats.titleFightTight ? Pick(rng, _lib.introsTense) : Pick(rng, _lib.introsNormal);
            string hook = race.isSprintWeekend ? Pick(rng, _lib.sprintHooks) : Pick(rng, _lib.standardHooks);

            string line = $"{mood} {hook}";
            outList.Add(ReplaceVars(line, race, stats));

            if (stats.teamInCrisis || stats.budgetTrouble)
                outList.Add(ReplaceVars(Pick(rng, _lib.preRaceCrisis), race, stats));

            return outList;
        }

        private List<string> GeneratePostRace(RandomStream rng, RaceContext race, StatsContext stats)
        {
            var outList = new List<string>();

            string headlineKey = PickHeadlineKey(rng, race, stats);
            if (!_lib.TryGetHeadline(headlineKey, out string headlineTpl) || string.IsNullOrWhiteSpace(headlineTpl))
            {
                // fallback
                _lib.TryGetHeadline("STANDARD", out headlineTpl);
                headlineTpl ??= "{winner} vence em {track}.";
            }

            outList.Add(ReplaceVars(headlineTpl, race, stats));

            string recapTpl = Pick(rng, _lib.postRaceRecaps);
            outList.Add(ReplaceVars(recapTpl, race, stats));

            if (race.weather == "Rainy" || race.weather == "Mixed")
                outList.Add(ReplaceVars(Pick(rng, _lib.weatherLines), race, stats));

            if (race.safetyCars > 0 || race.redFlag)
                outList.Add(ReplaceVars(Pick(rng, _lib.safetyLines), race, stats));

            if (race.wasUpset)
                outList.Add(ReplaceVars(Pick(rng, _lib.upsetLines), race, stats));

            return outList;
        }

        private List<string> GenerateMarket(RandomStream rng, RaceContext race, StatsContext stats)
        {
            var outList = new List<string>();

            if (stats.internalTension)
                outList.Add(ReplaceVars(Pick(rng, _lib.marketTensionRumors), race, stats));
            else
                outList.Add(ReplaceVars(Pick(rng, _lib.marketGenericRumors), race, stats));

            return outList;
        }

        private List<string> GenerateRegulations(RandomStream rng, RaceContext race, StatsContext stats)
        {
            var outList = new List<string>();
            outList.Add(ReplaceVars(Pick(rng, _lib.regulationLines), race, stats));
            return outList;
        }

        private List<string> GenerateGossip(RandomStream rng, RaceContext race, StatsContext stats)
        {
            var outList = new List<string>();

            if (stats.internalTension)
                outList.Add(ReplaceVars(Pick(rng, _lib.gossipTension), race, stats));
            else
                outList.Add(ReplaceVars(Pick(rng, _lib.gossipLight), race, stats));

            return outList;
        }

        private string PickHeadlineKey(RandomStream rng, RaceContext race, StatsContext stats)
        {
            if (race.wasUpset) return "UPSET";
            if (race.winnerGrid >= 6) return "FROM_BEHIND";
            if (race.weather == "Rainy" || race.weather == "Mixed") return "RAIN";
            if (race.safetyCars >= 2 || race.redFlag) return "CHAOS";
            if (stats.winnerWinStreak >= 3) return "STREAK";
            return "STANDARD";
        }

        private static string ReplaceVars(string s, RaceContext r, StatsContext st)
        {
            return (s ?? string.Empty)
                .Replace("{season}", r.season.ToString())
                .Replace("{round}", r.round.ToString())
                .Replace("{track}", r.trackName ?? "a pista")
                .Replace("{winner}", r.winnerDriver ?? "o vencedor")
                .Replace("{winnerTeam}", r.winnerTeam ?? "a equipe")
                .Replace("{grid}", r.winnerGrid.ToString())
                .Replace("{streak}", st.winnerWinStreak.ToString())
                .Replace("{rival}", string.IsNullOrWhiteSpace(st.rivalDriver) ? "um rival" : st.rivalDriver);
        }

        private static string Pick(RandomStream rng, List<string> list)
        {
            if (list == null || list.Count == 0) return string.Empty;
            return rng.Pick(list);
        }
    }
}
