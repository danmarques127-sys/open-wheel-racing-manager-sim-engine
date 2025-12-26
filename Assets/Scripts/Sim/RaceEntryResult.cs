using System;

[Serializable]
public class RaceEntryResult
{
    public int gridPos;
    public int finishPos;
    public string driverId;
    public string driverName;
    public string teamId;
    public string teamName;
    public bool dnf;

    public RaceEntryResult() { }

    public RaceEntryResult(int grid, int finish, string dId, string dName, string tId, string tName, bool dnf = false)
    {
        gridPos = grid;
        finishPos = finish;
        driverId = dId;
        driverName = dName;
        teamId = tId;
        teamName = tName;
        this.dnf = dnf;
    }
}
