using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class CardReaderUI : MonoBehaviour
{
    // ─── SINGLETON ─────────────────────────────────────────────
    public static CardReaderUI Instance { get; private set; }

    // ─── REFERENCIAS UI ────────────────────────────────────────
    [Header("Panel principal")]
    public GameObject readerPanel;      // panel completo que se muestra/oculta

    [Header("Contenido")]
    public Image cardImage;       // ilustración de la carta
    public TextMeshProUGUI authorText;  // nombre del autor
    public TextMeshProUGUI cardText;    // texto de la carta

    [Header("Audio")]
    public AudioSource babbleSource;    // fuente de audio para balbuceos

    [Header("Botón cerrar")]
    public Button closeButton;          // botón para cerrar la carta

    // ─── ESTADO ────────────────────────────────────────────────
    private bool isOpen = false;

    // ──────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debug.Log("[CardReaderUI] Instance inicializada correctamente");

        if (readerPanel != null)
            readerPanel.SetActive(false);
        else
            Debug.LogError("[CardReaderUI] readerPanel es NULL en Awake");
    }

    void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
    }

    private float openCooldown = 0f;

    void Update()
    {
        if (!isOpen) return;

        // Espera 0.1 segundos antes de permitir cerrar
        if (openCooldown > 0f)
        {
            openCooldown -= Time.unscaledDeltaTime; // unscaled porque el juego está pausado
            return;
        }

        if (Keyboard.current.eKey.wasPressedThisFrame ||
            Keyboard.current.spaceKey.wasPressedThisFrame)
            Hide();
    }

    // ─── MOSTRAR CARTA ─────────────────────────────────────────
    public void Show(CardData card)
    {
        Debug.Log($"[CardReaderUI] Show llamado con carta: {card?.cardID}");

        if (card == null)
        {
            Debug.LogError("[CardReaderUI] card es NULL");
            return;
        }

        if (cardImage != null)
            cardImage.sprite = card.cardSprite;
        else
            Debug.LogError("[CardReaderUI] cardImage es NULL");

        if (authorText != null)
            authorText.text = card.authorName;
        else
            Debug.LogError("[CardReaderUI] authorText es NULL");

        if (cardText != null)
            cardText.text = card.cardText;
        else
            Debug.LogError("[CardReaderUI] cardText es NULL");

        if (babbleSource != null && card.babbleAudio != null)
        {
            babbleSource.clip = card.babbleAudio;
            babbleSource.Play();
        }

        Debug.Log($"[CardReaderUI] Activando panel: {readerPanel?.name}");
        readerPanel.SetActive(true);
        Time.timeScale = 0f;
        isOpen = true;
        openCooldown = 0.1f;
    }

    // ─── OCULTAR CARTA ─────────────────────────────────────────
    public void Hide()
    {
        if (!isOpen) return;

        // Detener audio si sigue sonando
        if (babbleSource != null && babbleSource.isPlaying)
            babbleSource.Stop();

        readerPanel.SetActive(false);
        Time.timeScale = 1f;
        isOpen = false;
    }

    // ─── PROPIEDADES PÚBLICAS ──────────────────────────────────
    public bool IsOpen => isOpen;
}