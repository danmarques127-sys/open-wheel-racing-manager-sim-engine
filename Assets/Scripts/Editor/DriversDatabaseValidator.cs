#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using F1Manager.Data;

public static class DriversDatabaseValidator
{
    private static string Norm(string s) => string.IsNullOrWhiteSpace(s) ? "" : s.Trim().ToLowerInvariant();

    [MenuItem("F1 Manager/Validate/DriversDatabase_2026 (with Team Link Check)")]
    public static void ValidateDriversDb()
    {
        // Ajuste paths se necessário
        const string driversDbPath = "Assets/Data/DriversDatabase_2026.asset";
        const string teamsDbPath   = "Assets/Data/TeamsDatabase_2026.asset";

        var driversDb = AssetDatabase.LoadAssetAtPath<DriversDatabase>(driversDbPath);
        if (driversDb == null)
        {
            Debug.LogError($"[VALIDATOR] DriversDatabase não encontrado em: {driversDbPath}");
            return;
        }

        var teamsDb = AssetDatabase.LoadAssetAtPath<TeamsDatabase>(teamsDbPath);
        if (teamsDb == null)
        {
            Debug.LogError($"[VALIDATOR] TeamsDatabase não encontrado em: {teamsDbPath}");
            return;
        }

        // -------------------------
        // Build team id set + checks
        // -------------------------
        int teamErrors = 0;
        var teamIds = new HashSet<string>();
        var dupTeams = new List<string>();

        foreach (var t in teamsDb.teams)
        {
            if (t == null) { teamErrors++; Debug.LogError("[VALIDATOR] Team NULL dentro do TeamsDatabase"); continue; }

            var tid = Norm(t.teamId);
            if (string.IsNullOrWhiteSpace(tid))
            {
                teamErrors++;
                Debug.LogError($"[VALIDATOR] Team com teamId vazio: {t.name} / display='{t.displayName}'");
                continue;
            }

            if (!teamIds.Add(tid))
                dupTeams.Add(tid);
        }

        if (dupTeams.Count > 0)
        {
            teamErrors++;
            Debug.LogError($"[VALIDATOR] teamId duplicados no TeamsDatabase: {string.Join(", ", dupTeams)}");
        }

        Debug.Log($"[VALIDATOR] TeamsDatabase -> Teams={teamsDb.teams.Count} | UniqueTeamIds={teamIds.Count} | TeamErrors={teamErrors}");

        // -------------------------
        // Drivers validation
        // -------------------------
        int errors = 0;
        int warnings = 0;

        var idSet = new HashSet<string>();
        var duplicateIds = new List<string>();

        int contracted = 0;
        int freeAgents = 0;
        int invalidTeamLinks = 0;

        for (int i = 0; i < driversDb.Drivers.Count; i++)
        {
            var d = driversDb.Drivers[i];

            if (d == null)
            {
                errors++;
                Debug.LogError($"[VALIDATOR] Driver NULL no index {i} do DriversDatabase.");
                continue;
            }

            // driverId
            string didRaw = d.driverId ?? "";
            string did = didRaw.Trim();
            if (string.IsNullOrWhiteSpace(did))
            {
                errors++;
                Debug.LogError($"[VALIDATOR] driverId vazio em asset: {d.name}");
            }
            else
            {
                string norm = Norm(did);
                if (!idSet.Add(norm))
                    duplicateIds.Add(norm);
            }

            // Ratings sanity
            if (d.overall <= 0 || d.racePace <= 0 || d.qualifying <= 0 || d.consistency <= 0 ||
                d.aggression <= 0 || d.tireManagement <= 0 || d.wetSkill <= 0)
            {
                errors++;
                Debug.LogError($"[VALIDATOR] Ratings inválidos (<=0) em: {d.name} ({did})");
            }

            // Contract
            if (d.contract == null)
            {
                errors++;
                Debug.LogError($"[VALIDATOR] Contract NULL em: {d.name} ({did})");
                continue;
            }

            // FreeAgent vs Signed count
            if (d.IsFreeAgent) freeAgents++;
            else contracted++;

            // 🔥 LINK CHECK: se não é free agent, teamId deve existir no TeamsDatabase
            if (!d.IsFreeAgent)
            {
                string teamId = Norm(d.contract.teamId);

                if (string.IsNullOrWhiteSpace(teamId))
                {
                    errors++;
                    Debug.LogError($"[VALIDATOR] Contratado mas contract.teamId vazio: {d.name} ({did})");
                }
                else if (!teamIds.Contains(teamId))
                {
                    invalidTeamLinks++;
                    errors++;
                    Debug.LogError($"[VALIDATOR] TeamId NÃO EXISTE no TeamsDatabase: Driver={d.name} ({did}) contract.teamId='{d.contract.teamId}' (norm='{teamId}')");
                }

                // Years checks (se quiser manter)
                if (d.contract.contractStartYear <= 0 || d.contract.contractEndYear <= 0)
                {
                    warnings++;
                    Debug.LogWarning($"[VALIDATOR] Start/End year (<=0) em: {d.name} ({did}) start={d.contract.contractStartYear} end={d.contract.contractEndYear}");
                }
                else if (d.contract.contractEndYear < d.contract.contractStartYear)
                {
                    errors++;
                    Debug.LogError($"[VALIDATOR] EndYear < StartYear em: {d.name} ({did})");
                }
            }
        }

        if (duplicateIds.Count > 0)
        {
            errors++;
            Debug.LogError($"[VALIDATOR] driverId duplicados: {string.Join(", ", duplicateIds)}");
        }

        Debug.Log($"[VALIDATOR] DriversDatabase_2026 -> Errors={errors} | Warnings={warnings} | Total={driversDb.Drivers.Count} | Contracted={contracted} | FreeAgents={freeAgents} | InvalidTeamLinks={invalidTeamLinks}");
    }
}
#endif
