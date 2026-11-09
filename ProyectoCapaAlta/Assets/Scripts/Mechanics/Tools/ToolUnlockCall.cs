using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class ToolUnlockCall : MonoBehaviour
{
    public static bool AnyRinging { get; private set; }
    public static int LastAnswerFrame { get; private set; } = -1;

    [Header("Herramienta que se desbloquea")]
    public InventorySystem.ItemType toolToUnlock = InventorySystem.ItemType.Onda;

    [Header("Llamada de mamá")]
    public string speakerName = "Mamá";
    public DialogueLine[] callLines;

    [Header("Si el capítulo ya se jugó")]
    public bool ringAgainIfAlreadyUnlocked = true;
    public DialogueLine[] replayCallLines;   // opcional; vacío = usa callLines

    private bool alreadyUnlocked;

    [Header("UI y sonido")]
    public GameObject ringingPromptUI;  // "Mamá te está llamando: equipa el teléfono (mantén E) y haz click"
    public GameObject unlockedToastUI;  // opcional: "Herramienta desbloqueada"
    public float toastSeconds = 3f;
    

    [Header("Timbre (usa el click de botón)")]
    public float ringGap = 0.2f;          // pausa entre los 2 sonidos
    public float ringRepeatEvery = 0f;    // 0 = suena solo una vez; >0 = repite el par cada N s mientras suena
    private Coroutine ringRoutine;

    private TheoController theo;
    private bool ringing;
    private bool done;

    private bool lockedMovement;
    private bool calling;   // Theo con el teléfono en la oreja durante esta llamada


    void Start() => theo = FindAnyObjectByType<TheoController>();

    void OnTriggerEnter2D(Collider2D other)
    {
        if (done || ringing || !other.CompareTag("Player")) return;

        alreadyUnlocked = InventorySystem.Instance != null && InventorySystem.Instance.IsUnlocked(toolToUnlock);
        if (alreadyUnlocked && !ringAgainIfAlreadyUnlocked)
        {
            done = true;
            return;
        }

        ringing = true;
        AnyRinging = true;
        lockedMovement = theo != null && InventorySystem.Instance != null
                 && InventorySystem.Instance.IsUnlocked(InventorySystem.ItemType.Telefono);
        if (lockedMovement) theo.SetMovementLocked(true);
        if (ringingPromptUI != null) ringingPromptUI.SetActive(true);
        ringRoutine = StartCoroutine(RingRoutine());
    }

    void Update()
    {
        if (!ringing) return;

        var mouse = Mouse.current;
        var inventory = InventorySystem.Instance;
        if (mouse == null || inventory == null || theo == null) return;
        if (theo.IsBackpackOpen || theo.IsSitting || theo.IsInDialogue) return;
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen) return;

        if (inventory.Equipped == InventorySystem.ItemType.Telefono && mouse.leftButton.wasPressedThisFrame)
        {
            ringing = false;
            AnyRinging = false;
            LastAnswerFrame = Time.frameCount;
            if (ringingPromptUI != null) ringingPromptUI.SetActive(false);
            StopRing();
            UnlockMovement();
            StartCoroutine(AnswerRoutine());
        }
    }

    void UnlockMovement()
    {
        if (lockedMovement && theo != null) theo.SetMovementLocked(false);
        lockedMovement = false;
    }

    void OnDestroy()
    {
        StopRing();
        if (ringing) AnyRinging = false;
        UnlockMovement();
        EndCallPose();
    }

    IEnumerator AnswerRoutine()
    {
        done = true;

        var lines = (alreadyUnlocked && replayCallLines != null && replayCallLines.Length > 0)
            ? replayCallLines : callLines;

        if (DialogueManager.Instance != null && lines != null && lines.Length > 0)
        {
            StartCallPose();
            DialogueManager.Instance.ShowSimpleLines(speakerName, lines);
            yield return null;
            yield return new WaitWhile(() => DialogueManager.Instance.IsOpen);
            EndCallPose();
        }

        if (!alreadyUnlocked)
        {
            InventorySystem.Instance.UnlockItem(toolToUnlock);

            if (unlockedToastUI != null)
            {
                unlockedToastUI.SetActive(true);
                yield return new WaitForSeconds(toastSeconds);
                unlockedToastUI.SetActive(false);
            }
        }
    }

    IEnumerator RingRoutine()
    {
        var gap = new WaitForSeconds(ringGap);
        while (ringing)
        {
            AudioManager.PlayClick();
            yield return gap;
            AudioManager.PlayClick();
            if (ringRepeatEvery <= 0f) break;
            yield return new WaitForSeconds(ringRepeatEvery);
        }
        ringRoutine = null;
    }

    void StopRing()
    {
        if (ringRoutine != null) { StopCoroutine(ringRoutine); ringRoutine = null; }
    }

    void StartCallPose()
    {
        if (theo == null || !theo.IsGrounded) return;   // en el aire no tiene sentido la pose
        theo.SetCallingMom(true);                        // dispara TakeOut y luego el loop
        calling = true;
    }

    void EndCallPose()
    {
        if (calling && theo != null) theo.SetCallingMom(false);
        calling = false;
    }

}