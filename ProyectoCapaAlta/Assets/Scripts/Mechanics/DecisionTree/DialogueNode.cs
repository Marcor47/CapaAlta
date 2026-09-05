using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogueNode",
                 menuName = "Capa Alta/Dialogue Node")]
public class DialogueNode : ScriptableObject
{
    // ─── IDENTIFICACIÓN ────────────────────────────────────────
    [Header("Identificación")]
    public string nodeID;
    // ej: "gaz_cap1_enc1", "ryland_cap1_enc1"

    public string npcName;
    // Nombre que aparece en la UI: "Gaz", "Astro", "Ryland"...

    public int chapter;
    // Capítulo donde ocurre este encuentro (1-4)

    // ─── DIÁLOGO INICIAL ───────────────────────────────────────
    [Header("Diálogo inicial del NPC")]
    [Tooltip("Líneas que el NPC dice antes de que aparezcan las opciones")]
    public string[] openingLines;

    // ─── OPCIONES DEL JUGADOR ──────────────────────────────────
    [Header("Opciones del jugador")]
    public DialogueOption[] options;

    // ─── MANZANA TUTORIAL ──────────────────────────────────────
    [Header("Manzana")]
    [Tooltip("Este nodo entrega la manzana tutorial al jugador")]
    public bool givesManzanaOnEnd;

    [Tooltip("Líneas que se dicen al dar la manzana tutorial")]
    public string[] manzanaTutorialLines;

    // ─── CONSECUENCIAS ─────────────────────────────────────────
    [Header("Consecuencias")]
    [Tooltip("ID del NPC para registrar su relación en DecisionRecord")]
    public string npcID;
    // ej: "gaz", "ryland_astro", "haru"
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
    public string[] npcResponseLines;

    [Tooltip("Líneas adicionales comunes después de la respuesta (cierre del diálogo)")]
    public string[] closingLines;
}