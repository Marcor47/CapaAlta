using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CardCollector : MonoBehaviour
{
    // ─── DATOS ─────────────────────────────────────────────────
    [Header("Datos de la carta")]
    public CardData cardData;

    // ─── VISUAL ────────────────────────────────────────────────
    [Header("Visual")]
    public SpriteRenderer cardSprite;  // sprite de la carta en el nivel
    public GameObject glowEffect;      // efecto de brillo (opcional)

    // ─── ESTADO ────────────────────────────────────────────────
    private bool isCollected = false;
    private bool playerInRange = false;

    // ──────────────────────────────────────────────────────────
    void Start()
    {
        // Asigna el sprite de la carta si tiene uno definido
        if (cardSprite != null && cardData != null && cardData.cardSprite != null)
            cardSprite.sprite = cardData.cardSprite;

        // Verifica si ya fue recolectada en una sesión anterior
        if (CardInventory.Instance != null &&
            CardInventory.Instance.IsCollected(cardData.cardID))
        {
            SetCollected();
        }
    }

    void Update()
    {
        // Theo presiona E estando en rango para leer la carta
        if (playerInRange && !isCollected)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
                Collect();
        }
    }

    // ─── TRIGGER ───────────────────────────────────────────────
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        // TODO: mostrar indicador "presiona E" sobre la carta
        // UIHintManager.Instance.Show("Presiona E para leer");
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        // TODO: ocultar indicador
        // UIHintManager.Instance.Hide();
    }

    // ─── RECOLECCIÓN ───────────────────────────────────────────
    void Collect()
    {
        if (isCollected || cardData == null) return;

        // 1. Registrar en el inventario
        if (CardInventory.Instance != null)
            CardInventory.Instance.AddCard(cardData);

        // 2. Mostrar la UI de lectura
        if (CardReaderUI.Instance != null)
            CardReaderUI.Instance.Show(cardData);

        // 3. Aumentar motivación (M6)
        // TODO: MotivationSystem.Instance.AddMotivation(cardData.cardType);

        // 4. Marcar como recolectada
        SetCollected();
    }

    void SetCollected()
    {
        isCollected = true;
        playerInRange = false;

        // Oculta la carta del nivel (ya fue recogida)
        if (cardSprite != null) cardSprite.enabled = false;
        if (glowEffect != null) glowEffect.SetActive(false);

        // Desactiva el collider para que no siga detectando
        GetComponent<Collider2D>().enabled = false;
    }

    // ─── DEBUG ─────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = cardData != null && cardData.cardType == CardData.CardType.Father
            ? Color.yellow
            : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}