using System.Linq;
using UnityEditor;
using UnityEngine;

public static class GroundTileCreator
{
    [MenuItem("Capa Alta/Crear GroundTile desde el tileset seleccionado")]
    static void Create()
    {
        var tex = Selection.activeObject as Texture2D;
        if (tex == null)
        {
            EditorUtility.DisplayDialog("Capa Alta", "Selecciona en el Project la imagen del tileset (ya cortada en sprites).", "OK");
            return;
        }

        string texPath = AssetDatabase.GetAssetPath(tex);
        var sprites = AssetDatabase.LoadAllAssetsAtPath(texPath).OfType<Sprite>().ToArray();
        if (sprites.Length == 0)
        {
            EditorUtility.DisplayDialog("Capa Alta",
                "La imagen no tiene sprites. Pon Sprite Mode = Multiple y córtala con Grid By Cell Size 16x16.", "OK");
            return;
        }

        var tile = ScriptableObject.CreateInstance<GroundTile>();
        tile.sprites = new Sprite[GroundTile.SlotCount];

        foreach (var s in sprites)
        {
            int col = Mathf.RoundToInt(s.rect.x / 16f);
            int row = Mathf.RoundToInt((tex.height - s.rect.yMax) / 16f);   // fila contada desde arriba
            int idx = row * GroundTile.Columns + col;
            if (idx >= 0 && idx < tile.sprites.Length) tile.sprites[idx] = s;
        }

        string dir = System.IO.Path.GetDirectoryName(texPath);
        string path = AssetDatabase.GenerateUniqueAssetPath($"{dir}/GroundTile_{tex.name}.asset");
        AssetDatabase.CreateAsset(tile, path);
        AssetDatabase.SaveAssets();

        Selection.activeObject = tile;
        EditorGUIUtility.PingObject(tile);

        if (tile.sprites[GroundTile.C0] == null || tile.sprites[GroundTile.T0] == null)
            Debug.LogWarning("[GroundTile] Faltan sprites clave. Revisa que cortaste con celdas de 16x16.");
        else
            Debug.Log($"[GroundTile] Creado en {path}");
    }
}