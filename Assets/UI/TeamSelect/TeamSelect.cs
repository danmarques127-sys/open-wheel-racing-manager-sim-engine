using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class TeamSelect : MonoBehaviour
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
        if (uiDocument == null)
        {
            Debug.LogError("[TeamSelect] UIDocument não encontrado.");
            return;
        }

        var root = uiDocument.rootVisualElement;

        // No seu UXML crie:
        // - Um container com name="teamsList"
        // - Um botão com name="btnBack" (opcional)
        var teamsList = root.Q<VisualElement>("teamsList");
        var btnBack = root.Q<Button>("btnBack");

        if (btnBack != null) btnBack.clicked += () => SceneManager.LoadScene(backSceneName);

        if (teamsList == null)
        {
            Debug.LogError("[TeamSelect] Não achei 'teamsList' no UXML.");
            return;
        }

        teamsList.Clear();

        if (teams == null || teams.Length == 0)
        {
            Debug.LogError("[TeamSelect] Array 'teams' vazio. Arraste seus TeamProfileData no Inspector.");
            return;
        }

        foreach (var team in teams)
        {
            if (team == null) continue;

            // Botão por equipe
            var btn = new Button(() =>
            {
                GameSelection.SelectedTeamId = team.teamId;
                SceneManager.LoadScene(profileSceneName);
            });

            btn.text = string.IsNullOrWhiteSpace(team.teamName) ? team.teamId : team.teamName;
            btn.AddToClassList("team-card");

            teamsList.Add(btn);
        }
    }
}
