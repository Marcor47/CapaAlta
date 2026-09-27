using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CardData;

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



    [Header("Bonos permanentes")]
    public float regulacionBonusPerFatherCard = 10f;
    public float motivacionBonusPerSecondaryGroup = 10f;

    private TheoController theo;

    private static readonly Dictionary<string, int> secondaryGroupTotals = new Dictionary<string, int>
    {
        { "Grace", 2 }, { "Les", 3 }, { "Duke", 3 }, { "Benny", 3 }
    };



    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        theo = FindAnyObjectByType<TheoController>();
        LoadFromAccount(); // NUEVO
    }

    public void LoadFromAccount()
    {
        if (AccountManager.Instance == null || AccountManager.Instance.CurrentUser == null) return;
        var saved = AccountManager.Instance.CurrentUser.progress.allTimeCollectedCards;
        // Nota: esto marca cuáles ya se recolectaron ALGUNA VEZ, para los bonos —
        // no repuebla collectedCards/collectedIDs de la sesión actual, porque esas
        // representan lo recogido en ESTE intento del capítulo (se reinician al reiniciar).
        // Los bonos permanentes ya viven aparte, en theo.maxStamina/maxRegulacion (ver abajo).
    }

    public void ResetState()
    {
        collectedCards.Clear();
        collectedIDs.Clear();
    }

    // ─── AÑADIR CARTA ──────────────────────────────────────────
    public void AddCard(CardData card)
    {
        if (card == null) return;
        if (collectedIDs.Contains(card.cardID)) return; // ya la tiene

        collectedCards.Add(card);
        collectedIDs.Add(card.cardID);

        OnCardAdded?.Invoke(card);
        CheckPermanentBonuses(card);

        Debug.Log($"[CardInventory] Carta recolectada: {card.cardID} — {card.authorName}");
    }


    // ─── AÑADIR CARTA DA MEJORA PERMANENTE ──────────────────────────────────────────
    private void CheckPermanentBonuses(CardData card)
    {
        if (theo == null) return;
        if (AccountManager.Instance == null || AccountManager.Instance.CurrentUser == null) return;

        var progress = AccountManager.Instance.CurrentUser.progress;

        // Si esta carta YA fue recolectada alguna vez por este alumno (aunque haya reiniciado el capítulo), no repetir el bono
        bool alreadyCollectedEver = progress.allTimeCollectedCards.Exists(c => c.cardID == card.cardID);
        if (alreadyCollectedEver) return;

        progress.allTimeCollectedCards.Add(new CollectedCardRecord { cardID = card.cardID, authorName = card.authorName });

        if (card.cardType == CardType.Father)
        {
            theo.IncreaseRegulacionPermanent(regulacionBonusPerFatherCard);
            progress.regulacionBonusPermanent += regulacionBonusPerFatherCard; // NUEVO
        }
        else if (card.cardType == CardType.Secondary && secondaryGroupTotals.TryGetValue(card.authorName, out int total))
        {
            int allTimeFromAuthor = progress.allTimeCollectedCards.Count(c => c.authorName == card.authorName);
            if (allTimeFromAuthor == total)
            {
                theo.IncreaseMotivacionPermanent(motivacionBonusPerSecondaryGroup);
                progress.staminaBonusPermanent += motivacionBonusPerSecondaryGroup; // NUEVO — ojo, revisa el nombre exacto del campo (ver StudentProgress arriba: es staminaBonusPermanent)
            }
        }

        AccountManager.Instance.SaveProgress();
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