using System.Collections.Generic;
using UnityEngine;

namespace F1Manager.Data
{
    [CreateAssetMenu(fileName = "SeasonCalendar_", menuName = "F1 Manager/Season/Season Calendar", order = 32)]
    public class SeasonCalendar : ScriptableObject
    {
        [Header("Season")]
        public int seasonYear = 2026;

        [Header("Weekends (Ordered)")]
        [SerializeField] private List<RaceWeekendData> weekends = new List<RaceWeekendData>();

        public IReadOnlyList<RaceWeekendData> Weekends => weekends;

        /// <summary>
        /// Round é 1-based: round 1 = index 0
        /// </summary>
        public RaceWeekendData GetWeekendByRound(int round)
        {
            if (weekends == null || weekends.Count == 0) return null;
            if (round < 1 || round > weekends.Count) return null;
            return weekends[round - 1];
        }

        public void Validate()
        {
            if (weekends == null) weekends = new List<RaceWeekendData>();

            // Detecta rounds duplicados (evita bugs na simulação/UI)
            var seenRounds = new HashSet<int>();

            for (int i = 0; i < weekends.Count; i++)
            {
                var w = weekends[i];
                if (w == null) continue;

                // Se round não foi preenchido, corrige pela ordem da lista
                if (w.round <= 0) w.round = i + 1;

                // Sanitiza
                w.round = Mathf.Max(1, w.round);

                // Se você quiser FORÇAR o calendário a ser sempre sequencial e travado:
                // w.round = i + 1;

                // Warn de round duplicado
                if (!seenRounds.Add(w.round))
                {
                    Debug.LogWarning($"[SeasonCalendar] Duplicate round '{w.round}' detected in '{name}'. Check element index {i}.");
                }

                // Isso chama SyncSessionsWithTrack() via w.Validate()
                w.Validate();

                // (Opcional) Garante sessions alinhadas sempre, mesmo se alguém editou a lista manualmente:
                // w.SyncSessionsWithTrack(forceRebuild: false);
            }

            // (Opcional) Warn se o número de weekends não bater com o esperado em 2026 (24)
            // if (seasonYear == 2026 && weekends.Count != 24)
            //     Debug.LogWarning($"[SeasonCalendar] Season {seasonYear} expected 24 weekends, but found {weekends.Count} in '{name}'.");
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            Validate();
        }
#endif
    }
}
