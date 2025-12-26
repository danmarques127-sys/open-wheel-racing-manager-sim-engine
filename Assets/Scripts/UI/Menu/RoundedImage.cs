using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class RoundedImage : MonoBehaviour
{
    [Header("Shape")]
    [Range(2, 128)] public int radius = 32;
    [Range(64, 512)] public int textureSize = 256;

    [Header("Behavior")]
    public bool regenerateInEditor = true;

    private Image _img;
    private Sprite _sprite;

    private void OnEnable()
    {
        _img = GetComponent<Image>();
        Apply();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!regenerateInEditor) return;
        if (!isActiveAndEnabled) return;
        _img = GetComponent<Image>();
        Apply();
    }
#endif

    private void Apply()
    {
        if (_img == null) _img = GetComponent<Image>();

        // evita recriar a cada frame no Editor sem necessidade
        int size = Mathf.Clamp(textureSize, 64, 512);
        int r = Mathf.Clamp(radius, 2, size / 2);

        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        // sprite branco com alpha recortado (cantos arredondados)
        Color32 white = new Color32(255, 255, 255, 255);
        Color32 transparent = new Color32(255, 255, 255, 0);

        int rr = r * r;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool inside = true;

                // testa 4 cantos (círculos)
                if (x < r && y < r) // bottom-left
                {
                    int dx = r - 1 - x;
                    int dy = r - 1 - y;
                    inside = (dx * dx + dy * dy) <= rr;
                }
                else if (x >= size - r && y < r) // bottom-right
                {
                    int dx = x - (size - r);
                    int dy = r - 1 - y;
                    inside = (dx * dx + dy * dy) <= rr;
                }
                else if (x < r && y >= size - r) // top-left
                {
                    int dx = r - 1 - x;
                    int dy = y - (size - r);
                    inside = (dx * dx + dy * dy) <= rr;
                }
                else if (x >= size - r && y >= size - r) // top-right
                {
                    int dx = x - (size - r);
                    int dy = y - (size - r);
                    inside = (dx * dx + dy * dy) <= rr;
                }

                tex.SetPixel(x, y, inside ? white : transparent);
            }
        }

        tex.Apply();

        // borda para 9-slice = radius
        Vector4 border = new Vector4(r, r, r, r);

        // cria sprite
        _sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);

        _img.sprite = _sprite;
        _img.type = Image.Type.Sliced;
    }
}
