using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class CardCollector : MonoBehaviour
{
    // ─── DATOS ─────────────────────────────────────────────────
    [Header("Datos de la carta")]
    public CardData cardData;

    // ─── ESTADO ────────────────────────────────────────────────
    private bool isCollected = false;
    private bool playerInRange = false;
    private SpriteRenderer sr;

    // ──────────────────────────────────────────────────────────
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        // Verifica si ya fue recolectada
        if (CardInventory.Instance != null &&
            CardInventory.Instance.IsCollected(cardData.cardID))
        {
            SetCollected();
        }
    }

    void Update()
    {
        if (playerInRange && !isCollected)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
                Collect();
        }
    }

    // ─── TRIGGER ───────────────────────────────────────────────
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected || !other.CompareTag("Player")) return;
        playerInRange = true;
        // TODO: UIHintManager.Instance.Show("Presiona E para leer");
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        // TODO: UIHintManager.Instance.Hide();
    }

    // ─── RECOLECCIÓN ───────────────────────────────────────────
    void Collect()
    {
        if (isCollected || cardData == null) return;

        if (CardInventory.Instance != null)
            CardInventory.Instance.AddCard(cardData);

        if (CardReaderUI.Instance != null)
            CardReaderUI.Instance.Show(cardData);

        // TODO: MotivationSystem.Instance.AddMotivation(cardData.cardType);

        SetCollected();
    }

    void SetCollected()
    {
        isCollected = true;
        playerInRange = false;

        if (sr != null) sr.enabled = false;
        GetComponent<Collider2D>().enabled = false;
    }

    // ─── DEBUG ─────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = cardData != null && cardData.cardType == CardData.CardType.Father
            ? Color.yellow : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}