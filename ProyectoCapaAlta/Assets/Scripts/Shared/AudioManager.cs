using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Fuentes")]
    public AudioSource sfxSource;
    public AudioSource musicSource;

    [Header("SFX")]
    public AudioClip buttonClick;   // botones UI y disparo de herramientas
    public AudioClip footstep;
    public AudioClip jumpDash;      // mismo clip para salto y dash
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 0.3f)] public float pitchVariation = 0.08f;

    [Header("Música")]
    public AudioClip loginMusic;
    public AudioClip chapterMusic;  // una sola para los 4 capítulos
    [Range(0f, 1f)] public float musicVolume = 0.4f;
    public string loginSceneName = "LoginScene";
    public string chapterScenePrefix = "Cap";

    [Header("Balbuceo (diálogos, cartas, llamadas)")]
    public AudioClip babbleBlip;                       // sonido MUY corto (0.05–0.12 s)
    [Range(0f, 1f)] public float babbleVolume = 0.6f;
    public float babbleInterval = 0.07f;               // segundos entre blips
    public float babbleSecondsPerCharacter = 0.04f;    // duración según largo del texto
    public float babbleMaxSeconds = 3f;                // tope por línea o carta
    public float babblePitchMin = 0.9f;
    public float babblePitchMax = 1.2f;
    private Coroutine babbleRoutine;

    public AudioSource babbleSource;

    private bool userInteracted;
    private AudioClip pendingMusic;
    private readonly List<RaycastResult> hits = new List<RaycastResult>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (sfxSource != null) { sfxSource.playOnAwake = false; sfxSource.loop = false; }
        if (musicSource != null) { musicSource.playOnAwake = false; musicSource.loop = true; musicSource.volume = musicVolume; }
        if (babbleSource != null) { babbleSource.playOnAwake = false; babbleSource.loop = false; }
        SceneManager.sceneLoaded += OnSceneLoaded;
        // la escena inicial ya cargó antes de suscribirnos
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == loginSceneName) PlayMusic(loginMusic);
        else if (scene.name.StartsWith(chapterScenePrefix)) PlayMusic(chapterMusic);
        // otras escenas: se mantiene la música actual
    }

    void Update()
    {
        bool mousePressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool keyPressed = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;

        // WebGL no deja reproducir audio hasta el primer gesto del usuario
        if (!userInteracted && (mousePressed || keyPressed))
        {
            userInteracted = true;
            if (pendingMusic != null) { StartMusic(pendingMusic); pendingMusic = null; }
        }

        if (mousePressed && IsOverActiveButton()) PlayClick();
    }

    bool IsOverActiveButton()
    {
        if (EventSystem.current == null) return false;
        var data = new PointerEventData(EventSystem.current) { position = Mouse.current.position.ReadValue() };
        hits.Clear();
        EventSystem.current.RaycastAll(data, hits);
        foreach (var h in hits)
        {
            var b = h.gameObject.GetComponentInParent<Button>();
            if (b != null && b.interactable) return true;
        }
        return false;
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        if (!userInteracted) { pendingMusic = clip; return; }
        StartMusic(clip);
    }

    void StartMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void PlaySfx(AudioClip clip, bool randomPitch = false)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.pitch = randomPitch ? 1f + Random.Range(-pitchVariation, pitchVariation) : 1f;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // Helpers estáticos null-safe (no fallan si la escena se prueba sin AudioManager)
    public static void PlayClick() { if (Instance != null) Instance.PlaySfx(Instance.buttonClick); }
    public static void PlayStep() { if (Instance != null) Instance.PlaySfx(Instance.footstep, true); }
    public static void PlayJumpDash() { if (Instance != null) Instance.PlaySfx(Instance.jumpDash, true); }

    // ─── BALBUCEO ──────────────────────────────────────────────
    public static void PlayBabble(int characters, float pitchOffset = 0f)
    { if (Instance != null) Instance.StartBabble(characters, pitchOffset); }

    public static void StopBabble()
    { if (Instance != null) Instance.StopBabbleInternal(); }

    void StartBabble(int characters, float pitchOffset)
    {
        StopBabbleInternal();
        if (babbleBlip == null || babbleSource == null || characters <= 0) return;
        float duration = Mathf.Min(characters * babbleSecondsPerCharacter, babbleMaxSeconds);
        babbleRoutine = StartCoroutine(BabbleRoutine(duration, pitchOffset));
    }

    void StopBabbleInternal()
    {
        if (babbleRoutine != null) { StopCoroutine(babbleRoutine); babbleRoutine = null; }
    }

    IEnumerator BabbleRoutine(float duration, float pitchOffset)
    {
        var wait = new WaitForSecondsRealtime(babbleInterval);
        float t = 0f;
        while (t < duration)
        {
            babbleSource.pitch = Random.Range(babblePitchMin, babblePitchMax) + pitchOffset;
            babbleSource.PlayOneShot(babbleBlip, babbleVolume);
            yield return wait;
            t += babbleInterval;
        }
        babbleRoutine = null;
    }
}