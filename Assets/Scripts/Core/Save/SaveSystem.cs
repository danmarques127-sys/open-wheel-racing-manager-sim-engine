using System;
using System.IO;
using UnityEngine;

namespace F1Manager.Core.Save
{
    public static class SaveSystem
    {
        private const string DefaultFileName = "savegame.json";

        public static string GetSavePath(string fileName = null)
        {
            fileName ??= DefaultFileName;
            return Path.Combine(Application.persistentDataPath, fileName);
        }

        public static void Save(SaveGameData data, string fileName = null)
        {
            if (data == null)
            {
                Debug.LogError("SaveSystem.Save: data is null");
                return;
            }

            try
            {
                string path = GetSavePath(fileName);
                string json = JsonUtility.ToJson(data, prettyPrint: true);
                File.WriteAllText(path, json);
                Debug.Log($"✅ Saved: {path}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Save failed: {ex}");
            }
        }

        public static SaveGameData Load(string fileName = null)
        {
            try
            {
                string path = GetSavePath(fileName);
                if (!File.Exists(path))
                {
                    Debug.LogWarning($"No save found at: {path}");
                    return null;
                }

                string json = File.ReadAllText(path);
                var data = JsonUtility.FromJson<SaveGameData>(json);
                Debug.Log($"✅ Loaded: {path}");
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Load failed: {ex}");
                return null;
            }
        }

        public static void Delete(string fileName = null)
        {
            try
            {
                string path = GetSavePath(fileName);
                if (File.Exists(path))
                {
                    File.Delete(path);
                    Debug.Log($"🗑️ Deleted save: {path}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Delete save failed: {ex}");
            }
        }
    }
}
