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

        // Cerrar al inicio
        if (readerPanel != null)
            readerPanel.SetActive(false);
    }

    void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
    }

    void Update()
    {
        // También cerrar con E o Espacio
        if (isOpen)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame ||
                Keyboard.current.spaceKey.wasPressedThisFrame)
                Hide();
        }
    }

    // ─── MOSTRAR CARTA ─────────────────────────────────────────
    public void Show(CardData card)
    {
        if (card == null) return;

        // Rellenar contenido
        if (cardImage != null)
            cardImage.sprite = card.cardSprite;

        if (authorText != null)
            authorText.text = card.authorName;

        if (cardText != null)
            cardText.text = card.cardText;

        // Reproducir balbuceo si tiene audio y es carta del padre
        if (babbleSource != null && card.babbleAudio != null)
        {
            babbleSource.clip = card.babbleAudio;
            babbleSource.Play();
        }

        // Mostrar panel y pausar el juego
        readerPanel.SetActive(true);
        Time.timeScale = 0f;
        isOpen = true;
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