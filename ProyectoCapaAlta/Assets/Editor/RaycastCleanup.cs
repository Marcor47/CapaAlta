using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class RaycastCleanup
{
    // Paneles que deben seguir bloqueando clics (fondo del menú de pausa)
    static readonly HashSet<string> KeepByName = new HashSet<string> { "PausePanel", "PauseFondo" };

    [MenuItem("Capa Alta/Limpiar Raycast Target (escena abierta)")]
    static void Run()
    {
        var keep = new HashSet<Graphic>();
        int repaired = 0;

        // 1) Selectables: conservan el raycast de SU Target Graphic.
        //    Si está vacío o apunta a un objeto ajeno (típico al duplicar), se repara.
        foreach (var sel in Object.FindObjectsByType<Selectable>(FindObjectsInactive.Include))
        {
            if (!IsSceneObject(sel)) continue;

            Graphic target = sel.targetGraphic;
            bool belongs = target != null &&
                           (target.transform == sel.transform || target.transform.IsChildOf(sel.transform));

            if (!belongs)
            {
                Graphic fallback = FindOwnGraphic(sel);
                if (fallback != null)
                {
                    Undo.RecordObject(sel, "Reparar Target Graphic");
                    sel.targetGraphic = fallback;
                    EditorUtility.SetDirty(sel);
                    target = fallback;
                    repaired++;
                    Debug.LogWarning($"[RaycastCleanup] '{PathOf(sel.transform)}': Target Graphic estaba vacío o era de otro objeto. Se asignó '{fallback.name}'.", sel);
                }
                else
                {
                    Debug.LogWarning($"[RaycastCleanup] '{PathOf(sel.transform)}' no tiene ningún Graphic propio: no podrá recibir clics.", sel);
                }
            }

            if (target != null) keep.Add(target);
        }

        // 2) Campos de texto: conservan sus gráficos hijos (selección y caret)
        foreach (var input in Object.FindObjectsByType<TMP_InputField>(FindObjectsInactive.Include))
        {
            if (!IsSceneObject(input)) continue;
            foreach (var g in input.GetComponentsInChildren<Graphic>(true)) keep.Add(g);
        }

        // 3) Scroll: conservan el gráfico del viewport y de sus máscaras (para arrastrar)
        foreach (var scroll in Object.FindObjectsByType<ScrollRect>(FindObjectsInactive.Include))
        {
            if (!IsSceneObject(scroll)) continue;
            var viewport = scroll.viewport != null ? scroll.viewport : scroll.GetComponent<RectTransform>();
            var vg = viewport.GetComponent<Graphic>();
            if (vg != null) keep.Add(vg);
            foreach (var mask in scroll.GetComponentsInChildren<Mask>(true))
            {
                var mg = mask.GetComponent<Graphic>();
                if (mg != null) keep.Add(mg);
            }
        }

        // 4) Todo lo demás: raycast apagado, salvo por nombre explícito
        int disabled = 0;
        var touchedScenes = new HashSet<UnityEngine.SceneManagement.Scene>();
        foreach (var g in Object.FindObjectsByType<Graphic>(FindObjectsInactive.Include))
        {
            if (!IsSceneObject(g)) continue;
            if (!g.raycastTarget) continue;
            if (keep.Contains(g) || KeepByName.Contains(g.gameObject.name)) continue;

            Undo.RecordObject(g, "Limpiar Raycast Target");
            g.raycastTarget = false;
            EditorUtility.SetDirty(g);
            touchedScenes.Add(g.gameObject.scene);
            disabled++;
        }

        foreach (var s in touchedScenes) EditorSceneManager.MarkSceneDirty(s);
        if (repaired > 0)
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log($"[RaycastCleanup] Raycast apagado en {disabled} gráficos. Target Graphics reparados: {repaired}. Conservados: {keep.Count}.");
    }

    // Primer Image propio (o hijo) que no sea la marca de un Toggle ni un texto
    static Graphic FindOwnGraphic(Selectable sel)
    {
        Graphic exclude = (sel as Toggle) != null ? ((Toggle)sel).graphic : null;

        var own = sel.GetComponent<Graphic>();
        if (own != null && own != exclude) return own;

        foreach (var g in sel.GetComponentsInChildren<Graphic>(true))
        {
            if (g == exclude) continue;
            if (g is Image) return g;
        }
        return null;
    }

    static bool IsSceneObject(Component c)
    {
        return c != null && !EditorUtility.IsPersistent(c) && c.gameObject.scene.IsValid();
    }

    static string PathOf(Transform t)
    {
        string p = t.name;
        while (t.parent != null) { t = t.parent; p = t.name + "/" + p; }
        return p;
    }
}