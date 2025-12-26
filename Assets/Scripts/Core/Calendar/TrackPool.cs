using System.Collections.Generic;
using UnityEngine;
using F1Manager.Data;

namespace F1Manager.Core.Calendar
{
    [CreateAssetMenu(fileName = "TrackPool_", menuName = "F1 Manager/World/Track Pool", order = 10)]
    public class TrackPool : ScriptableObject
    {
        [Header("All Tracks Available For Calendar Generation")]
        [SerializeField] public List<TrackData> allTracks = new List<TrackData>();

        public IReadOnlyList<TrackData> AllTracks => allTracks;

        public void Validate()
        {
            if (allTracks == null) allTracks = new List<TrackData>();

            // Remove nulls
            allTracks.RemoveAll(t => t == null);

            // Warn duplicates by trackId
            var seen = new HashSet<string>();

            for (int i = 0; i < allTracks.Count; i++)
            {
                var t = allTracks[i];
                if (t == null) continue;

                // força validar TrackData se tiver OnValidate (já tem)
#if UNITY_EDITOR
                t.Validate();
#endif

                string id = (t.trackId ?? "").Trim();
                if (string.IsNullOrWhiteSpace(id))
                {
                    Debug.LogWarning($"[TrackPool] Track with EMPTY trackId detected in '{name}' at index {i} (asset: {t.name}).");
                    continue;
                }

                if (!seen.Add(id))
                {
                    Debug.LogWarning($"[TrackPool] DUPLICATE trackId '{id}' detected in '{name}'. Check index {i} (asset: {t.name}).");
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            Validate();
        }

        [ContextMenu("Validate / Remove Nulls + Warn Duplicates")]
        private void ValidateNow()
        {
            Validate();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
