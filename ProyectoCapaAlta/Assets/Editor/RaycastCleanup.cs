#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class RaycastCleanup
{
    static readonly string[] keep = { "PausePanel", "PauseFondo" }; // fondos que bloquean clics

    [MenuItem("Capa Alta/Limpiar Raycast Target (escena abierta)")]
    static void Run()
    {
        int n = 0;
        foreach (var g in Object.FindObjectsByType<Graphic>(FindObjectsInactive.Include))
        {
            if (!g.raycastTarget) continue;
            if (System.Array.IndexOf(keep, g.name) >= 0) continue;
            var sel = g.GetComponentInParent<Selectable>(true);
            if (sel != null && sel.targetGraphic == g) continue;          // imagen del botón
            if (g.GetComponentInParent<TMPro.TMP_InputField>(true) != null) continue;
            if (g.GetComponentInParent<ScrollRect>(true) != null && g.GetComponent<Mask>() != null) continue;
            Undo.RecordObject(g, "Raycast");
            g.raycastTarget = false;
            EditorUtility.SetDirty(g);
            n++;
        }
        Debug.Log($"Raycast Target desactivado en {n} elementos");
    }
}
#endif