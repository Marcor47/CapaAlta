using System;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class ResizableZone : MonoBehaviour
{
    [SerializeField] SpriteRenderer body;   // Cuerpo
    [SerializeField] SpriteRenderer cap;    // Cima (Muro) o Superficie (Agua); opcional
    [SerializeField] bool snapToTileX = false;   // true = el ancho salta de baldosa en baldosa
    [SerializeField] bool snapToTileY = false;   // true = el alto salta de baldosa en baldosa

    void Awake() { if (Application.isPlaying) GetComponent<SpriteRenderer>().enabled = false; }

#if UNITY_EDITOR
    void OnEnable() { UnityEditor.SceneView.duringSceneGui += OnSceneGUI; }
    void OnDisable() { UnityEditor.SceneView.duringSceneGui -= OnSceneGUI; }
    void OnValidate() { UnityEditor.EditorApplication.delayCall += () => { if (this) Apply(); }; }
    void Update() { if (!Application.isPlaying) Apply(); }

    void OnSceneGUI(UnityEditor.SceneView view)
    {
        Event e = Event.current;
        if (e != null && e.rawType == EventType.MouseUp && e.button == 0 &&
            Array.IndexOf(UnityEditor.Selection.gameObjects, gameObject) >= 0)
            UnityEditor.EditorApplication.delayCall += () => { if (this) SnapToModule(); };
    }

    float CapHeight()
    {
        if (cap == null) return 0f;
        return cap.drawMode == SpriteDrawMode.Simple ? cap.localBounds.size.y : cap.size.y;
    }

    [ContextMenu("Ajustar tamaño")]
    void SnapToModule()
    {
        var d = GetComponent<SpriteRenderer>();
        if (d == null || body == null || body.sprite == null) return;

        float px = 1f / body.sprite.pixelsPerUnit;                    // 1 píxel en unidades
        Vector2 step = body.sprite.bounds.size;                       // tamaño de una baldosa
        Vector4 br = body.sprite.border / body.sprite.pixelsPerUnit;  // x izq, z der
        float baseX = br.x + br.z;
        step.x -= baseX;                                              // ancho del centro repetible
        float capH = CapHeight();
        Vector2 s = d.size;

        float w;
        if (snapToTileX && step.x > 0f)
            w = baseX + Mathf.Max(1f, Mathf.Round((s.x - baseX) / step.x)) * step.x;
        else
            w = Mathf.Max(baseX + px, Mathf.Round(s.x / px) * px);

        float bh;
        if (snapToTileY)
            bh = Mathf.Max(step.y, Mathf.Round((s.y - capH) / step.y) * step.y);
        else
            bh = Mathf.Max(px, Mathf.Round((s.y - capH) / px) * px);

        Vector2 snapped = new Vector2(w, bh + capH);
        if ((snapped - s).sqrMagnitude < 1e-8f) return;

        UnityEditor.Undo.RecordObject(d, "Ajustar tamaño");
        UnityEditor.Undo.RecordObject(transform, "Ajustar tamaño");
        Vector3 oldMin = d.localBounds.min;
        d.size = snapped;
        Vector3 newMin = d.localBounds.min;
        transform.position += transform.TransformVector(oldMin - newMin); // esquina inferior izquierda fija
        Dirty(d);
        Dirty(transform);
        Apply();
    }

    void Apply()
    {
        var driver = GetComponent<SpriteRenderer>();
        var box = GetComponent<BoxCollider2D>();
        if (driver == null || box == null || body == null) return;

        Bounds b = driver.localBounds;
        float capH = CapHeight();
        float bodyH = Mathf.Max(0f, b.size.y - capH);

        if (((Vector2)box.size - (Vector2)b.size).sqrMagnitude > 1e-6f ||
            (box.offset - (Vector2)b.center).sqrMagnitude > 1e-6f)
        {
            box.size = b.size;
            box.offset = b.center;
            Dirty(box);
        }

        Fit(body, new Vector2(b.size.x, bodyH), new Vector2(b.center.x, b.min.y + bodyH * 0.5f));
        if (cap != null)
            Fit(cap, new Vector2(b.size.x, capH), new Vector2(b.center.x, b.max.y - capH * 0.5f));
    }

    static void Fit(SpriteRenderer r, Vector2 size, Vector2 centerInParent)
    {
        if (r.drawMode != SpriteDrawMode.Simple && (r.size - size).sqrMagnitude > 1e-6f)
        {
            r.size = size;
            Dirty(r);
        }
        Vector3 p = r.transform.localPosition;
        Vector2 want = centerInParent - (Vector2)r.localBounds.center;
        if (((Vector2)p - want).sqrMagnitude > 1e-8f)
        {
            r.transform.localPosition = new Vector3(want.x, want.y, p.z);
            Dirty(r.transform);
        }
    }

    static void Dirty(UnityEngine.Object o)
    {
        UnityEditor.EditorUtility.SetDirty(o);
        UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(o);
    }
#endif
}