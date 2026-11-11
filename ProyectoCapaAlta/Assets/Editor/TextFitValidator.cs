using TMPro;
using UnityEditor;
using UnityEngine;

public static class TextFitValidator
{
    [MenuItem("Capa Alta/Validar textos (cartas y diálogos)")]
    static void Run()
    {
        var reader = Object.FindAnyObjectByType<CardReaderUI>(FindObjectsInactive.Include);
        var dlg = Object.FindAnyObjectByType<DialogueManager>(FindObjectsInactive.Include);
        if (reader == null || dlg == null)
        {
            EditorUtility.DisplayDialog("Capa Alta", "Abre una escena de capítulo (necesita UICanvas con CardReaderUI y DialogueManager).", "OK");
            return;
        }

        int total = 0, bad = 0;

        foreach (var guid in AssetDatabase.FindAssets("t:CardData"))
        {
            var card = AssetDatabase.LoadAssetAtPath<CardData>(AssetDatabase.GUIDToAssetPath(guid));
            if (card == null) continue;
            total++;
            bad += Check(reader.cardText, card.cardText, "CARTA " + card.name);
        }

        TMP_Text optionRef = (dlg.optionTexts != null && dlg.optionTexts.Length > 0) ? dlg.optionTexts[0] : null;

        foreach (var guid in AssetDatabase.FindAssets("t:DialogueNode"))
        {
            var node = AssetDatabase.LoadAssetAtPath<DialogueNode>(AssetDatabase.GUIDToAssetPath(guid));
            if (node == null) continue;
            var so = new SerializedObject(node);
            var it = so.GetIterator();
            while (it.NextVisible(true))
            {
                if (it.propertyType != SerializedPropertyType.String) continue;
                if (it.name == "text")
                {
                    total++;
                    bad += Check(dlg.dialogueText, it.stringValue, "DIÁLOGO " + node.name);
                }
                else if (it.name == "optionText" && optionRef != null)
                {
                    total++;
                    bad += Check(optionRef, it.stringValue, "OPCIÓN " + node.name);
                }
            }
        }

        Debug.Log($"[TextFit] Revisados {total} textos. Se salen: {bad}.");
    }

    static int Check(TMP_Text t, string s, string label)
    {
        if (t == null || string.IsNullOrEmpty(s)) return 0;

        Rect r = t.rectTransform.rect;
        Vector4 m = t.margin;                       // izq, arriba, der, abajo
        float w = r.width - m.x - m.z;
        float h = r.height - m.y - m.w;

        Vector2 pref = t.GetPreferredValues(s, w, 0f);
        if (pref.y <= h + 0.01f) return 0;

        string excerpt = s.Length > 50 ? s.Substring(0, 50) + "…" : s;
        Debug.LogWarning($"[TextFit] {label}: necesita {pref.y:0} px de alto y hay {h:0}. \"{excerpt}\"");
        return 1;
    }
}