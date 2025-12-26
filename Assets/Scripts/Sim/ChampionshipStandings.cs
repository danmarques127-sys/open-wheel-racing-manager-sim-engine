using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using F1Manager.Data;

namespace F1Manager.Sim
{
    public class ChampionshipStandings
    {
        public class DriverStanding
        {
            public DriverData driver;
            public string teamId;
            public int points;

            // Tie-breakers (F1 style)
            public int wins;
            public int p2;
            public int p3;
            public int p4;
            public int p5;
            public int p6;
            public int p7;
            public int p8;
            public int p9;
            public int p10;

            public int podiums => p1 + p2 + p3;
            public int p1 => wins;
        }

        public class TeamStanding
        {
            public string teamId;
            public TeamData team;
            public int points;
            public int wins;
        }

        private readonly Dictionary<string, DriverStanding> driverTable = new Dictionary<string, DriverStanding>();
        private readonly Dictionary<string, TeamStanding> teamTable = new Dictionary<string, TeamStanding>();

        public IReadOnlyList<DriverStanding> GetDriverStandingsSorted()
        {
            return driverTable.Values
                .OrderByDescending(s => s.points)
                .ThenByDescending(s => s.wins)
                .ThenByDescending(s => s.p2)
                .ThenByDescending(s => s.p3)
                .ThenByDescending(s => s.p4)
                .ThenByDescending(s => s.p5)
                .ThenByDescending(s => s.p6)
                .ThenByDescending(s => s.p7)
                .ThenByDescending(s => s.p8)
                .ThenByDescending(s => s.p9)
                .ThenByDescending(s => s.p10)
                .ThenBy(s => s.driver.displayName)
                .ToList();
        }

        public IReadOnlyList<TeamStanding> GetTeamStandingsSorted()
        {
            return teamTable.Values
                .OrderByDescending(s => s.points)
                .ThenByDescending(s => s.wins)
                .ThenBy(s => s.team != null ? s.team.displayName : s.teamId)
                .ToList();
        }

        public void EnsureDriver(DriverData d)
        {
            if (d == null) return;

            string id = d.driverId;
            if (string.IsNullOrWhiteSpace(id))
                id = d.name; // fallback

            if (driverTable.ContainsKey(id)) return;

            driverTable[id] = new DriverStanding
            {
                driver = d,
                teamId = d.contract != null ? d.contract.teamId : "",
                points = 0
            };
        }

        public void EnsureTeam(string teamId, TeamData teamData = null)
        {
            teamId = (teamId ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(teamId)) return;

            if (teamTable.ContainsKey(teamId)) return;

            teamTable[teamId] = new TeamStanding
            {
                teamId = teamId,
                team = teamData,
                points = 0
            };
        }

        public void AwardPointsToDriver(DriverData d, TeamData team, int pointsAwarded, int finishingPositionForTiebreak)
        {
            if (d == null) return;
            EnsureDriver(d);

            string id = string.IsNullOrWhiteSpace(d.driverId) ? d.name : d.driverId;
            var s = driverTable[id];

            s.points += Mathf.Max(0, pointsAwarded);

            // Tie-breakers: conta resultados (p1..p10)
            switch (finishingPositionForTiebreak)
            {
                case 1: s.wins++; break;
                case 2: s.p2++; break;
                case 3: s.p3++; break;
                case 4: s.p4++; break;
                case 5: s.p5++; break;
                case 6: s.p6++; break;
                case 7: s.p7++; break;
                case 8: s.p8++; break;
                case 9: s.p9++; break;
                case 10: s.p10++; break;
            }

            // Teams
            if (team != null)
            {
                EnsureTeam(team.teamId, team);
                var ts = teamTable[team.teamId.Trim().ToLowerInvariant()];
                ts.points += Mathf.Max(0, pointsAwarded);
                if (finishingPositionForTiebreak == 1) ts.wins++;
            }
        }
    }
}
