using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Scenes")]
    [Tooltip("Nome da cena para Team Select (ex: TeamSelect). Deixe vazio se ainda não existir.")]
    [SerializeField] private string teamSelectSceneName = "TeamSelect";

    [Tooltip("Nome da cena para Load Game (opcional).")]
    [SerializeField] private string loadGameSceneName = "";

    [Header("Optional")]
    [SerializeField] private StyleSheet extraStyleSheet; // se quiser adicionar outro

    private VisualElement _root;

    private void Reset()
    {
        uiDocument = GetComponent<UIDocument>();
    }

    private void Awake()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError("MainMenuUIController: UIDocument not found on this GameObject.");
            enabled = false;
            return;
        }
    }

    private void OnEnable()
    {
        // rootVisualElement só fica disponível quando o painel está pronto
        _root = uiDocument.rootVisualElement;

        if (_root == null)
        {
            Debug.LogError("MainMenuUIController: rootVisualElement is null (Panel not ready).");
            return;
        }

        if (extraStyleSheet != null && !_root.styleSheets.Contains(extraStyleSheet))
            _root.styleSheets.Add(extraStyleSheet);

        HookMenu();
    }

    private void HookMenu()
    {
        // Esses names precisam bater com o UXML:
        // btnNewGame, btnLoadGame, btnSettings, btnCredits, btnQuit
        BindClickable("btnNewGame", OnNewGame);
        BindClickable("btnLoadGame", OnLoadGame);
        BindClickable("btnSettings", OnSettings);
        BindClickable("btnCredits", OnCredits);
        BindClickable("btnQuit", OnQuit);
    }

    private void BindClickable(string elementName, System.Action action)
    {
        var el = _root.Q<VisualElement>(elementName);

        if (el == null)
        {
            Debug.LogWarning($"MainMenuUIController: Element '{elementName}' not found in UXML.");
            return;
        }

        // Evita registrar múltiplas vezes se reativar o objeto
        el.UnregisterCallback<ClickEvent>(OnClickProxy);

        // Guardamos a action no userData
        el.userData = action;

        // Torna focável (bom pra navegação)
        el.focusable = true;

        el.RegisterCallback<ClickEvent>(OnClickProxy);
        el.RegisterCallback<NavigationSubmitEvent>(OnSubmitProxy);
    }

    private void OnClickProxy(ClickEvent evt)
    {
        if (evt.currentTarget is VisualElement el && el.userData is System.Action action)
            action.Invoke();
    }

    private void OnSubmitProxy(NavigationSubmitEvent evt)
    {
        if (evt.currentTarget is VisualElement el && el.userData is System.Action action)
            action.Invoke();
    }

    private void OnNewGame()
    {
        // Vai pra Team Select (se a cena existir no Build Settings)
        TryLoadScene(teamSelectSceneName);
    }

    private void OnLoadGame()
    {
        if (string.IsNullOrWhiteSpace(loadGameSceneName))
        {
            Debug.Log("Load Game: scene not configured yet.");
            return;
        }

        TryLoadScene(loadGameSceneName);
    }

    private void OnSettings()
    {
        Debug.Log("Settings: TODO");
        // futuramente: abrir painel Settings dentro do mesmo UI (sem trocar cena)
    }

    private void OnCredits()
    {
        Debug.Log("Credits: TODO");
    }

    private void OnQuit()
    {
#if UNITY_EDITOR
        Debug.Log("Quit (Editor): stop play mode.");
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void TryLoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("TryLoadScene: sceneName is empty.");
            return;
        }

        // Verifica se existe nas cenas do Build Settings sem usar índices perigosos
        if (!IsSceneInBuild(sceneName))
        {
            Debug.LogError($"Scene '{sceneName}' not found in Build Settings. Add it to File > Build Profiles (ou Build Settings).");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    private static bool IsSceneInBuild(string sceneName)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            if (string.IsNullOrEmpty(path)) continue;

            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName) return true;
        }
        return false;
    }
}
