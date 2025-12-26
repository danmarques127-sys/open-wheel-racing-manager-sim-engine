using System.Collections.Generic;
using UnityEngine;
using F1Manager.Data; // onde está o DriverData

[CreateAssetMenu(
    fileName = "TeamProfile_",
    menuName = "F1 Manager/UI/Team Profile Data",
    order = 10
)]
public class TeamProfileData : ScriptableObject
{
    [Header("Identity")]
    public string teamId;                 // ex: "ferrari"
    public string teamName;               // ex: "Scuderia Ferrari"
    public Sprite teamLogo;

    [Header("Theme")]
    public Color primaryColor = Color.red;
    public Color secondaryColor = Color.black;
    [Range(0f, 1f)] public float themeIntensity = 0.85f;

    [Header("Story")]
    [TextArea(6, 20)]
    public string shortHistory;

    [Header("Titles")]
    public int constructorsTitles;
    public int driversTitles;

    [Header("Current Drivers (References)")]
    public List<DriverData> currentDrivers = new List<DriverData>();
}
