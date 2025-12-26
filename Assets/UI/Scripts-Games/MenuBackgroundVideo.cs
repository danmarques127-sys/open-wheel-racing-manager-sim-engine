using UnityEngine;
using UnityEngine.UIElements;

public class MenuBackgroundVideo : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private RenderTexture videoRT;

    private void OnEnable()
    {
        if (uiDocument == null)
        {
            Debug.LogError("[MenuBackgroundVideo] UIDocument não foi atribuído no Inspector.");
            return;
        }

        if (videoRT == null)
        {
            Debug.LogError("[MenuBackgroundVideo] RenderTexture (videoRT) não foi atribuída no Inspector.");
            return;
        }

        var root = uiDocument.rootVisualElement;

        // 1) Ligar o RenderTexture no Image do vídeo
        var video = root.Q<Image>("VideoBG");
        if (video != null)
        {
            video.image = videoRT;
            video.pickingMode = PickingMode.Ignore; // não roubar clique
        }
        else
        {
            Debug.LogError("[MenuBackgroundVideo] Não achei Image com name=\"VideoBG\" no UXML.");
        }

        // 2) Camadas que NÃO podem roubar clique
        IgnoreByName(root, "VideoOverlay");
        IgnoreByClass(root, "telemetry-grid");
        IgnoreByClass(root, "vignette");
        IgnoreByClass(root, "gloss");
        IgnoreByClass(root, "bg"); // se você não usar, não faz mal

        // 3) garante que seus itens do menu cliquem
        MakeClickable(root, "btnNewGame");
        MakeClickable(root, "btnLoadGame");
        MakeClickable(root, "btnSettings");
        MakeClickable(root, "btnCredits");
        MakeClickable(root, "btnQuit");
    }

    private void IgnoreByName(VisualElement root, string name)
    {
        var el = root.Q<VisualElement>(name);
        if (el != null) el.pickingMode = PickingMode.Ignore;
    }

    private void IgnoreByClass(VisualElement root, string className)
    {
        var list = root.Query<VisualElement>(className: className).ToList();
        foreach (var el in list)
            el.pickingMode = PickingMode.Ignore;
    }

    private void MakeClickable(VisualElement root, string name)
    {
        var el = root.Q<VisualElement>(name);
        if (el == null)
        {
            Debug.LogWarning($"[MenuBackgroundVideo] Não achei elemento com name=\"{name}\" no UXML.");
            return;
        }

        el.pickingMode = PickingMode.Position; // garante que receba click
        el.RegisterCallback<ClickEvent>(_ => Debug.Log($"Clicked: {name}"));
    }
}
