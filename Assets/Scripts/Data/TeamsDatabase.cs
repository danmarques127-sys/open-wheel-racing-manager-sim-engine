using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TeamsDatabase", menuName = "F1 Manager/Data/Teams Database", order = 2)]
public class TeamsDatabase : ScriptableObject
{
    public List<TeamData> teams = new List<TeamData>();

    public TeamData GetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        id = id.Trim().ToLowerInvariant();

        foreach (var t in teams)
        {
            if (t != null && !string.IsNullOrWhiteSpace(t.teamId) && t.teamId.Trim().ToLowerInvariant() == id)
                return t;
        }
        return null;
    }
}
