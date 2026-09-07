using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogueNode",
                 menuName = "Capa Alta/Dialogue Node")]
public class DialogueNode : ScriptableObject
{
    // ─── IDENTIFICACIÓN ────────────────────────────────────────
    [Header("Identificación")]
    public string nodeID;
    public string npcName;
    public int chapter;
    public string npcID;

    [Tooltip("Si es true, todas las líneas usan npcName automáticamente. Si es false, cada línea define su propio speakerName.")]
    public bool isSingleSpeaker = true;

    // ─── DIÁLOGO INICIAL ───────────────────────────────────────
    [Header("Diálogo inicial del NPC")]
    [Tooltip("Líneas que el NPC dice antes de que aparezcan las opciones")]
    public DialogueLine[] openingLines;

    // ─── OPCIONES DEL JUGADOR ──────────────────────────────────
    [Header("Opciones del jugador")]
    public DialogueOption[] options;

    // ─── MANZANA TUTORIAL ──────────────────────────────────────
    [Header("Manzana")]
    public bool givesManzanaOnEnd;

    [Tooltip("Líneas tras entregar la manzana — speakerName por línea")]
    public DialogueLine[] manzanaTutorialLines;
}

// ─── LÍNEA DE DIÁLOGO ──────────────────────────────────────────
[System.Serializable]
public class DialogueLine
{
    [Tooltip("Quién dice esta línea. Vacío = usa npcName del asset.")]
    public string speakerName;
    [TextArea(1, 4)]
    public string text;
}

// ─── OPCIÓN DE DIÁLOGO ─────────────────────────────────────────
[System.Serializable]
public class DialogueOption
{
    [Tooltip("Texto que ve el jugador como opción")]
    public string optionText;

    [Tooltip("Cambio en la relación: -1 mala, 0 neutral, +1 buena")]
    [Range(-1, 1)]
    public int relationshipDelta;

    [Tooltip("Líneas que responde el NPC según esta opción")]
    public DialogueLine[] npcResponseLines;

    [Tooltip("Líneas adicionales comunes después de la respuesta")]
    public DialogueLine[] closingLines;
}
