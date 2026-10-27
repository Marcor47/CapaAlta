using UnityEngine;
using UnityEngine.InputSystem;

public class TelefonoTool : ToolBehaviour
{
    public override InventorySystem.ItemType Type => InventorySystem.ItemType.Telefono;

    [Header("Comentarios de mamá (se elige uno al azar)")]
    [TextArea(1, 3)] public string[] momComments;
    public float secondsBetweenComments = 5f;

    [Header("Balbuceo (opcional, Bloque 14)")]
    public AudioSource babbleSource;
    public AudioClip[] babbleClips;
    public float babbleSecondsPerCharacter = 0.05f;

    private bool calling;
    private float nextCommentTime;
    private float babbleStopTime;
    private CallBubbleUI bubble;

    protected override void Awake()
    {
        base.Awake();
        bubble = FindAnyObjectByType<CallBubbleUI>();
    }

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null || theo == null) return;

        if (calling)
        {
            bool stop = InputBlocked || !theo.IsGrounded
                        || mouse.rightButton.wasPressedThisFrame
                        || !mouse.leftButton.isPressed;
            if (stop) { EndCall(); return; }

            if (Time.time >= nextCommentTime) SayComment();
            if (babbleSource != null && babbleSource.isPlaying && Time.time >= babbleStopTime)
                babbleSource.Stop();
            return;
        }

        // Si hay una llamada de desbloqueo sonando, ese click es para contestarla
        bool answering = ToolUnlockCall.AnyRinging || Time.frameCount == ToolUnlockCall.LastAnswerFrame;
        if (InputBlocked || answering || !theo.IsGrounded) return;

        if (mouse.leftButton.wasPressedThisFrame) StartCall();
    }

    void StartCall()
    {
        calling = true;
        theo.SetCallingMom(true);
        nextCommentTime = Time.time;   // el primer comentario sale enseguida
    }

    void SayComment()
    {
        nextCommentTime = Time.time + secondsBetweenComments;
        if (momComments == null || momComments.Length == 0) return;

        string text = momComments[Random.Range(0, momComments.Length)];
        if (bubble != null) bubble.Show("Mamá", text);

        if (babbleSource != null && babbleClips != null && babbleClips.Length > 0)
        {
            babbleSource.clip = babbleClips[Random.Range(0, babbleClips.Length)];
            babbleSource.loop = true;
            babbleSource.Play();
            babbleStopTime = Time.time + text.Length * babbleSecondsPerCharacter;
        }
    }

    void EndCall()
    {
        if (!calling) return;
        calling = false;

        if (theo != null) theo.SetCallingMom(false);
        if (bubble != null) bubble.Hide();
        if (babbleSource != null) babbleSource.Stop();
    }

    void OnDisable() => EndCall();
}