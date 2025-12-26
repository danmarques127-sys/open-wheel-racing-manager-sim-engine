using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class TeamSelectController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Database")]
    [SerializeField] private TeamProfileData[] teams;

    [Header("Scenes")]
    [SerializeField] private string backSceneName = "MainMenu_UI";
    [SerializeField] private string profileSceneName = "TeamProfile";

    private void OnEnable()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) { Debug.LogError("[TeamSelect] UIDocument não encontrado."); return; }

        var root = uiDocument.rootVisualElement;

        var teamsList = root.Q<VisualElement>("teamsList");
        var btnBack = root.Q<Button>("btnBack");

        if (teamsList == null) { Debug.LogError("[TeamSelect] Não achei 'teamsList' no UXML."); return; }
        if (btnBack != null) btnBack.clicked += () => SceneManager.LoadScene(backSceneName);

        teamsList.Clear();

        if (teams == null || teams.Length == 0)
        {
            Debug.LogError("[TeamSelect] Array 'teams' vazio. Arraste seus TeamProfileData no Inspector.");
            return;
        }

        foreach (var t in teams)
        {
            if (t == null) continue;

            var row = new VisualElement();
            row.AddToClassList("team-row");
            row.name = $"team_{t.teamId}";

            var left = new VisualElement();
            left.AddToClassList("team-left");

            var logo = new Image();
            logo.AddToClassList("team-logo");
            logo.sprite = t.teamLogo;
            logo.scaleMode = ScaleMode.ScaleToFit;

            var nameLabel = new Label(t.teamName);
            nameLabel.AddToClassList("team-name");

            left.Add(logo);
            left.Add(nameLabel);

            var cta = new Label("View profile →");
            cta.AddToClassList("team-cta");

            row.Add(left);
            row.Add(cta);

            // clique no card
            row.RegisterCallback<ClickEvent>(_ =>
            {
                GameSelection.SelectedTeamId = t.teamId;
                SceneManager.LoadScene(profileSceneName);
            });

            teamsList.Add(row);
        }
    }
}
