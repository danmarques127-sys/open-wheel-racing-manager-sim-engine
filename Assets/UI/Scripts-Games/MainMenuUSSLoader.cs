using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MainMenuUSSLoader : MonoBehaviour
{
    [SerializeField] private StyleSheet mainMenuUSS;

    private void OnEnable()
    {
        var doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        if (root == null)
        {
            Debug.LogError("UIDocument rootVisualElement is null.");
            return;
        }

        if (mainMenuUSS == null)
        {
            Debug.LogError("Assign MainMenu.uss in Inspector.");
            return;
        }

        if (!root.styleSheets.Contains(mainMenuUSS))
            root.styleSheets.Add(mainMenuUSS);
    }
}
