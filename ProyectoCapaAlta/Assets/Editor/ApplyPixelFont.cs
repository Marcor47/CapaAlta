using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ApplyPixelFont
{
    const string PrefabFolder = "Assets/Prefabs/UI";

    static TMP_FontAsset GetFont()
    {
        var f = Selection.activeObject as TMP_FontAsset;
        if (f == null)
            EditorUtility.DisplayDialog("Fuente pixel",
                "Primero selecciona el Font Asset (PixelFont SDF) en la ventana Project.", "OK");
        return f;
    }

    static void Apply(TMP_Text t, TMP_FontAsset f)
    {
        Undo.RecordObject(t, "Fuente pixel");
        t.font = f;
        t.fontSharedMaterial = f.material;
        EditorUtility.SetDirty(t);
    }

    [MenuItem("Capa Alta/Fuente pixel/1 - Guardar en DashboardSkin")]
    static void SetSkin()
    {
        var f = GetFont(); if (f == null) return;
        foreach (var g in AssetDatabase.FindAssets("t:DashboardSkin"))
        {
            var s = AssetDatabase.LoadAssetAtPath<DashboardSkin>(AssetDatabase.GUIDToAssetPath(g));
            Undo.RecordObject(s, "Fuente skin");
            s.font = f;
            EditorUtility.SetDirty(s);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("DashboardSkin.font actualizado.");
    }

    [MenuItem("Capa Alta/Fuente pixel/2 - Aplicar a la escena abierta")]
    static void ApplyScene()
    {
        var f = GetFont(); if (f == null) return;
        int n = 0;
        foreach (var t in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include))
        {
            if (!t.gameObject.scene.IsValid()) continue;
            Apply(t, f); n++;
        }
        EditorSceneManager.MarkAllScenesDirty();
        Debug.Log($"Fuente aplicada a {n} textos de la escena.");
    }

    [MenuItem("Capa Alta/Fuente pixel/3 - Aplicar a los prefabs de UI")]
    static void ApplyPrefabs()
    {
        var f = GetFont(); if (f == null) return;
        if (!AssetDatabase.IsValidFolder(PrefabFolder)) { Debug.LogWarning("No existe " + PrefabFolder); return; }
        int n = 0;
        foreach (var g in AssetDatabase.FindAssets("t:Prefab", new[] { PrefabFolder }))
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var root = PrefabUtility.LoadPrefabContents(path);
            foreach (var t in root.GetComponentsInChildren<TMP_Text>(true)) { Apply(t, f); n++; }
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }
        Debug.Log($"Fuente aplicada a {n} textos en prefabs.");
    }
}