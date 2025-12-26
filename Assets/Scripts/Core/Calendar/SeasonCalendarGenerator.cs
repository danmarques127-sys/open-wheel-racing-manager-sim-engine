using System.Collections.Generic;
using System.Linq;
using F1Manager.Core.Season;
using F1Manager.Core.World;
using F1Manager.Data;

namespace F1Manager.Core.Calendar
{
    public static class SeasonCalendarGenerator
    {
        // selectedTracks: se null/empty -> usa TrackPool
        public static List<RoundEntry> Generate(
            int seasonYear,
            int worldSeed,
            CalendarFormatRules rules,
            IEnumerable<TrackData> poolTracks,
            int roundsWanted,
            IEnumerable<TrackData> selectedTracks = null)
        {
            var rng = new DeterministicRng(RngStreams.Derive(worldSeed, $"calendar_{seasonYear}"));

            var source = (selectedTracks != null && selectedTracks.Any())
                ? selectedTracks.ToList()
                : poolTracks.ToList();

            // Garantir roundsWanted válido
            if (rules != null && rules.allowedRounds != null && rules.allowedRounds.Length > 0)
            {
                if (!rules.allowedRounds.Contains(roundsWanted))
                    roundsWanted = rules.allowedRounds[0];
            }

            // Se não tem pistas suficientes, repete? (eu recomendo NÃO repetir no core)
            // Aqui vamos limitar ao máximo possível sem repetir.
            if (roundsWanted > source.Count) roundsWanted = source.Count;

            // Shuffle e pega N
            rng.Shuffle(source);
            var chosen = source.Take(roundsWanted).ToList();

            // Monta rounds com espaçamento
            var calendar = new List<RoundEntry>(roundsWanted);
            int dayCursor = 1;

            for (int i = 0; i < chosen.Count; i++)
            {
                var track = chosen[i];
                int gap = rng.NextInt(rules.minDaysBetweenRounds, rules.maxDaysBetweenRounds + 1);

                if (i == 0) dayCursor = 1;
                else dayCursor += gap;

                bool hasSprint = false;
                if (rules.allowSprint)
                {
                    // Se você já tiver "isSprintTrack" no TrackData, você pode priorizar aqui.
                    // Por enquanto: chance.
                    hasSprint = rng.Next01() < rules.sprintChance;
                }

                calendar.Add(new RoundEntry
                {
                    round = i + 1,
                    trackId = track.trackId,
                    startDay = dayCursor,
                    hasSprint = hasSprint
                });
            }

            return calendar;
        }
    }
}
