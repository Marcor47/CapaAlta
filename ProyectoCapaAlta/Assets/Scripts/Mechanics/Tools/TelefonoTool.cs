using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TelefonoTool : ToolBehaviour
{
    public override InventorySystem.ItemType Type => InventorySystem.ItemType.Telefono;

    [Header("Llamada: espera y enfriamiento")]
    public float minWait = 3f;                    // espera mínima hasta que mamá contesta
    public float maxWait = 5f;                    // espera máxima (también cuando NO contesta)
    [Range(0f, 1f)] public float answerChance = 0.75f;
    public float callCooldown = 8f;               // desde que termina el intento (conteste o no)
    public float dotsInterval = 0.35f;

    [Header("Aviso sobre Theo (RingingPrompt)")]
    public GameObject dotsRoot;                   // RingingPrompt
    public TMP_Text dotsText;                     // el texto TMP dentro de RingingPrompt
    public string noAnswerText = "Mamá no contesta...";
    public string cooldownText = "Sin señal por ahora...";
    public float tempMessageSeconds = 1.5f;

    [Header("Comentarios de mamá (se abren en el DialoguePanel)")]
    public string speakerName = "Mamá";
    [TextArea(1, 3)] public string[] momComments;
    public int linesPerCall = 2;                  // cuántos comentarios dice por llamada

    [Header("Balbuceo (opcional, Bloque 14)")]
    public AudioSource babbleSource;
    public AudioClip[] babbleClips;
    public float babbleSecondsPerCharacter = 0.05f;

    private enum Phase { Idle, Waiting, Connected }
    private static readonly string[] dotFrames = { "...", "..", "." };

    private Phase phase = Phase.Idle;
    private float cooldownUntil;
    private float phaseStart;
    private float answerAfter;
    private bool willAnswer;
    private int lastDotStep = -1;

    private bool dotsOwned;                       // true mientras RingingPrompt lo usa este script
    private string dotsOriginalText;
    private float tempMessageUntil;
    private float babbleStopTime;

    public override void ResetOnRespawn() => EndCall();

    protected override void Awake()
    {
        base.Awake();
        if (dotsText == null)
            Debug.LogWarning("[Telefono] Asigna 'Dots Root' y 'Dots Text' (RingingPrompt) en el Inspector.");
    }

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null || theo == null) return;

        // Si suena una llamada de desbloqueo, el RingingPrompt es de ella
        if (dotsOwned && ToolUnlockCall.AnyRinging) ReleaseDots(true);

        if (tempMessageUntil > 0f && Time.time >= tempMessageUntil)
        {
            tempMessageUntil = 0f;
            ReleaseDots(false);
        }

        if (phase == Phase.Waiting)
        {
            bool stop = InputBlocked || !theo.IsGrounded
                        || mouse.rightButton.wasPressedThisFrame
                        || !mouse.leftButton.isPressed;
            if (stop) { EndCall(); return; }
            UpdateWaiting();
            return;
        }

        if (phase == Phase.Connected)
        {
            UpdateConnected();
            return;
        }

        // Idle: si hay una llamada de desbloqueo sonando, ese click es para contestarla
        bool answering = ToolUnlockCall.AnyRinging || Time.frameCount == ToolUnlockCall.LastAnswerFrame;
        if (InputBlocked || answering || !theo.IsGrounded) return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            if (Time.time < cooldownUntil) ShowTemp(cooldownText);
            else StartCall();
        }
    }

    // ─── LLAMADA ───────────────────────────────────────────────
    void StartCall()
    {
        phase = Phase.Waiting;
        phaseStart = Time.time;
        willAnswer = Random.value < answerChance;
        answerAfter = Random.Range(minWait, maxWait);
        lastDotStep = -1;
        tempMessageUntil = 0f;
        theo.SetCallingMom(true);     // Theo se queda quieto sosteniendo el teléfono
    }

    void UpdateWaiting()
    {
        float t = Time.time - phaseStart;

        int step = Mathf.FloorToInt(t / dotsInterval) % dotFrames.Length;
        if (step != lastDotStep)
        {
            lastDotStep = step;
            ShowDots(dotFrames[step]);
        }

        if (willAnswer && t >= answerAfter) Connect();
        else if (!willAnswer && t >= maxWait) NoAnswer();
    }

    void Connect()
    {
        var lines = PickLines();
        if (lines == null || DialogueManager.Instance == null)
        {
            Debug.LogWarning("[Telefono] Falta 'Mom Comments' en el Inspector o no hay DialogueManager en la escena.");
            EndCall();
            return;
        }

        ReleaseDots(false);                         // quita los puntitos
        phase = Phase.Connected;
        theo.SetMomConnected(true);                 // desde aquí regenera Regulación rápido
        DialogueManager.Instance.ShowSimpleLines(speakerName, lines);
        StartBabble(lines);
    }

    void UpdateConnected()
    {
        // La llamada dura mientras el diálogo de mamá esté abierto
        if (DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen)
        {
            EndCall();
            return;
        }

        if (babbleSource != null && babbleSource.isPlaying && Time.time >= babbleStopTime)
            babbleSource.Stop();
    }

    void NoAnswer()
    {
        EndCall();                                  // libera a Theo y arranca el cooldown
        ShowTemp(noAnswerText);
    }

    void EndCall()
    {
        if (phase == Phase.Idle) return;
        phase = Phase.Idle;
        cooldownUntil = Time.time + callCooldown;   // cuenta siempre, conteste o no

        ReleaseDots(false);
        if (theo != null) theo.SetCallingMom(false);
        if (babbleSource != null) babbleSource.Stop();
    }

    void OnDisable() => EndCall();

    // ─── COMENTARIOS Y BALBUCEO ────────────────────────────────
    DialogueLine[] PickLines()
    {
        if (momComments == null || momComments.Length == 0) return null;

        int n = Mathf.Clamp(linesPerCall, 1, momComments.Length);
        var pool = new System.Collections.Generic.List<string>(momComments);
        var lines = new DialogueLine[n];
        for (int i = 0; i < n; i++)
        {
            int k = Random.Range(0, pool.Count);
            lines[i] = new DialogueLine { speakerName = speakerName, text = pool[k] };
            pool.RemoveAt(k);
        }
        return lines;
    }

    void StartBabble(DialogueLine[] lines)
    {
        if (babbleSource == null || babbleClips == null || babbleClips.Length == 0) return;

        int chars = 0;
        foreach (var l in lines) chars += l.text.Length;

        babbleSource.clip = babbleClips[Random.Range(0, babbleClips.Length)];
        babbleSource.loop = true;
        babbleSource.Play();
        babbleStopTime = Time.time + chars * babbleSecondsPerCharacter;
    }

    // ─── AVISO (RingingPrompt) ─────────────────────────────────
    void ShowDots(string s)
    {
        if (dotsText == null || ToolUnlockCall.AnyRinging) return;

        if (!dotsOwned)
        {
            dotsOriginalText = dotsText.text;       // guarda el texto original (lo usa ToolUnlockCall)
            dotsOwned = true;
            if (dotsRoot != null) dotsRoot.SetActive(true);
        }
        dotsText.text = s;
    }

    void ShowTemp(string text)
    {
        ShowDots(text);
        tempMessageUntil = Time.time + tempMessageSeconds;
    }

    void ReleaseDots(bool keepActive)
    {
        if (!dotsOwned) return;
        dotsOwned = false;
        if (dotsText != null) dotsText.text = dotsOriginalText;     // restaura el texto original
        if (!keepActive && dotsRoot != null) dotsRoot.SetActive(false);
    }
}