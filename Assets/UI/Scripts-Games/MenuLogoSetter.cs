using UnityEngine;
using UnityEngine.UIElements;

public class MenuLogoSetter : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private Sprite logoSprite;

    private void OnEnable()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

        if (uiDocument == null)
        {
            Debug.LogError("[MenuLogoSetter] UIDocument não encontrado.");
            return;
        }

        if (logoSprite == null)
        {
            Debug.LogError("[MenuLogoSetter] Logo Sprite não atribuído no Inspector.");
            return;
        }

        var root = uiDocument.rootVisualElement;
        var logo = root.Q<Image>("logoImage");

        if (logo == null)
        {
            Debug.LogError("[MenuLogoSetter] Não achei Image com name='logoImage' no UXML.");
            return;
        }

        logo.sprite = logoSprite;
        logo.scaleMode = ScaleMode.ScaleToFit;
    }
}
