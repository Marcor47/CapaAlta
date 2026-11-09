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
    [Range(0f, 1f)] public float loginMusicVolume = 0.6f;
    [Range(0f, 1f)] public float chapterMusicVolume = 0.4f;
    public string loginSceneName = "LoginScene";
    public string chapterScenePrefix = "Cap";

    [Header("Balbuceo (diálogos, cartas, llamadas)")]
    public AudioClip babbleBlip;                       // sonido MUY corto (0.05–0.12 s)
    [Range(0f, 1f)] public float babbleVolume = 0.6f;
    public float babbleInterval = 0.07f;               // segundos entre blips
    public float lettersPerBlip = 3f;       // 1 blip por cada ~3 letras de la palabra
    public float wordGap = 0.04f;           // pausa entre palabras
    public float commaPause = 0.15f;        // tras , ; :
    public float sentencePause = 0.3f;      // tras . ! ? …
    public float babbleMaxSeconds = 3f;                // tope por línea o carta
    
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
        if (musicSource != null) { musicSource.playOnAwake = false; musicSource.loop = true; }
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
        musicSource.volume = clip == loginMusic ? loginMusicVolume : chapterMusicVolume;
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
    public static void PlayBabble(string text, float maxSeconds = -1f)
    { if (Instance != null) Instance.StartBabble(text, maxSeconds); }

    void StartBabble(string text, float maxSeconds)
    {
        StopBabbleInternal();
        if (babbleBlip == null || babbleSource == null || string.IsNullOrEmpty(text)) return;
        babbleRoutine = StartCoroutine(BabbleRoutine(text, maxSeconds > 0f ? maxSeconds : babbleMaxSeconds));
    }

    IEnumerator BabbleRoutine(string text, float maxSeconds)
    {
        babbleSource.pitch = 1f;
        var blipWait = new WaitForSecondsRealtime(babbleInterval);
        float elapsed = 0f;
        int i = 0;

        while (i < text.Length && elapsed < maxSeconds)
        {
            while (i < text.Length && char.IsWhiteSpace(text[i])) i++;
            if (i >= text.Length) break;

            int start = i;
            while (i < text.Length && !char.IsWhiteSpace(text[i])) i++;
            string word = text.Substring(start, i - start);

            int blips = Mathf.Max(1, Mathf.RoundToInt(word.Length / lettersPerBlip));
            for (int b = 0; b < blips && elapsed < maxSeconds; b++)
            {
                babbleSource.PlayOneShot(babbleBlip, babbleVolume);
                yield return blipWait;
                elapsed += babbleInterval;
            }

            char last = word[word.Length - 1];
            float pause = (last == '.' || last == '!' || last == '?' || last == '…') ? sentencePause
                        : (last == ',' || last == ';') ? commaPause
                        : wordGap;
            yield return new WaitForSecondsRealtime(pause);
            elapsed += pause;
        }
        babbleRoutine = null;
    }

    public static void StopBabble()
    { if (Instance != null) Instance.StopBabbleInternal(); }



    void StopBabbleInternal()
    {
        if (babbleRoutine != null) { StopCoroutine(babbleRoutine); babbleRoutine = null; }
        if (babbleSource != null) babbleSource.Stop();
    }


}