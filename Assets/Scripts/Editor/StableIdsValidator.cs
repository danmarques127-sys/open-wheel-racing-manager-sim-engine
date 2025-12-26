#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using F1Manager.Data;

public static class StableIdsValidator
{
    [MenuItem("F1 Manager/Validate/Validate Stable IDs (Drivers/Teams/Tracks)")]
    public static void ValidateStableIds()
    {
        int errors = 0;

        errors += ValidateAssetsOfType<DriverData>("DriverData");
        errors += ValidateAssetsOfType<TeamData>("TeamData");
        errors += ValidateAssetsOfType<TrackData>("TrackData");

        if (errors == 0)
            Debug.Log("✅ Stable IDs: OK (no duplicates / missing IDs).");
        else
            Debug.LogError($"❌ Stable IDs: Found {errors} issues. Check Console.");
    }

    private static int ValidateAssetsOfType<T>(string label) where T : ScriptableObject
    {
        var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        var seen = new HashSet<string>();
        int issues = 0;

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) continue;

            // Reflection read "id"
            var field = typeof(T).GetField("id");
            if (field == null)
            {
                Debug.LogWarning($"[{label}] No 'id' field found on type {typeof(T).Name}. File: {path}");
                continue;
            }

            var id = field.GetValue(asset) as string;

            if (string.IsNullOrWhiteSpace(id))
            {
                issues++;
                Debug.LogError($"[{label}] Missing id on: {path}", asset);
                continue;
            }

            if (!seen.Add(id))
            {
                issues++;
                Debug.LogError($"[{label}] Duplicate id '{id}' found. Asset: {path}", asset);
            }
        }

        return issues;
    }
}
#endif
