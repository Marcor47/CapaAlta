using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance { get; private set; }

    [Header("Paneles")]
    public GameObject pausePanel;
    public GameObject optionsPanel;

    [Header("Botones — panel principal de pausa")]
    public Button resumeButton;
    public Button optionsButton;
    public Button mainMenuButton;

    [Header("Botones — panel de opciones (audio)")]
    public Button volumeUpButton;
    public Button volumeDownButton;
    public Button backFromOptionsButton;
    public TextMeshProUGUI volumeText;

    [Header("Configuración")]
    public float volumeStep = 0.1f;
    public string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;
    private TheoController theo;
    private bool isPausePending = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    void Start()
    {
        theo = FindAnyObjectByType<TheoController>();

        resumeButton.onClick.AddListener(ClosePause);
        optionsButton.onClick.AddListener(OpenOptions);
        mainMenuButton.onClick.AddListener(GoToMainMenu);

        volumeUpButton.onClick.AddListener(() => ChangeVolume(volumeStep));
        volumeDownButton.onClick.AddListener(() => ChangeVolume(-volumeStep));
        backFromOptionsButton.onClick.AddListener(CloseOptions);
        theo.OnNotebookLoopStarted += HandleNotebookLoopStarted;
        UpdateVolumeText();
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen) return;

            if (isPaused) ClosePause();
            else if (isPausePending) CancelPendingPause(); // presionó ESC de nuevo antes de que termine la animación
            else OpenPause();
        }
    }

    // ─── ABRIR / CERRAR PAUSA ───────────────────────────────────
    public void OpenPause()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen) return;

        isPausePending = true;
        if (theo != null) theo.TriggerNotebook();
        // Time.timeScale sigue en 1 aquí a propósito, para que la animación pueda avanzar hasta el Loop
    }

    void HandleNotebookLoopStarted()
    {
        if (!isPausePending) return; // el jugador solo se sentó con "S", no fue una pausa

        isPausePending = false;
        isPaused = true;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        if (theo != null) theo.SetDialogueState(true);
    }

    void CancelPendingPause()
    {
        isPausePending = false;
        if (theo != null) theo.StandUp();
    }

    public void ClosePause()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);

        if (theo != null)
        {
            theo.StandUp();
            theo.SetDialogueState(false);
        }
    }

    // ─── OPCIONES (AUDIO) ───────────────────────────────────────
    void OpenOptions()
    {
        pausePanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    void CloseOptions()
    {
        optionsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    void ChangeVolume(float delta)
    {
        AudioListener.volume = Mathf.Clamp01(AudioListener.volume + delta);
        UpdateVolumeText();
    }

    void UpdateVolumeText()
    {
        if (volumeText != null)
            volumeText.text = Mathf.RoundToInt(AudioListener.volume * 100f) + "%";
    }

    // ─── MENÚ PRINCIPAL (sin cerrar sesión) ─────────────────────
    void GoToMainMenu()
    {
        Time.timeScale = 1f; // importante: restaurar antes de cambiar de escena
        SceneManager.LoadScene(mainMenuSceneName);
    }
}