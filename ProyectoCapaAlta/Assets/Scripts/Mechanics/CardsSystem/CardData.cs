using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Capa Alta/Card Data")]
public class CardData : ScriptableObject
{
    // ─── IDENTIFICACIÓN ────────────────────────────────────────
    [Header("Identificación")]
    public string cardID;
    // ej: "father_01", "secondary_03"

    public string authorName;
    // ej: "Tu padre", "Un habitante de Capa Alta"

    public enum CardType { Father, Secondary }
    public CardType cardType;

    public int chapter;
    // Capítulo donde aparece esta carta (1–4)

    // ─── CONTENIDO ─────────────────────────────────────────────
    [Header("Contenido")]
    [TextArea(4, 10)]
    public string cardText;
    // Texto completo de la carta

    // ─── VISUAL Y AUDIO ────────────────────────────────────────
    [Header("Visual y Audio")]
    public Sprite cardSprite;
    // Ilustración de la carta — se muestra en la UI de lectura (160x240 px)
    // El sprite en el mundo va directo al SpriteRenderer del objeto en escena

    public AudioClip babbleAudio;
    // Audio de balbuceo al leer la carta
    // Solo necesario para cartas del padre (CardType.Father)
}