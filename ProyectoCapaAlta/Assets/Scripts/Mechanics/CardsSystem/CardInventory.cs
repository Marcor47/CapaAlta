using System.Collections.Generic;
using UnityEngine;

public class CardInventory : MonoBehaviour
{
    // ─── SINGLETON ─────────────────────────────────────────────
    public static CardInventory Instance { get; private set; }

    // ─── INVENTARIO ────────────────────────────────────────────
    private List<CardData> collectedCards = new List<CardData>();
    private HashSet<string> collectedIDs = new HashSet<string>();

    // ─── EVENTOS ───────────────────────────────────────────────
    // Otros sistemas pueden suscribirse para reaccionar
    public event System.Action<CardData> OnCardAdded;

    // ──────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ─── AÑADIR CARTA ──────────────────────────────────────────
    public void AddCard(CardData card)
    {
        if (card == null) return;
        if (collectedIDs.Contains(card.cardID)) return; // ya la tiene

        collectedCards.Add(card);
        collectedIDs.Add(card.cardID);

        OnCardAdded?.Invoke(card);

        Debug.Log($"[CardInventory] Carta recolectada: {card.cardID} — {card.authorName}");
    }

    // ─── CONSULTAS ─────────────────────────────────────────────
    public bool IsCollected(string cardID)
    {
        return collectedIDs.Contains(cardID);
    }

    public List<CardData> GetAllCards()
    {
        return collectedCards;
    }

    public List<CardData> GetCardsByType(CardData.CardType type)
    {
        return collectedCards.FindAll(c => c.cardType == type);
    }

    public List<CardData> GetCardsByChapter(int chapter)
    {
        return collectedCards.FindAll(c => c.chapter == chapter);
    }

    public int TotalCollected => collectedCards.Count;

    // ─── DEBUG ─────────────────────────────────────────────────
    [ContextMenu("Listar cartas recolectadas")]
    void DebugListCards()
    {
        if (collectedCards.Count == 0)
        {
            Debug.Log("[CardInventory] No hay cartas recolectadas.");
            return;
        }
        foreach (var c in collectedCards)
            Debug.Log($"  [{c.cardType}] {c.cardID} — Cap.{c.chapter} — {c.authorName}");
    }
}