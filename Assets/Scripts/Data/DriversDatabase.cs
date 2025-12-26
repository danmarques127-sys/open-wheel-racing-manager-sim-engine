using System.Collections.Generic;
using UnityEngine;

namespace F1Manager.Data
{
    [CreateAssetMenu(fileName = "DriversDatabase_", menuName = "F1 Manager/Database/Drivers Database")]
    public class DriversDatabase : ScriptableObject
    {
        // ============================================================
        // SOURCE OF TRUTH (EDITABLE IN INSPECTOR)
        // ============================================================

        [Header("Drivers (editável no Inspector)")]
        [Tooltip("Arraste aqui os DriverData (pilotos) para compor o database.")]
        public List<DriverData> drivers = new List<DriverData>();

        // ✅ Compat: mantém API antiga "Drivers" (somente leitura)
        public IReadOnlyList<DriverData> Drivers => drivers;

        public int Count => drivers != null ? drivers.Count : 0;

        // ============================================================
        // HELPERS
        // ============================================================

        private static string Norm(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? "" : s.Trim().ToLowerInvariant();
        }

        private static bool SameId(string a, string b)
        {
            return Norm(a) == Norm(b);
        }

        // ============================================================
        // QUERIES
        // ============================================================

        public DriverData GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            if (drivers == null || drivers.Count == 0) return null;

            string key = Norm(id);

            for (int i = 0; i < drivers.Count; i++)
            {
                var d = drivers[i];
                if (d == null) continue;

                if (SameId(d.driverId, key))
                    return d;
            }

            return null;
        }

        public List<DriverData> GetByTeam(string teamId, bool includeReserves = true, bool includeTestDrivers = true)
        {
            var result = new List<DriverData>();
            if (string.IsNullOrWhiteSpace(teamId)) return result;
            if (drivers == null || drivers.Count == 0) return result;

            string key = Norm(teamId);

            for (int i = 0; i < drivers.Count; i++)
            {
                var d = drivers[i];
                if (d == null) continue;

                // contract pode ser null em assets antigos
                if (d.contract == null) continue;

                // ✅ Regra definitiva: se não tem teamId no contrato, não pertence a time nenhum
                string driverTeam = Norm(d.contract.teamId);
                if (string.IsNullOrWhiteSpace(driverTeam)) continue;

                // ✅ comparação normalizada (corrige case/trim)
                if (driverTeam != key) continue;

                // Filtra por roles (novo)
                if (!includeReserves && d.role != DriverRole.Starter)
                    continue;

                if (!includeTestDrivers && d.role == DriverRole.Test)
                    continue;

                // Opcional: excluir suspensos/aposentados automaticamente
                // if (d.careerStatus != DriverCareerStatus.Active) continue;

                result.Add(d);
            }

            return result;
        }

        public List<DriverData> GetFreeAgents(bool includeInactiveCareer = false)
        {
            var result = new List<DriverData>();
            if (drivers == null || drivers.Count == 0) return result;

            for (int i = 0; i < drivers.Count; i++)
            {
                var d = drivers[i];
                if (d == null) continue;

                // ✅ FreeAgent baseado em teamId vazio (mais confiável)
                if (d.contract == null) continue;

                string driverTeam = Norm(d.contract.teamId);
                bool freeAgent = string.IsNullOrWhiteSpace(driverTeam);

                if (!freeAgent) continue;

                if (!includeInactiveCareer && d.careerStatus != DriverCareerStatus.Active)
                    continue;

                result.Add(d);
            }

            return result;
        }

#if UNITY_EDITOR
        // ============================================================
        // EDITOR QA / FIXERS
        // ============================================================

        [ContextMenu("Validate / Remove Null Entries")]
        private void RemoveNullEntries()
        {
            if (drivers == null) drivers = new List<DriverData>();
            drivers.RemoveAll(d => d == null);
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log($"[DriversDatabase] Removed null entries. Count={drivers.Count}");
        }

        [ContextMenu("Fix/Normalize DriverIds and Contract TeamIds (trim+lower)")]
        private void NormalizeIdsAndContracts()
        {
            if (drivers == null) drivers = new List<DriverData>();

            int changed = 0;

            for (int i = 0; i < drivers.Count; i++)
            {
                var d = drivers[i];
                if (d == null) continue;

                string beforeDriverId = d.driverId;

                // Normaliza driverId
                d.driverId = Norm(d.driverId);

                // Garante contract
                if (d.contract == null)
                    d.contract = new DriverContract();

                string beforeTeam = d.contract.teamId;

                // Normaliza teamId do contrato
                d.contract.teamId = Norm(d.contract.teamId);

                // Se tem teamId, força Signed; se vazio, FreeAgent
                if (string.IsNullOrWhiteSpace(d.contract.teamId))
                    d.contract.status = ContractStatus.FreeAgent;
                else
                    d.contract.status = ContractStatus.Signed;

                // Se mudou algo, marca o asset do driver
                if (beforeDriverId != d.driverId || beforeTeam != d.contract.teamId)
                {
                    changed++;
                    UnityEditor.EditorUtility.SetDirty(d);
                }
            }

            // remove nulls
            int before = drivers.Count;
            drivers.RemoveAll(x => x == null);
            int removed = before - drivers.Count;

            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();

            Debug.Log($"[DriversDatabase] Normalize complete. Changed={changed}, RemovedNulls={removed}, Count={drivers.Count}");
        }

        private void OnValidate()
        {
            if (drivers == null) drivers = new List<DriverData>();
            drivers.RemoveAll(d => d == null);
        }
#endif
    }
}
