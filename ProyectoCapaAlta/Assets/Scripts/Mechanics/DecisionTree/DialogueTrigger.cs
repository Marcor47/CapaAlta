using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class DialogueTrigger : MonoBehaviour
{
    // ─── DATOS ─────────────────────────────────────────────────
    [Header("Datos del encuentro")]
    public DialogueNode dialogueNode;

    [Tooltip("NodeID del encuentro anterior requerido para que este NPC aparezca")]
    [SerializeField] private string requiredPreviousNodeID = "";
    // Dejar vacío si no requiere encuentro previo (ej: primer encuentro de Gaz)

    // ─── ESTADO ────────────────────────────────────────────────
    private bool playerInRange = false;
    private bool isCompleted = false;
    private bool canInteract = true;

    // ──────────────────────────────────────────────────────────
    void Start()
    {
        // Verifica si este NPC debe aparecer según encuentros previos
        if (!string.IsNullOrEmpty(requiredPreviousNodeID))
        {
            if (DecisionRecord.Instance != null &&
                !DecisionRecord.Instance.CanAppear(requiredPreviousNodeID))
            {
                // El encuentro anterior no fue completado — oculta el NPC
                gameObject.SetActive(false);
                return;
            }
        }

        // Verifica si este encuentro ya fue completado (ej: volvió al capítulo)
        if (DecisionRecord.Instance != null &&
            DecisionRecord.Instance.IsNodeCompleted(dialogueNode.nodeID))
        {
            isCompleted = true;
            canInteract = false;
        }
    }

    void Update()
    {
        if (!playerInRange || !canInteract || isCompleted) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
            StartDialogue();
    }

    // ─── TRIGGER ───────────────────────────────────────────────
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (isCompleted || !canInteract) return;

        playerInRange = true;
        // TODO: UIHintManager.Instance.Show("Presiona E para hablar");
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        // TODO: UIHintManager.Instance.Hide();
    }

    // ─── INICIAR DIÁLOGO ───────────────────────────────────────
    void StartDialogue()
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("[DialogueTrigger] DialogueManager.Instance es null");
            return;
        }

        canInteract = false;
        DialogueManager.Instance.StartDialogue(dialogueNode, OnDialogueComplete);
    }

    // ─── CALLBACK AL TERMINAR ──────────────────────────────────
    void OnDialogueComplete(int chosenOptionIndex)
    {
        isCompleted = true;

        // Registrar encuentro como completado
        if (DecisionRecord.Instance != null)
            DecisionRecord.Instance.MarkNodeCompleted(dialogueNode.nodeID);

        // Si este NPC da la manzana tutorial al terminar
        if (dialogueNode.givesManzanaOnEnd && DecisionRecord.Instance != null)
            DecisionRecord.Instance.AddManzana();

        Debug.Log($"[DialogueTrigger] Encuentro completado: {dialogueNode.nodeID} — opción elegida: {chosenOptionIndex}");
    }

    // ─── PROPIEDADES PÚBLICAS ──────────────────────────────────
    public bool IsCompleted => isCompleted;
    public bool PlayerInRange => playerInRange;

    // ─── DEBUG ─────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }
}