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

    // ─── RONDAS DE DIÁLOGO ──────────────────────────────────────
    [Header("Rondas de diálogo")]
    [Tooltip("Cada ronda: líneas de introducción del NPC + 3 opciones del jugador. " +
             "Ronda 0 = apertura del encuentro. Si el encuentro tiene una sola toma " +
             "de decisión, deja el array con un solo elemento.")]
    public DialogueRound[] rounds;

    // ─── MANZANA: NPC → THEO ─────────────────────────────────────
    [Header("Manzana que el NPC le da a Theo (tutorial)")]
    public bool givesManzanaOnEnd;

    [Tooltip("Líneas tras entregar la manzana — se muestran igual sin importar qué opción se eligió")]
    public DialogueLine[] manzanaTutorialLines;

    // ─── MANZANA: THEO → NPC ─────────────────────────────────────
    [Header("Manzana que Theo le da al NPC (después de este encuentro)")]
    [Tooltip("Deja los 3 arrays vacíos si en este encuentro no se puede dar una manzana")]
    public DialogueLine[] manzanaGivenWhenBad;      // relación -1 → sube a neutral
    public DialogueLine[] manzanaGivenWhenNeutral;  // relación 0 → sube a buena
    public DialogueLine[] manzanaGivenWhenGood;     // relación +1 → ya al máximo

    public DialogueLine[] GetManzanaGivenResponse(int currentRelationship)
    {
        if (currentRelationship <= -1) return manzanaGivenWhenBad;
        if (currentRelationship == 0) return manzanaGivenWhenNeutral;
        return manzanaGivenWhenGood;
    }
}

// ─── RONDA DE DIÁLOGO ────────────────────────────────────────────
[System.Serializable]
public class DialogueRound
{
    [Tooltip("Opcional: aperturas alternativas según la relación con el NPC al iniciar este " +
             "encuentro (ej. Gaz Cap2 Enc2 según cómo quedó Cap1). Solo tiene sentido en la ronda 0.")]
    public ConditionalOpening[] conditionalOpenings;

    [Tooltip("Líneas del NPC antes de mostrar las opciones — se usan si ninguna condición en conditionalOpenings aplica")]
    public DialogueLine[] npcLeadInLines;


    [Tooltip("Opciones del jugador para esta ronda (mala/neutral/buena)")]
    public DialogueOption[] options;

    [Tooltip("Líneas comunes que se muestran DESPUÉS de la respuesta de la opción elegida, sin importar cuál fue")]
    public DialogueLine[] sharedFollowUpLines;

    public DialogueLine[] GetOpeningLines(int currentRelationship)
    {
        if (conditionalOpenings != null)
        {
            foreach (var co in conditionalOpenings)
            {
                if (co.requiredRelationship == currentRelationship)
                    return co.npcLeadInLines;
            }
        }
        return npcLeadInLines;
    }
}

[System.Serializable]
public class ConditionalOpening
{
    [Range(-1, 1)]
    public int requiredRelationship;

    public DialogueLine[] npcLeadInLines;
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

    [Tooltip("Cambio en la relación de ESTA ronda: -1 mala, 0 neutral, +1 buena. " +
             "Si hay varias rondas, se suman todas y el total se limita entre -1 y +1 " +
             "al terminar el nodo.")]
    [Range(-1, 1)]
    public int relationshipDelta;

    [Tooltip("Todo lo que dice el NPC en respuesta a esta opción — incluida la " +
             "despedida si esta es la última ronda del nodo.")]
    public DialogueLine[] npcResponseLines;
}