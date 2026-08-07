using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Capa Alta/Card Data")]
public class CardData : ScriptableObject
{
    // ─── IDENTIFICACIÓN ────────────────────────────────────────
    [Header("Identificación")]
    public string cardID;           // ej: "father_01", "secondary_03"
    public string authorName;       // ej: "Tu padre", "Un habitante de Capa Alta"

    public enum CardType { Father, Secondary }
    public CardType cardType;

    // ─── CONTENIDO ─────────────────────────────────────────────
    [Header("Contenido")]
    [TextArea(4, 10)]
    public string cardText;         // texto completo de la carta

    public Sprite cardSprite;       // ilustración de la carta

    public AudioClip babbleAudio;   // audio de balbuceo (solo cartas del padre)

    // ─── PROGRESIÓN ────────────────────────────────────────────
    [Header("Progresión")]
    public int chapter;             // capítulo donde aparece (1-4)
    public bool isCollected;        // se actualiza en runtime, no guardar aquí
}