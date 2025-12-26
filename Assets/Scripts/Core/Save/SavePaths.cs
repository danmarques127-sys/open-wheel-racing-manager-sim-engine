using System.IO;
using UnityEngine;

namespace F1Manager.Core.Save
{
    public static class SavePaths
    {
        public static string RootFolder
        {
            get
            {
                string p = Path.Combine(Application.persistentDataPath, "F1ManagerSaves");
                if (!Directory.Exists(p)) Directory.CreateDirectory(p);
                return p;
            }
        }

        public static string GetSaveFile(string slotName)
        {
            if (string.IsNullOrEmpty(slotName)) slotName = "slot1";
            return Path.Combine(RootFolder, $"{slotName}.json");
        }
    }
}
