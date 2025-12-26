using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using F1Manager.Data;
using F1Manager.UI;

public class F1UIController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameManager gm;

    [Header("UI")]
    [SerializeField] private TMP_Text headerText;
    [SerializeField] private TMP_Text weekendText;
    [SerializeField] private TMP_Text gridText;
    [SerializeField] private TMP_Text finishText;
    [SerializeField] private TMP_Text standingsDriversText;
    [SerializeField] private TMP_Text standingsTeamsText;
    [SerializeField] private TMP_Text chartText;
    [SerializeField] private TMP_InputField seedInput;

    [Header("Buttons")]
    [SerializeField] private Button btnSimCurrent;
    [SerializeField] private Button btnSimNext;
    [SerializeField] private Button btnSimFull;
    [SerializeField] private Button btnReset;
    [SerializeField] private Button btnSave;
    [SerializeField] private Button btnLoad;

    private int currentRound = 1;
    private int lastSeed = 1234;

    // cache dos últimos resultados (pra UI)
    private List<RaceEntryResult> lastResults = new List<RaceEntryResult>();

    private void Awake()
    {
        if (gm == null) gm = FindFirstObjectByType<GameManager>();
    }

    private void Start()
    {
        WireButtons();
        RefreshAll();
    }

    private void WireButtons()
    {
        if (btnSimCurrent != null) btnSimCurrent.onClick.AddListener(() => SimulateRound(currentRound));
        if (btnSimNext != null) btnSimNext.onClick.AddListener(() =>
        {
            SimulateRound(currentRound);
            currentRound++;
            RefreshAll();
        });

        if (btnSimFull != null) btnSimFull.onClick.AddListener(() =>
        {
            int rounds = gm != null ? gm.GetTotalRoundsFallback() : 0;
            if (rounds <= 0) rounds = 24;

            for (int r = currentRound; r <= rounds; r++)
                SimulateRound(r);

            currentRound = rounds + 1;
            RefreshAll();
        });

        if (btnReset != null) btnReset.onClick.AddListener(() =>
        {
            currentRound = 1;
            lastResults.Clear();
            if (gm != null) gm.Debug_ResetSeasonRuntime(); // helper abaixo
            RefreshAll();
        });

        if (btnSave != null) btnSave.onClick.AddListener(() =>
        {
            if (gm != null) gm.Core_SaveNow();
            RefreshAll();
        });

        if (btnLoad != null) btnLoad.onClick.AddListener(() =>
        {
            if (gm != null) gm.Core_TryLoadGame();
            // se você quiser sincronizar round da UI com State.currentRound:
            int r = gm != null ? gm.GetCurrentRoundFallback() : 1;
            currentRound = Mathf.Max(1, r);
            RefreshAll();
        });
    }

    private int ReadSeed()
    {
        if (seedInput == null) return lastSeed;
        if (int.TryParse(seedInput.text, out int s))
        {
            lastSeed = s;
            return s;
        }
        return lastSeed;
    }

    private void SimulateRound(int round)
    {
        if (gm == null)
        {
            Debug.LogError("[UI] GameManager not found.");
            return;
        }

        int seed = ReadSeed();

        // ✅ precisa existir no seu GameManager:
        // public List<RaceEntryResult> SimulateRoundAndReturnResults(int round, int seed = 0)
        lastResults = gm.SimulateRoundAndReturnResults(round, seed) ?? new List<RaceEntryResult>();

        RefreshAll();
    }

    private void RefreshAll()
    {
        if (gm == null)
        {
            SafeSet(headerText, "No GameManager found.");
            return;
        }

        // Header
        SafeSet(headerText,
            $"Season: {gm.currentSeason}   Round(UI): {currentRound}   Seed: {lastSeed}");

        // Weekend info (do calendário)
        var weekend = gm.GetWeekendByRoundFallback(currentRound);
        if (weekend != null)
        {
            SafeSet(weekendText,
                F1UITable.Table("WEEKEND",
                    $"Track: {weekend.TrackName} ({weekend.Country})",
                    $"Format: {(weekend.HasSprint ? "SPRINT" : "STANDARD")}",
                    $"Distance: {weekend.RaceDistanceKm:0.0} km  | Laps: {weekend.Laps}  | Lap: {weekend.LapLengthKm:0.00} km"
                )
            );
        }
        else
        {
            SafeSet(weekendText, "WEEKEND\n(no weekend found for this round)");
        }

        // Grid/Finish (último resultado)
        SafeSet(gridText, BuildGridTable(lastResults));
        SafeSet(finishText, BuildFinishTable(lastResults));

        // Standings
        var drivers = gm.GetDriverStandingsTopN(10);
        SafeSet(standingsDriversText, BuildStandingsDrivers(drivers));

        var teams = gm.GetTeamStandingsAll();
        SafeSet(standingsTeamsText, BuildStandingsTeams(teams));

        // “Gráfico” ASCII (top 10 drivers points)
        if (chartText != null)
        {
            var chartData = drivers.Select(d => (name: ShortName(d.name), value: d.points));
            SafeSet(chartText, F1UIGraphText.BarChart("POINTS (Top 10)", chartData, 10, 22));
        }
    }

    private string BuildGridTable(List<RaceEntryResult> results)
    {
        if (results == null || results.Count == 0)
            return "GRID\n(no results yet)";

        var ordered = results.OrderBy(r => r.gridPos).Take(20).ToList();
        var lines = new List<string>();

        lines.Add(F1UITable.Row(("P", 3), ("Driver", 18), ("Team", 16)));
        lines.Add(F1UITable.Divider(45));

        foreach (var r in ordered)
        {
            lines.Add(F1UITable.Row(
                ($"{r.gridPos:00}", 3),
                (ShortName(r.driverName), 18),
                (ShortName(r.teamName), 16)
            ));
        }

        return F1UITable.Table("GRID (Top 20)", lines.ToArray());
    }

    private string BuildFinishTable(List<RaceEntryResult> results)
    {
        if (results == null || results.Count == 0)
            return "RACE\n(no results yet)";

        var ordered = results.OrderBy(r => r.finishPos).Take(20).ToList();
        var lines = new List<string>();

        lines.Add(F1UITable.Row(("P", 3), ("Driver", 18), ("Team", 16), ("DNF", 3)));
        lines.Add(F1UITable.Divider(55));

        foreach (var r in ordered)
        {
            lines.Add(F1UITable.Row(
                ($"{r.finishPos:00}", 3),
                (ShortName(r.driverName), 18),
                (ShortName(r.teamName), 16),
                (r.dnf ? "Y" : "", 3)
            ));
        }

        return F1UITable.Table("RACE (Top 20)", lines.ToArray());
    }

    private string BuildStandingsDrivers(List<DriverStandingUI> ds)
    {
        if (ds == null || ds.Count == 0) return "DRIVERS STANDINGS\n(no data)";

        var lines = new List<string>();
        lines.Add(F1UITable.Row(("#", 3), ("Driver", 18), ("Pts", 5), ("W", 3)));
        lines.Add(F1UITable.Divider(45));

        for (int i = 0; i < ds.Count; i++)
        {
            lines.Add(F1UITable.Row(
                ($"{i + 1:00}", 3),
                (ShortName(ds[i].name), 18),
                ($"{ds[i].points}", 5),
                ($"{ds[i].wins}", 3)
            ));
        }

        return F1UITable.Table("DRIVERS STANDINGS (Top 10)", lines.ToArray());
    }

    private string BuildStandingsTeams(List<TeamStandingUI> ts)
    {
        if (ts == null || ts.Count == 0) return "TEAMS STANDINGS\n(no data)";

        var lines = new List<string>();
        lines.Add(F1UITable.Row(("#", 3), ("Team", 18), ("Pts", 5), ("W", 3)));
        lines.Add(F1UITable.Divider(45));

        for (int i = 0; i < ts.Count; i++)
        {
            lines.Add(F1UITable.Row(
                ($"{i + 1:00}", 3),
                (ShortName(ts[i].name), 18),
                ($"{ts[i].points}", 5),
                ($"{ts[i].wins}", 3)
            ));
        }

        return F1UITable.Table("TEAMS STANDINGS", lines.ToArray());
    }

    private static void SafeSet(TMP_Text t, string value)
    {
        if (t != null) t.text = value ?? "";
    }

    private static string ShortName(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        if (s.Length <= 16) return s;
        return s.Substring(0, 16);
    }
}
