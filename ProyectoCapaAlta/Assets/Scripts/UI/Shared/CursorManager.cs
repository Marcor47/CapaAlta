using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    [Header("Cursor normal (pluma)")]
    public Texture2D defaultCursor;
    public Vector2 defaultHotspot = new Vector2(0f, 31f);   // la punta de la pluma

    [Header("Cursor al apuntar (opcional)")]
    public Texture2D aimCursor;
    public Vector2 aimHotspot = new Vector2(16f, 16f);      // centro de la mira

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SetDefault();
    }

    public void SetDefault() => Cursor.SetCursor(defaultCursor, defaultHotspot, CursorMode.Auto);

    public void SetAim() =>
        Cursor.SetCursor(aimCursor != null ? aimCursor : defaultCursor, aimHotspot, CursorMode.Auto);
}