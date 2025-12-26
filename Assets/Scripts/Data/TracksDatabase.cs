using System.Collections.Generic;
using UnityEngine;

namespace F1Manager.Data
{
    [CreateAssetMenu(fileName = "TracksDatabase_", menuName = "F1 Manager/Database/Tracks Database", order = 31)]
    public class TracksDatabase : ScriptableObject
    {
        [Header("Season / Label")]
        public int seasonYear = 2026;

        [Header("Tracks")]
        public List<TrackData> tracks = new List<TrackData>();

        public IReadOnlyList<TrackData> Tracks => tracks;

        public Dictionary<string, TrackData> BuildIndex()
        {
            var dict = new Dictionary<string, TrackData>();

            foreach (var t in tracks)
            {
                if (t == null) continue;
                t.Validate();

                if (string.IsNullOrWhiteSpace(t.trackId)) continue;

                if (!dict.ContainsKey(t.trackId))
                    dict.Add(t.trackId, t);
                else
                    Debug.LogWarning($"[TracksDatabase] Duplicate trackId '{t.trackId}' in {name}. Keeping first.");
            }

            return dict;
        }
    }
}
