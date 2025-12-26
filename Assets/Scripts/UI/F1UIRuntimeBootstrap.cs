using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class F1UIRuntimeBootstrap : MonoBehaviour
{
    [Header("Hook")]
    [SerializeField] private GameManager gm;

    [Header("Runtime UI refs (read-only)")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform panel;

    [SerializeField] private TMP_Text headerText;
    [SerializeField] private TMP_Text weekendText;

    [SerializeField] private TMP_InputField seedInput;

    [SerializeField] private Button btnSimCurrent;
    [SerializeField] private Button btnSimNext;
    [SerializeField] private Button btnSimFull;
    [SerializeField] private Button btnReset;

    // NEW: session tabs
    private readonly Dictionary<string, Button> sessionButtons = new Dictionary<string, Button>();
    private string selectedSessionKey = "RACE";

    // NEW: table (scroll)
    private RectTransform tableContainer;
    private ScrollRect tableScroll;
    private RectTransform tableContent;
    private TMP_Text tableTitle;

    // NEW: standings text (keep)
    [SerializeField] private TMP_Text standingsDriversText;
    [SerializeField] private TMP_Text standingsTeamsText;

    // NEW: chart
    private RectTransform chartContainer;
    private TMP_Text chartTitle;
    private readonly List<int> chartSeries = new List<int>(); // points over rounds (runtime)
    private readonly List<GameObject> chartObjects = new List<GameObject>();

    // ---------- runtime state ----------
    private int roundLocal = 1;
    private int totalRoundsLocal = 0;

    // last sim cache
    private object lastWeekendResultObj = null; // SimpleRaceSimulator.WeekendResult (via reflection-safe)
    private List<RaceEntryResult> lastRaceEntryResults = null;

    private void Awake()
    {
        if (gm == null)
            gm = UnityEngine.Object.FindFirstObjectByType<GameManager>();

        BuildUI();
        WireButtons();

        RebuildRoundCacheFromGM();
        RefreshAll();
    }

    private void RebuildRoundCacheFromGM()
    {
        if (gm == null)
        {
            roundLocal = 1;
            totalRoundsLocal = 0;
            return;
        }

        totalRoundsLocal = Mathf.Max(0, gm.GetTotalRoundsFallback());
        roundLocal = Mathf.Clamp(gm.GetCurrentRoundFallback(), 1, Mathf.Max(1, totalRoundsLocal));
    }

    // ===========================
    // UI BUILD
    // ===========================

    private void BuildUI()
    {
        var existing = GameObject.Find("Canvas_Runtime");
        if (existing != null)
            Destroy(existing);

        var canvasGO = new GameObject("Canvas_Runtime", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            new GameObject(
                "EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule)
            );
        }

        var panelGO = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panelGO.transform.SetParent(canvasGO.transform, false);
        panel = panelGO.GetComponent<RectTransform>();

        var img = panelGO.GetComponent<Image>();
        img.color = new Color(0.07f, 0.09f, 0.11f, 0.94f);

        StretchToFull(panel);

        float pad = 24f;
        float topBarH = 120f;
        float btnH = 44f;
        float colGap = 24f;

        // Header
        headerText = CreateTMP(panel, "HeaderText", "F1 Manager", 36, TextAlignmentOptions.Left);
        AnchorTopLeft(headerText.rectTransform, pad, -pad, 1400, 48);

        weekendText = CreateTMP(panel, "WeekendText", "Round: - | Track: -", 20, TextAlignmentOptions.Left);
        AnchorTopLeft(weekendText.rectTransform, pad, -(pad + 52), 1400, 32);

        // Seed input (top right)
        seedInput = CreateTMPInput(panel, "SeedInput");
        AnchorTopRight(seedInput.GetComponent<RectTransform>(), pad, -pad, 260, 44);
        seedInput.text = "";

        var seedLabel = CreateTMP(panel, "SeedLabel", "Seed", 18, TextAlignmentOptions.Right);
        AnchorTopRight(seedLabel.rectTransform, pad + 270, -(pad + 6), 120, 28);

        // Buttons row
        btnSimCurrent = CreateButton(panel, "BtnSimCurrent", "Sim Current");
        btnSimNext = CreateButton(panel, "BtnSimNext", "Sim Next");
        btnSimFull = CreateButton(panel, "BtnSimFull", "Sim Full");
        btnReset = CreateButton(panel, "BtnReset", "Reset");

        float btnY = -(topBarH);
        float btnW = 160f;
        float startX = pad;

        AnchorTopLeft(btnSimCurrent.GetComponent<RectTransform>(), startX, btnY, btnW, btnH);
        AnchorTopLeft(btnSimNext.GetComponent<RectTransform>(), startX + (btnW + 12), btnY, btnW, btnH);
        AnchorTopLeft(btnSimFull.GetComponent<RectTransform>(), startX + 2 * (btnW + 12), btnY, btnW, btnH);
        AnchorTopLeft(btnReset.GetComponent<RectTransform>(), startX + 3 * (btnW + 12), btnY, btnW, btnH);

        // Session Tabs (right of buttons)
        CreateSessionTabs(panel, pad + 4 * (btnW + 12) + 18, btnY, 900, btnH);

        // Layout
        float contentTop = topBarH + btnH + 18f;
        float contentH = 1080f - contentTop - pad;

        float leftW = (1920f - pad * 2 - colGap) * 0.60f;
        float rightW = (1920f - pad * 2 - colGap) - leftW;

        // Left: Table + Chart
        CreateTable(panel, pad, -contentTop, leftW, contentH * 0.65f);
        CreateChart(panel, pad, -(contentTop + contentH * 0.65f + 12f), leftW, contentH * 0.35f - 12f);

        // Right: Standings
        standingsDriversText = CreateTMP(panel, "StandingsDriversText", "DRIVER STANDINGS\n-", 18, TextAlignmentOptions.TopLeft);
        AnchorTopLeft(standingsDriversText.rectTransform, pad + leftW + colGap, -contentTop, rightW, contentH * 0.50f);

        standingsTeamsText = CreateTMP(panel, "StandingsTeamsText", "TEAM STANDINGS\n-", 18, TextAlignmentOptions.TopLeft);
        AnchorTopLeft(standingsTeamsText.rectTransform, pad + leftW + colGap, -(contentTop + contentH * 0.50f + 12f), rightW, contentH * 0.50f - 12f);
    }

    private void CreateSessionTabs(RectTransform parent, float x, float y, float w, float h)
    {
        // container
        var tabsGO = new GameObject("SessionTabs", typeof(RectTransform));
        tabsGO.transform.SetParent(parent, false);
        var rt = tabsGO.GetComponent<RectTransform>();
        AnchorTopLeft(rt, x, y, w, h);

        string[] keys = new[] { "TL1", "TL2", "TL3", "QUALI", "SPRINT", "RACE" };
        float gap = 10f;
        float btnW = (w - gap * (keys.Length - 1)) / keys.Length;

        sessionButtons.Clear();

        for (int i = 0; i < keys.Length; i++)
        {
            var b = CreateButton(rt, "Tab_" + keys[i], keys[i]);
            var brt = b.GetComponent<RectTransform>();
            AnchorTopLeft(brt, i * (btnW + gap), 0, btnW, h);

            string key = keys[i];
            b.onClick.AddListener(() =>
            {
                selectedSessionKey = key;
                RefreshTableAndTabs();
            });

            sessionButtons[key] = b;
        }

        selectedSessionKey = "RACE";
    }

    private void CreateTable(RectTransform parent, float x, float y, float w, float h)
    {
        var containerGO = new GameObject("TableContainer", typeof(RectTransform), typeof(Image));
        containerGO.transform.SetParent(parent, false);
        tableContainer = containerGO.GetComponent<RectTransform>();
        AnchorTopLeft(tableContainer, x, y, w, h);

        containerGO.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.06f);

        tableTitle = CreateTMP(tableContainer, "TableTitle", "SESSION", 20, TextAlignmentOptions.Left);
        AnchorTopLeft(tableTitle.rectTransform, 16, -12, w - 32, 28);

        // ScrollRect
        var scrollGO = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollGO.transform.SetParent(tableContainer, false);
        var scrollRT = scrollGO.GetComponent<RectTransform>();
        AnchorTopLeft(scrollRT, 12, -48, w - 24, h - 60);

        scrollGO.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.20f);

        tableScroll = scrollGO.GetComponent<ScrollRect>();
        tableScroll.horizontal = false;

        // Viewport
        var viewportGO = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewportGO.transform.SetParent(scrollGO.transform, false);
        var viewportRT = viewportGO.GetComponent<RectTransform>();
        StretchToFull(viewportRT);
        viewportGO.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.02f);
        viewportGO.GetComponent<Mask>().showMaskGraphic = false;

        // Content
        var contentGO = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentGO.transform.SetParent(viewportGO.transform, false);
        tableContent = contentGO.GetComponent<RectTransform>();
        tableContent.anchorMin = new Vector2(0, 1);
        tableContent.anchorMax = new Vector2(1, 1);
        tableContent.pivot = new Vector2(0.5f, 1f);
        tableContent.anchoredPosition = Vector2.zero;
        tableContent.sizeDelta = new Vector2(0, 0);

        var vlg = contentGO.GetComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(8, 8, 8, 8);
        vlg.spacing = 6;
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.childControlHeight = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childForceExpandWidth = true;

        var fitter = contentGO.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        tableScroll.viewport = viewportRT;
        tableScroll.content = tableContent;
    }

    private void CreateChart(RectTransform parent, float x, float y, float w, float h)
    {
        var containerGO = new GameObject("ChartContainer", typeof(RectTransform), typeof(Image));
        containerGO.transform.SetParent(parent, false);
        chartContainer = containerGO.GetComponent<RectTransform>();
        AnchorTopLeft(chartContainer, x, y, w, h);

        containerGO.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.06f);

        chartTitle = CreateTMP(chartContainer, "ChartTitle", "POINTS TREND (runtime)", 20, TextAlignmentOptions.Left);
        AnchorTopLeft(chartTitle.rectTransform, 16, -12, w - 32, 28);

        // Plot area
        var plotGO = new GameObject("Plot", typeof(RectTransform), typeof(Image));
        plotGO.transform.SetParent(chartContainer, false);
        var plotRT = plotGO.GetComponent<RectTransform>();
        AnchorTopLeft(plotRT, 12, -48, w - 24, h - 60);
        plotGO.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.20f);
    }

    // ===========================
    // BUTTONS
    // ===========================

    private void WireButtons()
    {
        if (btnSimCurrent != null) btnSimCurrent.onClick.RemoveAllListeners();
        if (btnSimNext != null) btnSimNext.onClick.RemoveAllListeners();
        if (btnSimFull != null) btnSimFull.onClick.RemoveAllListeners();
        if (btnReset != null) btnReset.onClick.RemoveAllListeners();

        btnSimCurrent.onClick.AddListener(() =>
        {
            int seed = GetSeedOrZero();
            var sim = SimulateRound(roundLocal, seed);
            ApplySimCache(sim);
            RefreshAll();
        });

        btnSimNext.onClick.AddListener(() =>
        {
            int seed = GetSeedOrZero();
            roundLocal = Mathf.Clamp(roundLocal + 1, 1, Mathf.Max(1, totalRoundsLocal));
            var sim = SimulateRound(roundLocal, seed);
            ApplySimCache(sim);
            RefreshAll();
        });

        btnSimFull.onClick.AddListener(() =>
        {
            int seed = GetSeedOrZero();

            if (gm == null)
            {
                Debug.LogWarning("F1UIRuntimeBootstrap: GameManager is null.");
                return;
            }

            if (totalRoundsLocal <= 0)
            {
                Debug.LogWarning("F1UIRuntimeBootstrap: totalRoundsLocal is 0. Assign calendar/tracks.");
                return;
            }

            object last = null;
            for (int r = roundLocal; r <= totalRoundsLocal; r++)
                last = SimulateRound(r, seed);

            roundLocal = totalRoundsLocal;
            ApplySimCache(last);
            RefreshAll();
        });

        btnReset.onClick.AddListener(() =>
        {
            if (gm != null)
                gm.Debug_ResetSeasonRuntime();

            chartSeries.Clear();
            ClearChartObjects();

            RebuildRoundCacheFromGM();
            roundLocal = 1;
            lastWeekendResultObj = null;
            lastRaceEntryResults = null;

            RefreshAll();
        });
    }

    private int GetSeedOrZero()
    {
        if (seedInput == null) return 0;
        if (int.TryParse(seedInput.text, out int seed)) return seed;
        return 0;
    }

    // ===========================
    // SIM CALLS
    // ===========================

    /// <summary>
    /// Returns either:
    /// - WeekendResult object (if gm has method SimulateRoundAndReturnWeekend)
    /// - Or List<RaceEntryResult> (fallback old method)
    /// </summary>
    private object SimulateRound(int round, int seed)
    {
        if (gm == null)
        {
            Debug.LogWarning("F1UIRuntimeBootstrap: gm is null.");
            return null;
        }

        // Try new method: SimulateRoundAndReturnWeekend(int round, int seed)
        var t = gm.GetType();
        var mi = t.GetMethod("SimulateRoundAndReturnWeekend",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic);

        if (mi != null)
        {
            try
            {
                return mi.Invoke(gm, new object[] { round, seed });
            }
            catch (Exception ex)
            {
                Debug.LogWarning("F1UIRuntimeBootstrap: SimulateRoundAndReturnWeekend invoke failed: " + ex.Message);
            }
        }

        // fallback old: List<RaceEntryResult> SimulateRoundAndReturnResults(int round, int seed)
        return gm.SimulateRoundAndReturnResults(round, seed);
    }

    private void ApplySimCache(object sim)
    {
        lastWeekendResultObj = null;
        lastRaceEntryResults = null;

        if (sim == null) return;

        // WeekendResult: has fields "sessions" and "entries"
        if (HasMember(sim, "sessions") && HasMember(sim, "entries"))
        {
            lastWeekendResultObj = sim;
            // also build legacy race results list for safety
            lastRaceEntryResults = BuildLegacyRaceEntryResultsFromWeekend(sim);
            PushChartPointFromStandingsLeader();
            return;
        }

        // Legacy: List<RaceEntryResult>
        if (sim is List<RaceEntryResult> legacy)
        {
            lastRaceEntryResults = legacy;
            PushChartPointFromStandingsLeader();
            return;
        }
    }

    // ===========================
    // REFRESH
    // ===========================

    private void RefreshAll()
    {
        if (headerText == null || weekendText == null ||
            standingsDriversText == null || standingsTeamsText == null)
        {
            Debug.LogWarning("F1UIRuntimeBootstrap: UI refs are null (BuildUI failed).");
            return;
        }

        if (gm == null)
        {
            headerText.text = "F1 Manager (GameManager not found)";
            weekendText.text = "Round: - | Track: -";
            standingsDriversText.text = "DRIVER STANDINGS\n-";
            standingsTeamsText.text = "TEAM STANDINGS\n-";
            return;
        }

        if (totalRoundsLocal <= 0)
            totalRoundsLocal = Mathf.Max(0, gm.GetTotalRoundsFallback());

        headerText.text = $"F1 Manager — Season {gm.currentSeason}   |   Round {roundLocal}/{Mathf.Max(1, totalRoundsLocal)}";

        var weekend = gm.GetWeekendByRoundFallback(roundLocal);
        if (weekend != null && weekend.track != null)
        {
            string trackName =
                !string.IsNullOrWhiteSpace(weekend.track.trackName) ? weekend.track.trackName :
                !string.IsNullOrWhiteSpace(weekend.track.name) ? weekend.track.name :
                !string.IsNullOrWhiteSpace(weekend.track.trackId) ? weekend.track.trackId :
                "Unknown Track";

            string fmt = weekend.HasSprint ? "SPRINT" : "STANDARD";
            weekendText.text = $"Round {roundLocal} | Track: {trackName} | Format: {fmt}";
        }
        else
        {
            weekendText.text = $"Round {roundLocal} | Track: (missing calendar/track)";
        }

        RefreshTableAndTabs();

        standingsDriversText.text = BuildDriverStandingsText();
        standingsTeamsText.text = BuildTeamStandingsText();

        RedrawChart();
    }

    private void RefreshTableAndTabs()
    {
        // update tab visuals
        foreach (var kv in sessionButtons)
        {
            var img = kv.Value.GetComponent<Image>();
            if (img == null) continue;
            bool active = kv.Key == selectedSessionKey;
            img.color = active ? new Color(1f, 1f, 1f, 0.95f) : new Color(1f, 1f, 1f, 0.65f);
        }

        // sprint visibility: if no sprint, grey out Sprint tab
        bool hasSprint = false;
        var weekend = gm != null ? gm.GetWeekendByRoundFallback(roundLocal) : null;
        if (weekend != null) hasSprint = weekend.HasSprint;

        if (sessionButtons.TryGetValue("SPRINT", out var sprintBtn))
        {
            sprintBtn.interactable = hasSprint;
            var img = sprintBtn.GetComponent<Image>();
            if (img != null) img.color = hasSprint ? img.color : new Color(1f, 1f, 1f, 0.25f);
            if (!hasSprint && selectedSessionKey == "SPRINT")
                selectedSessionKey = "RACE";
        }

        // build rows
        ClearTableRows();

        var rows = GetSessionRows(selectedSessionKey);

        tableTitle.text = $"SESSION — {selectedSessionKey}";

        if (rows == null || rows.Count == 0)
        {
            AddTableRow("—", "No data", "");
            return;
        }

        // header row
        AddTableRow("#", "Driver", "Team", header: true);

        foreach (var r in rows)
        {
            AddTableRow(r.posStr, r.driver, r.team);
        }
    }

    private void ClearTableRows()
    {
        if (tableContent == null) return;
        for (int i = tableContent.childCount - 1; i >= 0; i--)
            Destroy(tableContent.GetChild(i).gameObject);
    }

    private void AddTableRow(string col1, string col2, string col3, bool header = false)
    {
        if (tableContent == null) return;

        var rowGO = new GameObject("Row", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
        rowGO.transform.SetParent(tableContent, false);

        var img = rowGO.GetComponent<Image>();
        img.color = header ? new Color(1f, 1f, 1f, 0.10f) : new Color(1f, 1f, 1f, 0.05f);

        var hlg = rowGO.GetComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(8, 8, 6, 6);
        hlg.spacing = 10;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlHeight = true;
        hlg.childControlWidth = true;
        hlg.childForceExpandHeight = false;
        hlg.childForceExpandWidth = true;

        var rt = rowGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, header ? 36 : 34);

        CreateRowCell(rowGO.transform, col1, header ? 18 : 16, 0.12f, header);
        CreateRowCell(rowGO.transform, col2, header ? 18 : 16, 0.48f, header);
        CreateRowCell(rowGO.transform, col3, header ? 18 : 16, 0.40f, header);
    }

    private void CreateRowCell(Transform parent, string text, float fontSize, float widthPct, bool header)
    {
        var go = new GameObject("Cell", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        go.transform.SetParent(parent, false);

        var le = go.GetComponent<LayoutElement>();
        le.flexibleWidth = Mathf.Max(1f, widthPct * 100f);

        var t = go.GetComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = fontSize;
        t.color = header ? new Color(1f, 1f, 1f, 0.95f) : new Color(1f, 1f, 1f, 0.90f);
        t.alignment = TextAlignmentOptions.Left;
        t.textWrappingMode = TextWrappingModes.NoWrap;
    }

    private void ClearChartObjects()
    {
        foreach (var go in chartObjects)
            if (go != null) Destroy(go);
        chartObjects.Clear();
    }

    private void PushChartPointFromStandingsLeader()
    {
        // uses gm standings: top1 driver points
        var top = gm != null ? gm.GetDriverStandingsTopN(1) : null;
        if (top == null || top.Count == 0) return;

        int pts = top[0].points;
        chartSeries.Add(pts);
        if (chartSeries.Count > 40) chartSeries.RemoveAt(0); // safety
    }

    private void RedrawChart()
    {
        if (chartContainer == null) return;

        var plot = chartContainer.Find("Plot") as RectTransform;
        if (plot == null) return;

        ClearChartObjects();

        if (chartSeries.Count < 2)
            return;

        int max = Mathf.Max(1, chartSeries.Max());
        float w = plot.rect.width;
        float h = plot.rect.height;

        // padding inside plot
        float padL = 14f;
        float padR = 14f;
        float padT = 14f;
        float padB = 14f;

        float usableW = Mathf.Max(1f, w - padL - padR);
        float usableH = Mathf.Max(1f, h - padT - padB);

        Vector2 ToPoint(int i, int val)
        {
            float x = padL + (usableW * (i / (float)(chartSeries.Count - 1)));
            float y = padB + (usableH * (val / (float)max));
            return new Vector2(x, y);
        }

        // points + segments
        for (int i = 0; i < chartSeries.Count; i++)
        {
            Vector2 p = ToPoint(i, chartSeries[i]);
            CreateChartDot(plot, p);
            if (i > 0)
            {
                Vector2 p0 = ToPoint(i - 1, chartSeries[i - 1]);
                CreateChartSegment(plot, p0, p);
            }
        }

        // label max
        var label = CreateTMP(plot, "MaxLabel", $"max {max}", 14, TextAlignmentOptions.TopRight);
        label.color = new Color(1f, 1f, 1f, 0.70f);
        var rt = label.rectTransform;
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-8, -6);
        rt.sizeDelta = new Vector2(200, 24);
        chartObjects.Add(label.gameObject);
    }

    private void CreateChartDot(RectTransform plot, Vector2 pos)
    {
        var go = new GameObject("Dot", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(plot, false);

        var img = go.GetComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.90f);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(8, 8);

        chartObjects.Add(go);
    }

    private void CreateChartSegment(RectTransform plot, Vector2 a, Vector2 b)
    {
        var go = new GameObject("Seg", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(plot, false);

        var img = go.GetComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.55f);

        Vector2 d = b - a;
        float len = d.magnitude;
        float ang = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = a;
        rt.sizeDelta = new Vector2(len, 3f);
        rt.localRotation = Quaternion.Euler(0, 0, ang);

        chartObjects.Add(go);
    }

    private struct SessionRow
    {
        public string posStr;
        public string driver;
        public string team;
    }

    private List<SessionRow> GetSessionRows(string key)
    {
        // If we have weekend result object, use it
        if (lastWeekendResultObj != null)
        {
            // sessions is List<SessionResult>, each has "type" and "classification"
            var sessions = GetListMember(lastWeekendResultObj, "sessions");
            if (sessions != null)
            {
                // find session by type string match
                foreach (var s in sessions)
                {
                    string typeStr = GetEnumStringAny(s, "type", "Type");
                    if (SessionKeyMatches(key, typeStr))
                    {
                        var classification = GetListMember(s, "classification");
                        if (classification == null) break;

                        // each entry has driver/team and position fields
                        var rows = new List<SessionRow>();
                        foreach (var e in classification)
                        {
                            string driver = GetNestedStringAny(e, "driver", "displayName", "name") ?? "(driver)";
                            string team = GetNestedStringAny(e, "team", "displayName", "name") ?? "(team)";
                            int pos = GetPosForSessionKey(key, e);
                            rows.Add(new SessionRow
                            {
                                posStr = pos.ToString("00"),
                                driver = driver,
                                team = team
                            });
                        }

                        // order by pos
                        rows = rows.OrderBy(r => int.TryParse(r.posStr, out int p) ? p : 999).ToList();
                        return rows;
                    }
                }
            }
        }

        // fallback legacy: show Grid or Race finish only
        if (lastRaceEntryResults != null && lastRaceEntryResults.Count > 0)
        {
            if (key == "QUALI")
            if (key == "QUALI")
{
    return lastRaceEntryResults
        .OrderBy(e => e.gridPos)
        .Select(e => new SessionRow
        {
            posStr = e.gridPos.ToString("00"),
            driver = string.IsNullOrWhiteSpace(e.driverName) ? "-" : e.driverName,
            team   = string.IsNullOrWhiteSpace(e.teamName) ? "-" : e.teamName
        })
        .ToList();
}

if (key == "RACE")
{
    return lastRaceEntryResults
        .OrderBy(e => e.finishPos)
        .Select(e => new SessionRow
        {
            posStr = e.finishPos.ToString("00"),
            driver = string.IsNullOrWhiteSpace(e.driverName) ? "-" : e.driverName,
            team   = string.IsNullOrWhiteSpace(e.teamName) ? "-" : e.teamName
        })
        .ToList();
}


            return new List<SessionRow>();
        }

        return new List<SessionRow>();
    }

    private int GetPosForSessionKey(string key, object entry)
    {
        // DriverEntry has gridPos, finishPos, sprintFinishPos etc.
        if (key == "QUALI")
            return GetIntAny(entry, "gridPos", "GridPos");

        if (key == "RACE")
            return GetIntAny(entry, "finishPos", "FinishPos");

        if (key == "SPRINT")
            return GetIntAny(entry, "sprintFinishPos", "SprintFinishPos", "finishPos", "FinishPos");

        if (key == "TL1")
            return GetRankByScore(entry, "p1Score");

        if (key == "TL2")
            return GetRankByScore(entry, "p2Score");

        if (key == "TL3")
            return GetRankByScore(entry, "p3Score");

        return 0;
    }

    private int GetRankByScore(object entry, string scoreField)
    {
        // We do not have direct pos stored for practice; rank by score descending.
        // This helper needs access to the full classification list already sorted.
        // In our UI we sort by posStr based on this return, so for practice we cannot compute rank here reliably.
        // We'll just return 0 and rely on classification ordering (already should be sorted by pX score in the simulator session).
        // To keep display stable, we fallback to finishPos if available.
        int fp = GetIntAny(entry, "finishPos", "FinishPos");
        return fp > 0 ? fp : 0;
    }

    private bool SessionKeyMatches(string key, string typeStr)
    {
        typeStr = (typeStr ?? "").ToUpperInvariant();

        if (key == "TL1") return typeStr.Contains("PRACTICE1");
        if (key == "TL2") return typeStr.Contains("PRACTICE2");
        if (key == "TL3") return typeStr.Contains("PRACTICE3");
        if (key == "QUALI") return typeStr.Contains("QUALIFYING");
        if (key == "SPRINT") return typeStr.Contains("SPRINT");
        if (key == "RACE") return typeStr.Contains("RACE");

        return false;
    }

    private string BuildDriverStandingsText()
    {
        var sb = new StringBuilder();
        sb.AppendLine("DRIVER STANDINGS (Top 10)");

        if (gm == null)
        {
            sb.AppendLine("-");
            return sb.ToString();
        }

        var ds = gm.GetDriverStandingsTopN(10);
        if (ds == null || ds.Count == 0)
        {
            sb.AppendLine("-");
            return sb.ToString();
        }

        for (int i = 0; i < ds.Count; i++)
        {
            var d = ds[i];
            sb.Append("#").Append((i + 1).ToString("00")).Append(" ");
            sb.Append(d.name).Append(" — ").Append(d.points).Append(" pts");
            sb.Append("  (W:").Append(d.wins).Append(")");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string BuildTeamStandingsText()
    {
        var sb = new StringBuilder();
        sb.AppendLine("TEAM STANDINGS");

        if (gm == null)
        {
            sb.AppendLine("-");
            return sb.ToString();
        }

        var ts = gm.GetTeamStandingsAll();
        if (ts == null || ts.Count == 0)
        {
            sb.AppendLine("-");
            return sb.ToString();
        }

        for (int i = 0; i < ts.Count; i++)
        {
            var t = ts[i];
            sb.Append("#").Append((i + 1).ToString("00")).Append(" ");
            sb.Append(t.name).Append(" — ").Append(t.points).Append(" pts");
            sb.Append("  (W:").Append(t.wins).Append(")");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    // ===========================
    // Legacy conversion
    // ===========================

    private List<RaceEntryResult> BuildLegacyRaceEntryResultsFromWeekend(object weekendResult)
    {
        var results = new List<RaceEntryResult>();

        // race session classification
        var sessions = GetListMember(weekendResult, "sessions");
        if (sessions == null) return results;

        object raceSession = null;
        foreach (var s in sessions)
        {
            string typeStr = GetEnumStringAny(s, "type", "Type");
            if ((typeStr ?? "").ToUpperInvariant().Contains("RACE"))
            {
                raceSession = s;
                break;
            }
        }

        if (raceSession == null) return results;

        var classification = GetListMember(raceSession, "classification");
        if (classification == null) return results;

        foreach (var e in classification)
        {
            int gridPos = GetIntAny(e, "gridPos", "GridPos");
            int finishPos = GetIntAny(e, "finishPos", "FinishPos");

            string dId = GetNestedStringAny(e, "driver", "driverId") ?? "";
            string dName = GetNestedStringAny(e, "driver", "displayName") ?? "(driver)";
            string tId = GetNestedStringAny(e, "team", "teamId") ?? "";
            string tName = GetNestedStringAny(e, "team", "displayName") ?? "(team)";

            bool dnf = false;

            results.Add(new RaceEntryResult(gridPos, finishPos, dId, dName, tId, tName, dnf));
        }

        return results;
    }

    // ===========================
    // Reflection helpers
    // ===========================

    private static bool HasMember(object obj, string name)
    {
        if (obj == null) return false;
        var t = obj.GetType();
        const System.Reflection.BindingFlags flags =
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic;
        return t.GetField(name, flags) != null || t.GetProperty(name, flags) != null;
    }

    private static object GetMember(object obj, string name)
    {
        if (obj == null) return null;
        var t = obj.GetType();
        const System.Reflection.BindingFlags flags =
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic;

        var p = t.GetProperty(name, flags);
        if (p != null)
        {
            try { return p.GetValue(obj); } catch { return null; }
        }

        var f = t.GetField(name, flags);
        if (f != null)
        {
            try { return f.GetValue(obj); } catch { return null; }
        }

        return null;
    }

    private static List<object> GetListMember(object obj, string name)
    {
        var v = GetMember(obj, name);
        if (v == null) return null;

        if (v is System.Collections.IEnumerable en)
        {
            var list = new List<object>();
            foreach (var it in en) list.Add(it);
            return list;
        }

        return null;
    }

    private static string GetEnumStringAny(object obj, params string[] names)
    {
        if (obj == null) return null;
        foreach (var n in names)
        {
            var v = GetMember(obj, n);
            if (v == null) continue;
            return v.ToString();
        }
        return null;
    }

    private static string GetNestedStringAny(object obj, string nestedObjName, params string[] names)
    {
        var nested = GetMember(obj, nestedObjName);
        if (nested == null) return null;

        foreach (var n in names)
        {
            var v = GetMember(nested, n);
            if (v is string s && !string.IsNullOrWhiteSpace(s)) return s;
        }

        return null;
    }

    private static int GetIntAny(object obj, params string[] names)
    {
        if (obj == null) return 0;

        var t = obj.GetType();
        const System.Reflection.BindingFlags flags =
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic;

        foreach (var n in names)
        {
            var p = t.GetProperty(n, flags);
            if (p != null)
            {
                try
                {
                    var v = p.GetValue(obj);
                    if (v is int i) return i;
                    if (v is short s) return s;
                    if (v is byte b) return b;
                    if (v is long l) return (int)l;
                }
                catch { }
            }

            var f = t.GetField(n, flags);
            if (f != null)
            {
                try
                {
                    var v = f.GetValue(obj);
                    if (v is int i) return i;
                    if (v is short s) return s;
                    if (v is byte b) return b;
                    if (v is long l) return (int)l;
                }
                catch { }
            }
        }

        return 0;
    }

    // ===========================
    // UI helpers
    // ===========================

    private static TMP_Text CreateTMP(RectTransform parent, string name, string text, float size, TextAlignmentOptions align)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        var t = go.GetComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.alignment = align;
        t.textWrappingMode = TextWrappingModes.Normal;
        t.color = Color.white;

        return t;
    }

    private static TMP_InputField CreateTMPInput(RectTransform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
        go.transform.SetParent(parent, false);

        var img = go.GetComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.95f);

        var input = go.GetComponent<TMP_InputField>();

        var textArea = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
        textArea.transform.SetParent(go.transform, false);
        var taRT = textArea.GetComponent<RectTransform>();
        StretchToFull(taRT, 8, 6, 8, 6);

        var ph = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
        ph.transform.SetParent(textArea.transform, false);
        var phT = ph.GetComponent<TextMeshProUGUI>();
        phT.text = "seed...";
        phT.fontSize = 18;
        phT.color = new Color(0.2f, 0.2f, 0.2f, 0.6f);
        phT.alignment = TextAlignmentOptions.Left;
        phT.textWrappingMode = TextWrappingModes.NoWrap;
        StretchToFull(phT.rectTransform);

        var txt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txt.transform.SetParent(textArea.transform, false);
        var txtT = txt.GetComponent<TextMeshProUGUI>();
        txtT.text = "";
        txtT.fontSize = 18;
        txtT.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        txtT.alignment = TextAlignmentOptions.Left;
        txtT.textWrappingMode = TextWrappingModes.NoWrap;
        StretchToFull(txtT.rectTransform);

        input.textViewport = taRT;
        input.textComponent = txtT;
        input.placeholder = phT;

        return input;
    }

    private static Button CreateButton(RectTransform parent, string name, string label)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        var img = go.GetComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.92f);

        var btn = go.GetComponent<Button>();

        var txt = new GameObject("Text (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI));
        txt.transform.SetParent(go.transform, false);

        var t = txt.GetComponent<TextMeshProUGUI>();
        t.text = label;
        t.fontSize = 18;
        t.alignment = TextAlignmentOptions.Center;
        t.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        t.textWrappingMode = TextWrappingModes.NoWrap;

        StretchToFull(t.rectTransform);

        return btn;
    }

    private static void StretchToFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void StretchToFull(RectTransform rt, float left, float bottom, float right, float top)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
    }

    private static void AnchorTopLeft(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
    }

    private static void AnchorTopRight(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-x, y);
        rt.sizeDelta = new Vector2(w, h);
    }
}
