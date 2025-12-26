using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using F1Manager.Data;
using F1Manager.Sim;

// =========================
// ADDED (Complemento do outro GM)
// =========================
using F1Manager.Core.Calendar;
using F1Manager.Core.Events;
using F1Manager.Core.Save;
using F1Manager.Core.Season;
using F1Manager.Core.World;

// =========================
// NEW (Car baseline → TeamCarState + Finance + Facilities)
// =========================
using F1Manager.Core.Car;
using F1Manager.Core.Finance;
using F1Manager.Core.Facilities;

// =========================
// NEW (Driver Market / Contracts / Academy / Scouting)
// =========================
using F1Manager.Core.Market;
using F1Manager.Core.Academy;
using F1Manager.Core.Scouting;
using F1Manager.Core.Randomness;
using F1Manager.Core.Contracts;

// =========================
// NEW (Procedural Generation + Narrative)
// =========================
using F1Manager.Core.Generation;
using F1Manager.Core.Narrative;

// ✅ Resolve ambiguidade entre Data.RegulationRuleset e Core.Season.RegulationRuleset
using SeasonRegulationRuleset = F1Manager.Core.Season.RegulationRuleset;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Season")]
    public int currentSeason = 2026;
    public bool seasonRunning = false;

    [Header("Databases")]
    [SerializeField] private TeamsDatabase teamsDatabase2026;
    [SerializeField] private DriversDatabase driversDatabase2026;

    [Header("Tracks / Calendar")]
    [SerializeField] private TracksDatabase tracksDatabase2026;
    [SerializeField] private SeasonCalendar seasonCalendar2026;

    [Header("Rules / Standings")]
    [SerializeField] private PointsRuleset pointsRuleset2026;

    // ============================================================
    // NEW: Car Baseline + Economy States (runtime, depois vai pro save)
    // ============================================================
    [Header("Car / Economy Runtime State (NEW)")]
    [Tooltip("Se ligado, cria TeamCarState + FinanceState + FacilityState para cada equipe carregada (baseline vem do TeamData).")]
    [SerializeField] private bool initializeCarAndEconomyOnStart = true;

    [Tooltip("Se ligado, aplica wear nas peças após cada SimulateRound (simples: +baseWearPerRace por peça).")]
    [SerializeField] private bool applyWearAfterEachRound = true;

    [Header("Car Definitions (Optional but recommended)")]
    [Tooltip("Arraste aqui os CarPartDefinition_ (um por CarPartType). Isso controla wear e (futuramente) pesos de performance.")]
    [SerializeField] private List<CarPartDefinition> carPartDefinitions = new List<CarPartDefinition>();

    // Runtime state dictionaries keyed by teamId (string)
    private readonly Dictionary<string, TeamCarState> carStateByTeamId = new Dictionary<string, TeamCarState>();
    private readonly Dictionary<string, FinanceState> financeByTeamId = new Dictionary<string, FinanceState>();
    private readonly Dictionary<string, FacilityState> facilitiesByTeamId = new Dictionary<string, FacilityState>();

    // Cache em runtime
    public IReadOnlyList<TeamData> Teams => teams;
    private readonly List<TeamData> teams = new List<TeamData>();

    // Drivers cache (por equipe)
    private readonly Dictionary<string, List<DriverData>> driversByTeam
        = new Dictionary<string, List<DriverData>>();

    // Standings em runtime (somatório de pontos da temporada)
    private ChampionshipStandings standings;

    // ============================================================
    // NEW: Driver Market / Academy / Scouting (Runtime MVP)
    // ============================================================
    [Header("Driver Market (NEW)")]
    [Tooltip("Se ligado, inicializa mercado de pilotos/contratos/academia/scouting (MVP data-driven, sem UI).")]
    [SerializeField] private bool initializeDriverMarketOnStart = true;

    [Tooltip("Seed opcional para o mercado. Se 0, usa State.worldSeed (quando existir) ou currentSeason.")]
    [SerializeField] private int driverMarketSeedOverride = 0;

    // Runtime states (depois você pode mover para SaveGameData)
    private DriverMarketState driverMarketState = new DriverMarketState();
    private AcademyState academyState = new AcademyState();
    private ScoutingState scoutingState = new ScoutingState();

    // Services
    private DriverMarketService driverMarket;
    private AcademyService academy;
    private ScoutingService scouting;
    private IRandom driverRng;

    // ============================================================
    // Procedural (sem LLM) + Narrative
    // ============================================================
    [Header("Procedural Generation (NEW)")]
    [SerializeField] private GenerationRuleset generationRuleset;
    [SerializeField] private ExpansionRuleset expansionRuleset;
    [SerializeField] private SponsorRuleset sponsorRuleset;
    [SerializeField] private SeasonRegulationRuleset seasonRegulationRuleset;

    [Header("Narrative (NEW)")]
    [SerializeField] private NarrativeLibrary narrativeLibrary;

    // Runtime generated states (MVP - pode ir pro Save depois)
    private readonly List<DriverState> generatedDrivers = new List<DriverState>();
    private readonly List<TeamState> generatedTeams = new List<TeamState>();
    private readonly List<SponsorBrand> sponsorBrands = new List<SponsorBrand>();
    private readonly List<SponsorDeal> sponsorDeals = new List<SponsorDeal>();
    private RegulationsState currentRegulations; // estado das regras da era (Core.Season)

    // Deterministic RNG streams (baseado no worldSeed)
    private RandomService worldRngService;

    // Narrative
    private NarrativeService narrative;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // ============================================================
        // 🔁 IMPORTANTE: Primeiro resolve Core Save/Calendar Pipeline,
        // depois inicia a temporada e caches (evita estado desalinhado).
        // ============================================================

        if (autoLoadOnStart)
        {
            bool loaded = Core_TryLoadGame_InternalReturnBool();
            if (!loaded)
                Core_NewGame();
        }

        StartSeason();
    }

    public void StartSeason()
    {
        seasonRunning = true;
        Debug.Log($"Season {currentSeason} started");

        // Inicializa standings para a temporada
        standings = new ChampionshipStandings();

        // Confirmação rápida do DB de pilotos
        if (driversDatabase2026 == null)
            Debug.LogError("DriversDatabase_2026 NÃO foi atribuído no GameManager.");
        else
            Debug.Log($"DriversDatabase_2026 OK. Total drivers: {driversDatabase2026.Drivers.Count}");

        // Confirmação rápida do DB de tracks (opcional, mas útil)
        if (tracksDatabase2026 == null)
            Debug.LogWarning("TracksDatabase_2026 NÃO foi atribuído no GameManager (ok por enquanto, mas necessário para calendário).");
        else
            Debug.Log($"TracksDatabase_2026 OK. Total tracks: {tracksDatabase2026.Tracks.Count}");

        // Confirmação rápida do calendário
        if (seasonCalendar2026 == null)
            Debug.LogWarning("SeasonCalendar_2026 NÃO foi atribuído no GameManager (necessário para simular rounds).");
        else
            Debug.Log($"SeasonCalendar_2026 OK. Total rounds: {seasonCalendar2026.Weekends.Count}");

        // Confirmação do ruleset de pontos
        if (pointsRuleset2026 == null)
            Debug.LogWarning("[Points] PointsRuleset_2026 NÃO atribuído no GameManager. Pontos não serão computados.");
        else
            Debug.Log($"[Points] Ruleset loaded: {pointsRuleset2026.rulesetName}");

        LoadTeamsForCurrentSeason();
        LoadDriversForCurrentSeason();

        // ============================================================
        // RNG determinístico por mundo/era
        // ============================================================
        EnsureWorldRng();

        // ============================================================
        // NEW: inicializa baseline de carro + finanças + facilities
        // ============================================================
        if (initializeCarAndEconomyOnStart)
            InitializeCarAndEconomyRuntimeStates();

        // ============================================================
        // NEW: inicializa Mercado/Academia/Scouting
        // ============================================================
        if (initializeDriverMarketOnStart)
            InitializeDriverMarketRuntimeStates();

        // ============================================================
        // NEW: inicializa Narrative
        // ============================================================
        if (narrativeLibrary != null)
            narrative = new NarrativeService(narrativeLibrary);

        // ============================================================
        // NEW: inicializa RegulationsState da era (se ainda não existir)
        // ============================================================
        EnsureInitialRegulations();
    }

    public void EndSeason()
    {
        seasonRunning = false;

        // Offseason pipeline (procedural): regs + rookies + team entry + sponsors
        RunOffseasonProceduralPipeline();

        currentSeason++;
        Debug.Log($"Season ended. Next season: {currentSeason}");
    }

    private void EnsureWorldRng()
    {
        int seed =
            (State != null ? State.worldSeed : 0);

        if (seed == 0)
        {
            // fallback determinístico (não “random” de Unity, pra não mudar a cada play)
            seed = currentSeason * 1000 + 12345;
        }

        worldRngService = new RandomService(seed);
    }

    private void EnsureInitialRegulations()
    {
        if (currentRegulations != null) return;

        // seed/regra inicial - você pode puxar de um asset “baseline 2026” depois
        currentRegulations = new RegulationsState
        {
            season = currentSeason,
            aeroWeight = 0.34f,
            powerWeight = 0.33f,
            reliabilityWeight = 0.33f,
            costCap = 0.60f,
            sprintWeekendEnabled = true,
            sprintWeekendsPerSeason = 6,
            changeVolatility = 0.25f
        };
    }

    private void RunOffseasonProceduralPipeline()
    {
        if (worldRngService == null) EnsureWorldRng();

        int nextSeason = currentSeason + 1;

        // =========================
        // 1) Regulations change
        // =========================
        if (seasonRegulationRuleset != null)
        {
            var rngRegs = worldRngService.GetStream($"GEN_REGS_{nextSeason}");
            var regGen = new RegulationChangeGenerator(seasonRegulationRuleset);
            currentRegulations = regGen.GenerateNext(rngRegs, currentRegulations, nextSeason);

            Debug.Log($"🧩 [REGS] {nextSeason}: aero={currentRegulations.aeroWeight:0.00} power={currentRegulations.powerWeight:0.00} rel={currentRegulations.reliabilityWeight:0.00} cap={currentRegulations.costCap:0.00} sprint={(currentRegulations.sprintWeekendEnabled ? currentRegulations.sprintWeekendsPerSeason : 0)}");
        }
        else
        {
            Debug.LogWarning("[REGS] seasonRegulationRuleset not assigned. Skipping regulation evolution.");
        }

        // =========================
        // 2) Driver regen rookies
        // =========================
        if (generationRuleset != null)
        {
            var rngDrivers = worldRngService.GetStream($"GEN_DRIVERS_{nextSeason}");
            var gen = new DriverRegenGenerator(generationRuleset);

            var existingIds = new HashSet<string>(
                (driversDatabase2026 != null && driversDatabase2026.Drivers != null)
                    ? driversDatabase2026.Drivers.Select(GetDriverStableIdSafe)
                    : Enumerable.Empty<string>()
            );

            var rookies = gen.GenerateRookies(rngDrivers, nextSeason, existingIds);
            generatedDrivers.AddRange(rookies);

            Debug.Log($"🧬 [ROOKIES] Generated {rookies.Count} rookies for {nextSeason}.");
        }
        else
        {
            Debug.LogWarning("[ROOKIES] generationRuleset not assigned. Skipping rookie generation.");
        }

        // =========================
        // 3) Team entry
        // =========================
        if (expansionRuleset != null)
        {
            var rngTeams = worldRngService.GetStream($"GEN_TEAMS_{nextSeason}");
            var teamGen = new TeamEntryGenerator(expansionRuleset);

            var existingTeamIds = new HashSet<string>(teams.Select(t => NormalizeId(t.teamId)));
            int currentTeamCount = teams.Count;

            float gridHealth01 = ComputeGridHealth01();

            var newTeam = teamGen.TryGenerateTeam(
                rngTeams,
                nextSeason,
                existingTeamIds,
                currentTeamCount,
                gridHealth01
            );

            if (newTeam != null)
            {
                generatedTeams.Add(newTeam);
                Debug.Log($"🏎️ [EXPANSION] New team enters in {nextSeason}: {newTeam.teamName} ({newTeam.country}) budget={newTeam.budget:0.00} carBase={newTeam.carBase:0.00}");
            }
            else
            {
                Debug.Log($"🏎️ [EXPANSION] No new team for {nextSeason} (rules/chance/grid health).");
            }
        }
        else
        {
            Debug.LogWarning("[EXPANSION] expansionRuleset not assigned. Skipping team entry generation.");
        }

        // =========================
        // 4) Sponsors (brands + deals)
        // =========================
        if (sponsorRuleset != null)
        {
            var rngSp = worldRngService.GetStream($"GEN_SPONSORS_{nextSeason}");
            var spGen = new SponsorGenerator(sponsorRuleset);

            var existingSponsorIds = new HashSet<string>(sponsorBrands.Select(b => b.sponsorId));
            var newBrands = spGen.GenerateNewBrands(rngSp, existingSponsorIds, nextSeason);
            sponsorBrands.AddRange(newBrands);

            // Propor 1 deal por time (MVP) usando teamPrestige calculado
            int teamsCount = teams.Count;
            for (int i = 0; i < teams.Count; i++)
            {
                var t = teams[i];
                if (t == null) continue;

                string teamId = NormalizeId(t.teamId);
                float teamPrestige01 = ComputeTeamPrestige01(t);

                // lastPos: se você tiver standings final salvo, pluga aqui.
                // por enquanto: usa posição atual no cache como proxy.
                int lastPos = i + 1;

                var brand = rngSp.Pick(newBrands.Count > 0 ? newBrands : sponsorBrands);
                var deal = spGen.ProposeDeal(rngSp, brand, teamId, nextSeason, teamPrestige01, lastPos, teamsCount);

                sponsorDeals.Add(deal);
            }

            Debug.Log($"💰 [SPONSORS] +{newBrands.Count} brands, total deals={sponsorDeals.Count}.");
        }
        else
        {
            Debug.LogWarning("[SPONSORS] sponsorRuleset not assigned. Skipping sponsor generation.");
        }
    }

    private float ComputeGridHealth01()
    {
        // MVP: saúde do grid baseada em budgets normalizados + estabilidade (>=0) + facilities
        if (teams == null || teams.Count == 0) return 0.5f;

        float sum = 0f;
        int n = 0;

        for (int i = 0; i < teams.Count; i++)
        {
            var t = teams[i];
            if (t == null) continue;

            float prestige = ComputeTeamPrestige01(t);
            sum += prestige;
            n++;
        }

        if (n <= 0) return 0.5f;
        return Mathf.Clamp01(sum / n);
    }

    private float ComputeTeamPrestige01(TeamData t)
    {
        // ✅ Não depende de fields que não existem.
        // Pega sinais que você já tem no TeamData (pelo seu GameManager):
        // startingBudgetMillions, hqLevel, aeroDepartment, powerUnitDept, strategyTeam
        float budget01 = 0.5f;
        if (t != null)
        {
            // Map budget 50..300 milhões => 0..1 (ajuste como quiser)
            float b = Mathf.Max(0f, t.startingBudgetMillions);
            budget01 = Mathf.InverseLerp(50f, 300f, b);

            float facilitiesAvg =
                (Mathf.Clamp01(t.hqLevel / 10f) +
                 Mathf.Clamp01(t.aeroDepartment / 10f) +
                 Mathf.Clamp01(t.powerUnitDept / 10f) +
                 Mathf.Clamp01(t.strategyTeam / 10f)) / 4f;

            // mistura
            return Mathf.Clamp01(0.60f * budget01 + 0.40f * facilitiesAvg);
        }
        return 0.5f;
    }

    private void LoadTeamsForCurrentSeason()
    {
        teams.Clear();

        if (teamsDatabase2026 == null)
        {
            Debug.LogError("TeamsDatabase_2026 NÃO foi atribuído no GameManager.");
            return;
        }

        if (teamsDatabase2026.teams == null || teamsDatabase2026.teams.Count == 0)
        {
            Debug.LogWarning("TeamsDatabase_2026 está vazio.");
            return;
        }

        foreach (var t in teamsDatabase2026.teams)
        {
            if (t == null) continue;
            teams.Add(t);
        }

        Debug.Log($"Teams loaded for {currentSeason}: {teams.Count}");

        for (int i = 0; i < teams.Count; i++)
            Debug.Log($"[{i}] {teams[i].displayName} ({teams[i].teamId})");
    }

    private void LoadDriversForCurrentSeason()
    {
        driversByTeam.Clear();

        if (driversDatabase2026 == null)
        {
            Debug.LogError("DriversDatabase_2026 NÃO foi atribuído no GameManager.");
            return;
        }

        if (teams == null || teams.Count == 0)
        {
            Debug.LogWarning("Nenhum time carregado ainda. Carregue os times antes dos pilotos.");
            return;
        }

        for (int i = 0; i < teams.Count; i++)
        {
            var team = teams[i];
            if (team == null) continue;

            string teamId = NormalizeId(team.teamId);
            if (string.IsNullOrWhiteSpace(teamId))
            {
                Debug.LogWarning($"[Grid] Team com teamId vazio: {team.displayName}");
                continue;
            }

            var teamDrivers = driversDatabase2026.GetByTeam(
                teamId,
                includeReserves: true,
                includeTestDrivers: true
            );

            driversByTeam[teamId] = teamDrivers;

            LogTeamDrivers(team.displayName, teamId, teamDrivers);
        }
    }

    private void LogTeamDrivers(string teamName, string teamId, List<DriverData> drivers)
    {
        if (drivers == null || drivers.Count == 0)
        {
            Debug.LogWarning($"[Grid] {teamName} ({teamId}) SEM pilotos!");
            return;
        }

        string log = $"[Grid] {teamName}: ";

        for (int i = 0; i < drivers.Count; i++)
        {
            var d = drivers[i];
            if (d == null) continue;

            string driverName = string.IsNullOrWhiteSpace(d.displayName) ? d.name : d.displayName;
            log += $"{driverName} ({d.role})";

            if (i < drivers.Count - 1)
                log += " | ";
        }

        Debug.Log(log);
    }

    public IReadOnlyList<DriverData> GetDriversForTeam(string teamId)
    {
        if (string.IsNullOrWhiteSpace(teamId)) return null;

        string key = NormalizeId(teamId);
        if (driversByTeam.TryGetValue(key, out var list))
            return list;

        return null;
    }

    // ============================================================
    // NEW: Driver Market Runtime Initialization + Helpers
    // ============================================================

    private void InitializeDriverMarketRuntimeStates()
    {
        // Seed: override > State.worldSeed (se existir) > currentSeason
        int seed =
            driverMarketSeedOverride != 0 ? driverMarketSeedOverride :
            (State != null ? State.worldSeed : currentSeason * 1000 + 123);

        driverRng = new DeterministicRandom(seed);

        // states podem estar null se algo substituiu em runtime
        if (driverMarketState == null) driverMarketState = new DriverMarketState();
        if (academyState == null) academyState = new AcademyState();
        if (scoutingState == null) scoutingState = new ScoutingState();

        driverMarket = new DriverMarketService(driverMarketState, driverRng);
        academy = new AcademyService(academyState, driverMarketState, driverRng);
        scouting = new ScoutingService(scoutingState, driverMarketState, driverRng);

        driverMarket.InitializeForSeason(currentSeason);
        academy.InitializeForSeason();

        Debug.Log($"✅ [DriverMarket] Initialized. Seed={seed}. Drivers={driverMarketState.drivers.Count}, FreeAgents={driverMarketState.marketEntries.Count}");

        driverMarket.DebugPrintTopFreeAgents(10);
    }

    private void AdvanceDriverSystemsWeek()
    {
        if (!initializeDriverMarketOnStart) return;
        if (driverMarket == null || academy == null || scouting == null) return;

        scouting.AdvanceWeek();
        academy.AdvanceWeek();
        driverMarket.AdvanceWeek();
    }

    // ============================================================
    // NEW: Car & Economy Runtime Initialization + Helpers
    // ============================================================

    private void InitializeCarAndEconomyRuntimeStates()
    {
        carStateByTeamId.Clear();
        financeByTeamId.Clear();
        facilitiesByTeamId.Clear();

        if (teams == null || teams.Count == 0)
        {
            Debug.LogWarning("[Car/Econ] No teams loaded. Skipping init.");
            return;
        }

        int defsCount = carPartDefinitions != null ? carPartDefinitions.Count : 0;
        if (defsCount <= 0)
            Debug.LogWarning("[Car/Econ] carPartDefinitions is empty. Wear will use fallback values.");
        else
            Debug.Log($"[Car/Econ] Loaded {defsCount} CarPartDefinition(s).");

        for (int i = 0; i < teams.Count; i++)
        {
            var t = teams[i];
            if (t == null) continue;

            string teamId = NormalizeId(t.teamId);
            if (string.IsNullOrWhiteSpace(teamId)) continue;

            // CAR baseline from TeamData detailed 2026
            TeamCarState car = TeamCarInitializer.BuildFromTeamData(t, specYear: currentSeason);
            car.teamId = teamId;
            carStateByTeamId[teamId] = car;

// FINANCE baseline (usar long internamente)
long startingBudgetUsd = (long)System.Math.Round(t.startingBudgetMillions * 1_000_000d);

FinanceState fin = new FinanceState
{
    teamId = teamId,
    currentBudget = startingBudgetUsd
};

financeByTeamId[teamId] = fin;


            // FACILITIES baseline
            FacilityState fac = new FacilityState
            {
                teamId = teamId,
                hqLevel = t.hqLevel,
                aeroDeptLevel = t.aeroDepartment,
                puDeptLevel = t.powerUnitDept,
                strategyTeamLevel = t.strategyTeam
            };
            FacilitiesService.RecalculateMultipliers(fac);
            facilitiesByTeamId[teamId] = fac;
        }

        Debug.Log($"✅ [Car/Econ] Initialized runtime states: cars={carStateByTeamId.Count}, finance={financeByTeamId.Count}, facilities={facilitiesByTeamId.Count}");
    }

    public TeamCarState GetCarState(string teamId)
    {
        if (string.IsNullOrWhiteSpace(teamId)) return null;
        string key = NormalizeId(teamId);
        carStateByTeamId.TryGetValue(key, out var s);
        return s;
    }

    public FinanceState GetFinanceState(string teamId)
    {
        if (string.IsNullOrWhiteSpace(teamId)) return null;
        string key = NormalizeId(teamId);
        financeByTeamId.TryGetValue(key, out var s);
        return s;
    }

    public FacilityState GetFacilityState(string teamId)
    {
        if (string.IsNullOrWhiteSpace(teamId)) return null;
        string key = NormalizeId(teamId);
        facilitiesByTeamId.TryGetValue(key, out var s);
        return s;
    }

    private CarPartDefinition GetPartDefinition(CarPartType type)
    {
        if (carPartDefinitions == null) return null;
        for (int i = 0; i < carPartDefinitions.Count; i++)
        {
            var d = carPartDefinitions[i];
            if (d != null && d.partType == type) return d;
        }
        return null;
    }

    private void ApplyWearForAllTeamsAfterRound()
    {
        if (!applyWearAfterEachRound) return;

        foreach (var kv in carStateByTeamId)
        {
            var car = kv.Value;
            if (car == null || car.parts == null) continue;

            for (int i = 0; i < car.parts.Count; i++)
            {
                var p = car.parts[i];
                var def = GetPartDefinition(p.partType);

                float wearAdd = def != null ? def.baseWearPerRace : 2f;
                p.wearPercent = Mathf.Clamp(p.wearPercent + wearAdd, 0f, 100f);
            }
        }
    }

    // ============================================================
    // SIMULAÇÃO (sem hardcode, sem UI)
    // ============================================================

    public void SimulateRound(int round, int seed = 0)
    {
        if (seasonCalendar2026 == null)
        {
            Debug.LogError("[Sim] seasonCalendar2026 is null. Assign SeasonCalendar_2026.asset in Inspector.");
            return;
        }

        // ✅ Use o tipo real: RaceWeekendData (não SeasonCalendar.Weekend)
        RaceWeekendData weekend = seasonCalendar2026.GetWeekendByRound(round);

        if (weekend == null || weekend.track == null)
        {
            Debug.LogError($"[Sim] Round {round} not found or track is null in SeasonCalendar.");
            return;
        }

        if (driversDatabase2026 == null)
        {
            Debug.LogError("[Sim] driversDatabase2026 is null.");
            return;
        }

        if (teams == null || teams.Count == 0)
        {
            Debug.LogError("[Sim] Teams cache is empty. LoadTeamsForCurrentSeason must run first.");
            return;
        }

        if (initializeCarAndEconomyOnStart && carStateByTeamId.Count == 0)
        {
            Debug.LogWarning("[Car/Econ] Runtime states not initialized yet. Initializing now.");
            InitializeCarAndEconomyRuntimeStates();
        }

        if (initializeDriverMarketOnStart && (driverMarket == null || academy == null || scouting == null))
        {
            Debug.LogWarning("[DriverMarket] Runtime states not initialized yet. Initializing now.");
            InitializeDriverMarketRuntimeStates();
        }

        // Log extra
       Debug.Log($"=== ROUND {round} — {GetTrackName(weekend)} | Format: {(weekend.HasSprint ? "SPRINT" : "STANDARD")} ===");


        var allDrivers = driversDatabase2026.Drivers;

        // ✅ Não tipa como SimpleRaceSimulator.Entry (não existe)
        // Deixa var: a lista retornada tem gridPos/finishPos/driver/team (no seu Sim já existe)
        var results = SimpleRaceSimulator.SimulateWeekend(
            weekend: weekend,
            teams: teams,
            allDrivers: allDrivers,
            seasonYear: currentSeason,
            seed: seed
        );

Debug.Log("QUALI (Grid):");
foreach (var e in results.entries.OrderBy(x => x.gridPos))
    Debug.Log($"P{e.gridPos:00}  {e.driver.displayName}  | {e.team.displayName}");

Debug.Log("RACE (Finish):");
foreach (var e in results.entries.OrderBy(x => x.finishPos))
    Debug.Log($"P{e.finishPos:00}  {e.driver.displayName}  | {e.team.displayName}");


        // ============================================================
        // PONTOS + STANDINGS
        // ============================================================

        if (pointsRuleset2026 == null)
        {
            Debug.LogWarning("[Points] No ruleset assigned. Skipping championship points.");
            return;
        }

        if (standings == null)
            standings = new ChampionshipStandings();
var race = results.sessions
    .First(s => s.type == SimpleRaceSimulator.SessionType.Race)
    .classification;

foreach (var e in race)
{
    int pts = pointsRuleset2026.GetRacePointsForPosition(e.finishPos);
    standings.AwardPointsToDriver(e.driver, e.team, pts, e.finishPos);
}


        LogStandingsTop10();

        // ============================================================
        // NEW: Wear
        // ============================================================
        if (initializeCarAndEconomyOnStart)
            ApplyWearForAllTeamsAfterRound();

        // ============================================================
        // NEW: Tick manager systems
        // ============================================================
        AdvanceDriverSystemsWeek();

        // ============================================================
        // NEW: Narrative (pós-corrida)
        // ============================================================
        TryGenerateNarrativeForRound(round, weekend, results.entries);
    }

    private void TryGenerateNarrativeForRound(
    int round,
    RaceWeekendData weekend,
    List<SimpleRaceSimulator.DriverEntry> results
)
{
    if (narrative == null || worldRngService == null) return;
    if (weekend == null) return;
    if (results == null || results.Count == 0) return;

    // Winner = P1 finish
    var winnerEntry = results
        .OrderBy(x => x.finishPos)
        .FirstOrDefault();

    if (winnerEntry == null) return;

    string trackName = GetTrackName(weekend);

    // Streams: NARR_RACE_{season}_R{round}
    var rng = worldRngService.GetStream($"NARR_RACE_{currentSeason}_R{round}");

    // Safe driver/team names
    string winnerDriverName =
        winnerEntry.driver != null
            ? (!string.IsNullOrWhiteSpace(winnerEntry.driver.displayName) ? winnerEntry.driver.displayName : winnerEntry.driver.name)
            : "Unknown Driver";

    string winnerTeamName =
        winnerEntry.team != null
            ? (!string.IsNullOrWhiteSpace(winnerEntry.team.displayName) ? winnerEntry.team.displayName : winnerEntry.team.name)
            : "Unknown Team";

    // Stats MVP
    var raceCtx = new RaceContext
    {
        season = currentSeason,
        round = round,
        trackName = trackName,
        isSprintWeekend = weekend.HasSprint,
        weather = "Sunny",
        safetyCars = 0,
        retirements = 0,
        bigCrash = false,
        redFlag = false,
        winnerDriver = winnerDriverName,
        winnerTeam = winnerTeamName,
        winnerGrid = winnerEntry.gridPos,
        wasUpset = false,
        upsetVictimTeam = null,
        headlineHook = null
    };

    var statsCtx = new StatsContext
    {
        winnerWinStreak = 1,
        teamWinStreak = 1,
        titleFightTight = true,
        rivalDriver = "um rival",
        teamInCrisis = false,
        budgetTrouble = false,
        internalTension = false
    };

    var lines = narrative.GenerateNews(rng, NewsCategory.PostRace, raceCtx, statsCtx);
    if (lines == null || lines.Count == 0) return;

    Debug.Log("📰 === NEWS (Post-Race) ===");
    for (int i = 0; i < lines.Count; i++)
        Debug.Log(lines[i]);
}


private static string GetTrackName(RaceWeekendData w)
{
    if (w == null) return "Unknown";
    if (w.track == null) return "Unknown Track";

    // Nome principal definido no seu TrackData
    if (!string.IsNullOrWhiteSpace(w.track.trackName))
        return w.track.trackName;

    // Fallback: nome do asset (Unity)
    if (!string.IsNullOrWhiteSpace(w.track.name))
        return w.track.name;

    // Fallback final: ID
    if (!string.IsNullOrWhiteSpace(w.track.trackId))
        return w.track.trackId;

    return "Unknown Track";
}


    private void LogStandingsTop10()
    {
        if (standings == null) return;

        Debug.Log("=== CHAMPIONSHIP (Drivers) TOP 10 ===");
        var ds = standings.GetDriverStandingsSorted();
        for (int i = 0; i < Mathf.Min(10, ds.Count); i++)
        {
            Debug.Log($"#{i + 1:00} {ds[i].driver.displayName} - {ds[i].points} pts (W:{ds[i].wins})");
        }

        Debug.Log("=== CHAMPIONSHIP (Teams) ===");
        var tsSorted = standings.GetTeamStandingsSorted();
        for (int i = 0; i < tsSorted.Count; i++)
        {
            var teamName = tsSorted[i].team != null ? tsSorted[i].team.displayName : tsSorted[i].teamId;
            Debug.Log($"#{i + 1:00} {teamName} - {tsSorted[i].points} pts (W:{tsSorted[i].wins})");
        }
    }

    // ============================================================
    // DEBUG / QA (ContextMenu)
    // ============================================================

    [ContextMenu("Simulate/Reset Standings (New Season Table)")]
    private void DebugResetStandings()
    {
        standings = new ChampionshipStandings();
        Debug.Log("[Sim] Standings reset (new ChampionshipStandings created).");
    }

    [ContextMenu("Simulate/Run Round 1 (Seed 1234)")]
    private void DebugSimRound1()
    {
        SimulateRound(1, 1234);
    }

    [ContextMenu("Simulate/Run Round 2 (Seed 1234)")]
    private void DebugSimRound2()
    {
        SimulateRound(2, 1234);
    }

    [ContextMenu("Simulate/Run Full Season (Seed 1234)")]
    private void DebugSimFullSeason()
    {
        if (seasonCalendar2026 == null)
        {
            Debug.LogError("[Sim] seasonCalendar2026 is null. Assign SeasonCalendar_2026.asset in Inspector.");
            return;
        }

        int rounds = seasonCalendar2026.Weekends != null ? seasonCalendar2026.Weekends.Count : 0;
        if (rounds <= 0)
        {
            Debug.LogError("[Sim] SeasonCalendar has 0 weekends.");
            return;
        }

        Debug.Log($"[Sim] Running full season simulation: {rounds} rounds (seed 1234)");

        standings = new ChampionshipStandings();

        for (int r = 1; r <= rounds; r++)
            SimulateRound(r, 1234);

        Debug.Log("[Sim] Full season simulation completed.");

        // termina temporada e roda offseason (procedural)
        EndSeason();
    }

    // ============================================================
    // NEW DEBUG: Inspect Car/Econ State
    // ============================================================

    [ContextMenu("Car/Econ/Print Budgets")]
    private void DebugPrintBudgets()
    {
        if (financeByTeamId.Count == 0)
        {
            Debug.LogWarning("[Car/Econ] financeByTeamId is empty. Initialize first.");
            return;
        }

        Debug.Log("=== BUDGETS (Runtime) ===");
        foreach (var kv in financeByTeamId.OrderBy(k => k.Key))
        {
            Debug.Log($"{kv.Key}: ${kv.Value.currentBudget:0}");
        }
    }

    [ContextMenu("Car/Econ/Print Wear Summary (Top 3 Worn Parts per Team)")]
    private void DebugPrintWearSummary()
    {
        if (carStateByTeamId.Count == 0)
        {
            Debug.LogWarning("[Car/Econ] carStateByTeamId is empty. Initialize first.");
            return;
        }

        foreach (var kv in carStateByTeamId)
        {
            var car = kv.Value;
            if (car == null || car.parts == null) continue;

            var top = car.parts
                .OrderByDescending(p => p.wearPercent)
                .Take(3)
                .ToList();

            string s = $"[{car.teamId}] Wear top3: ";
            for (int i = 0; i < top.Count; i++)
            {
                s += $"{top[i].partType}={top[i].wearPercent:0.0}%";
                if (i < top.Count - 1) s += " | ";
            }
            Debug.Log(s);
        }
    }

    // ============================================================
    // NEW DEBUG: Driver Market
    // ============================================================

    [ContextMenu("DriverMarket/Print Top 10 Free Agents")]
    private void DebugPrintTopFreeAgents()
    {
        if (driverMarket == null)
        {
            Debug.LogWarning("[DriverMarket] Not initialized.");
            return;
        }
        driverMarket.DebugPrintTopFreeAgents(10);
    }

    [ContextMenu("DriverMarket/Advance Week (Market+Academy+Scouting)")]
    private void DebugAdvanceMarketWeek()
    {
        AdvanceDriverSystemsWeek();

        if (driverMarket != null)
            driverMarket.DebugPrintTopFreeAgents(5);
    }

    // ============================================================
    // =========================
    // ADDED (Complemento do outro GM)
    // =========================
    // ============================================================

    [Header("World / Rules (Optional - Core Save/Calendar Pipeline)")]
    [SerializeField] private RegulationsRuleset regulations;
    [SerializeField] private TrackPool trackPool; // opcional (se null, usa TracksDatabase)

    [Header("Calendar Settings (Optional - Generator)")]
    [Tooltip("Escolha 10 / 16 / 24. Se ruleset não permitir, ele ajusta para um permitido.")]
    [SerializeField] private int roundsWanted = 24;

    [Header("Save (Optional - Core SaveSystem)")]
    [SerializeField] private string saveFileName = "savegame.json";
    [SerializeField] private bool autoLoadOnStart = false; // padrão: não interfere no seu fluxo atual

    // Runtime state (pipeline do Core)
    public SeasonState State { get; private set; }

    // Cache extra (opcional, estilo do outro GM)
    public IReadOnlyList<DriverData> Drivers => _drivers;
    private readonly List<DriverData> _drivers = new List<DriverData>();

    private void BuildRuntimeCaches_CoreStyle()
    {
        _drivers.Clear();

        if (driversDatabase2026 != null && driversDatabase2026.drivers != null)
            _drivers.AddRange(driversDatabase2026.drivers);
        else if (driversDatabase2026 != null && driversDatabase2026.Drivers != null)
            _drivers.AddRange(driversDatabase2026.Drivers);
    }

    [ContextMenu("Core/New Game (Create SeasonState + Generated Calendar)")]
    public void Core_NewGame()
    {
        BuildRuntimeCaches_CoreStyle();

        int seed = UnityEngine.Random.Range(int.MinValue / 2, int.MaxValue / 2);
        State = new SeasonState
        {
            seasonYear = currentSeason,
            worldSeed = seed,
            currentRound = 1,
            seasonRunning = true,
            clock = new SeasonClock { day = 1 },
            calendar = new List<RoundEntry>(),
            results = new List<RoundResult>(),
            driverPoints = new Dictionary<string, int>(),
            teamPoints = new Dictionary<string, int>(),
        };

        Core_GenerateCalendarForState();

        Debug.Log($"✅ [Core] New Game. Seed={State.worldSeed}, Rounds={State.TotalRounds}");

        Core_SaveNow();
        GameEvents.RaiseSeasonStarted(State);
    }

    private bool Core_TryLoadGame_InternalReturnBool()
    {
        var data = SaveSystem.Load(saveFileName);
        if (data == null || data.seasonState == null) return false;

        State = data.seasonState;

        if (State.calendar == null || State.calendar.Count == 0)
            Core_GenerateCalendarForState();

        Debug.Log($"✅ [Core] Loaded season: {State.seasonYear} round {State.currentRound} day {State.clock.day}");
        GameEvents.RaiseSeasonStarted(State);
        return true;
    }

    [ContextMenu("Core/Try Load Game (SaveSystem)")]
    public void Core_TryLoadGame()
    {
        bool ok = Core_TryLoadGame_InternalReturnBool();
        if (!ok)
            Debug.LogWarning("[Core] No save found (or invalid).");
    }

    [ContextMenu("Core/Save Now")]
    public void Core_SaveNow()
    {
        if (State == null)
        {
            Debug.LogWarning("[Core] No State to save.");
            return;
        }

        var data = new SaveGameData
        {
            version = 1,
            seasonState = State
        };

        SaveSystem.Save(data, saveFileName);
        Debug.Log($"💾 [Core] Saved: {saveFileName}");
    }

    [ContextMenu("Core/Delete Save")]
    public void Core_DeleteSave()
    {
        SaveSystem.Delete(saveFileName);
        Debug.Log($"🗑️ [Core] Deleted save: {saveFileName}");
    }

    private void Core_GenerateCalendarForState()
    {
        if (State == null)
        {
            Debug.LogError("[Core] State is null. Create a new game first.");
            return;
        }

        if (regulations == null)
        {
            Debug.LogError("[Core] RegulationsRuleset is missing!");
            return;
        }

        var rules = regulations.calendarRules;

        List<TrackData> pool = new List<TrackData>();
        if (trackPool != null && trackPool.allTracks != null && trackPool.allTracks.Count > 0)
            pool.AddRange(trackPool.allTracks);
        else if (tracksDatabase2026 != null && tracksDatabase2026.tracks != null && tracksDatabase2026.tracks.Count > 0)
            pool.AddRange(tracksDatabase2026.tracks);
        else if (tracksDatabase2026 != null && tracksDatabase2026.Tracks != null && tracksDatabase2026.Tracks.Count > 0)
            pool.AddRange(tracksDatabase2026.Tracks);
        else
        {
            Debug.LogError("[Core] No tracks available! Assign TrackPool or TracksDatabase.");
            return;
        }

        State.calendar = SeasonCalendarGenerator.Generate(
            seasonYear: State.seasonYear,
            worldSeed: State.worldSeed,
            rules: rules,
            poolTracks: pool,
            roundsWanted: roundsWanted
        );

        if (State.currentRound < 1) State.currentRound = 1;
        if (State.currentRound > State.TotalRounds) State.currentRound = State.TotalRounds + 1;

        Debug.Log($"🗓️ [Core] Calendar generated: {State.calendar.Count} rounds.");
    }

    // ============================================================
    // HELPERS: ids seguros (não depender de DriverData.id etc)
    // ============================================================
    private static string NormalizeId(string s)
        => string.IsNullOrWhiteSpace(s) ? "" : s.Trim().ToLowerInvariant();

    private static string GetDriverStableIdSafe(DriverData d)
    {
        if (d == null) return "";
        // tenta driverId, senão name/displayName
        if (!string.IsNullOrWhiteSpace(d.driverId)) return NormalizeId(d.driverId);
        if (!string.IsNullOrWhiteSpace(d.name)) return NormalizeId(d.name);
        if (!string.IsNullOrWhiteSpace(d.displayName)) return NormalizeId(d.displayName);
        return "";
    }
// ============================================================
// UI HELPERS (MVP) — usado pelo F1UIController
// ============================================================

public int GetTotalRoundsFallback()
{
    if (seasonCalendar2026 != null && seasonCalendar2026.Weekends != null && seasonCalendar2026.Weekends.Count > 0)
        return seasonCalendar2026.Weekends.Count;

    if (State != null && State.calendar != null && State.calendar.Count > 0)
        return State.calendar.Count;

    return 0;
}

public int GetCurrentRoundFallback()
{
    if (State != null && State.currentRound > 0) return State.currentRound;
    return 1;
}

public RaceWeekendData GetWeekendByRoundFallback(int round)
{
    if (seasonCalendar2026 == null) return null;
    return seasonCalendar2026.GetWeekendByRound(round);
}

public void Debug_ResetSeasonRuntime()
{
    // reseta standings e caches de sim
    standings = new ChampionshipStandings();
}

public List<DriverStandingUI> GetDriverStandingsTopN(int n)
{
    var list = new List<DriverStandingUI>();
    if (standings == null) return list;

    var ds = standings.GetDriverStandingsSorted();
    int count = Mathf.Min(n, ds.Count);

    for (int i = 0; i < count; i++)
    {
        var s = ds[i];

        // tenta tirar o id do DriverData (mais consistente)
        string id =
            (s.driver != null && !string.IsNullOrWhiteSpace(s.driver.driverId)) ? s.driver.driverId :
            (s.driver != null && !string.IsNullOrWhiteSpace(s.driver.name)) ? s.driver.name :
            $"driver_{i}";

        string name =
            (s.driver != null && !string.IsNullOrWhiteSpace(s.driver.displayName)) ? s.driver.displayName :
            (s.driver != null && !string.IsNullOrWhiteSpace(s.driver.name)) ? s.driver.name :
            id;

        list.Add(new DriverStandingUI
        {
            id = id,
            name = name,
            points = s.points,
            wins = s.wins
        });
    }

    return list;
}

public List<TeamStandingUI> GetTeamStandingsAll()
{
    var list = new List<TeamStandingUI>();
    if (standings == null) return list;

    var ts = standings.GetTeamStandingsSorted();
    for (int i = 0; i < ts.Count; i++)
    {
        var t = ts[i];
        string name = (t.team != null) ? t.team.displayName : t.teamId;

        list.Add(new TeamStandingUI
        {
            id = t.teamId,
            name = name,
            points = t.points,
            wins = t.wins
        });
    }

    return list;
}

// ✅ ESSA é a ponte principal: UI chama isso e recebe results já "UI-ready".
public List<RaceEntryResult> SimulateRoundAndReturnResults(int round, int seed = 0)
{
    // Reaproveita sua SimulateRound, mas você precisa retornar a lista do simulador.
    // Se sua SimulateRound já gera "results" dentro dela, copie a parte e retorne.

    if (seasonCalendar2026 == null)
    {
        Debug.LogError("[Sim] seasonCalendar2026 is null.");
        return new List<RaceEntryResult>();
    }

    var weekend = seasonCalendar2026.GetWeekendByRound(round);
    if (weekend == null || weekend.track == null)
    {
        Debug.LogError($"[Sim] Round {round} not found or track is null.");
        return new List<RaceEntryResult>();
    }

    if (driversDatabase2026 == null)
    {
        Debug.LogError("[Sim] driversDatabase2026 is null.");
        return new List<RaceEntryResult>();
    }

    if (teams == null || teams.Count == 0)
    {
        Debug.LogError("[Sim] Teams cache is empty.");
        return new List<RaceEntryResult>();
    }

    // chama seu simulador real:
    var allDrivers = driversDatabase2026.Drivers;

    // ⚠️ Ajuste aqui se seu simulador retorna outro tipo.
    // A ideia: converter para List<RaceEntryResult>.
    var sim = SimpleRaceSimulator.SimulateWeekend(
        weekend: weekend,
        teams: teams,
        allDrivers: allDrivers,
        seasonYear: currentSeason,
        seed: seed
    );

    // Converter do tipo que você já tem em "sim" para RaceEntryResult:
    // Vou assumir que sim é uma lista de entries com: gridPos, finishPos, driver, team, dnf(optional)
    var results = new List<RaceEntryResult>();

    var race = sim.sessions
    .First(s => s.type == SimpleRaceSimulator.SessionType.Race)
    .classification;

foreach (var e in race)
{
    string dId   = e.driver != null ? e.driver.driverId : "";
    string dName = e.driver != null ? e.driver.displayName : "(driver)";
    string tId   = e.team != null ? e.team.teamId : "";
    string tName = e.team != null ? e.team.displayName : "(team)";

    bool dnf = false;
    // futuramente:
    // dnf = e.dnf;

    results.Add(new RaceEntryResult(
        e.gridPos,
        e.finishPos,
        dId,
        dName,
        tId,
        tName,
        dnf
    ));
}


    // Pontos / standings (reusa tua lógica)
if (pointsRuleset2026 != null)
{
    if (standings == null) standings = new ChampionshipStandings();

    foreach (var e in sim.entries.OrderBy(x => x.finishPos))
    {
        int pts = pointsRuleset2026.GetRacePointsForPosition(e.finishPos);
        standings.AwardPointsToDriver(e.driver, e.team, pts, e.finishPos);
    }
}


    // Wear + driver systems tick (se você já faz na SimulateRound, pode manter lá e remover daqui)
    if (initializeCarAndEconomyOnStart) ApplyWearForAllTeamsAfterRound();
    AdvanceDriverSystemsWeek();

    return results;
}

}
