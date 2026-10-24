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

    [Header("UI y sonido")]
    public GameObject ringingPromptUI;  // "Mamá te está llamando: equipa el teléfono (mantén E) y haz click"
    public GameObject unlockedToastUI;  // opcional: "Herramienta desbloqueada"
    public float toastSeconds = 3f;
    public AudioSource ringtone;

    private TheoController theo;
    private bool ringing;
    private bool done;

    void Start() => theo = FindAnyObjectByType<TheoController>();

    void OnTriggerEnter2D(Collider2D other)
    {
        if (done || ringing || !other.CompareTag("Player")) return;

        // Si ya tiene la herramienta (progreso guardado), no vuelve a sonar
        if (InventorySystem.Instance != null && InventorySystem.Instance.IsUnlocked(toolToUnlock))
        {
            done = true;
            return;
        }

        ringing = true;
        AnyRinging = true;
        if (ringingPromptUI != null) ringingPromptUI.SetActive(true);
        if (ringtone != null) ringtone.Play();
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
            if (ringtone != null) ringtone.Stop();
            StartCoroutine(AnswerRoutine());
        }
    }

    IEnumerator AnswerRoutine()
    {
        done = true;

        if (DialogueManager.Instance != null && callLines != null && callLines.Length > 0)
        {
            DialogueManager.Instance.ShowSimpleLines(speakerName, callLines);
            yield return null;
            yield return new WaitWhile(() => DialogueManager.Instance.IsOpen);
        }

        InventorySystem.Instance.UnlockItem(toolToUnlock);

        if (unlockedToastUI != null)
        {
            unlockedToastUI.SetActive(true);
            yield return new WaitForSeconds(toastSeconds);
            unlockedToastUI.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (ringing) AnyRinging = false; // evita que quede "sonando" al cambiar de escena
    }
}