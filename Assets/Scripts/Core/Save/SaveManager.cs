using System.IO;
using UnityEngine;

namespace F1Manager.Core.Save
{
    public static class SaveManager
    {
        public static void Save(string slotName, SaveData data)
        {
            string path = SavePaths.GetSaveFile(slotName);
            string json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(path, json);
            Debug.Log($"[SaveManager] Saved: {path}");
        }

        public static SaveData LoadOrCreate(string slotName)
        {
            string path = SavePaths.GetSaveFile(slotName);
            if (!File.Exists(path))
            {
                Debug.Log($"[SaveManager] No save found. Creating new: {path}");
                var fresh = new SaveData { saveId = slotName };
                Save(slotName, fresh);
                return fresh;
            }

            string json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<SaveData>(json);
            if (data == null)
            {
                Debug.LogWarning("[SaveManager] Failed to parse save. Creating new.");
                data = new SaveData { saveId = slotName };
            }
            return data;
        }
    }
}
