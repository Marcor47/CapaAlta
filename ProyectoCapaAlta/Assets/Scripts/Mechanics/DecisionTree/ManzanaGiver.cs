using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(DialogueTrigger))]
public class ManzanaGiver : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Ej. un texto o ícono 'Presiona E para dar manzana'")]
    public GameObject promptUI;

    private DialogueTrigger trigger;
    private bool playerInRange = false;

    void Awake()
    {
        trigger = GetComponent<DialogueTrigger>();
    }

    void Update()
    {
        if (!playerInRange)
        {
            if (promptUI != null) promptUI.SetActive(false);
            return;
        }

        bool canGive = DecisionRecord.Instance != null
            && DecisionRecord.Instance.ManzanasAvailable > 0
            && trigger.IsCompleted
            && DialogueManager.Instance != null
            && !DialogueManager.Instance.IsOpen;

        if (promptUI != null) promptUI.SetActive(canGive);

        if (canGive && Keyboard.current.eKey.wasPressedThisFrame)
            GiveManzana();
    }

    void GiveManzana()
    {
        DialogueNode node = trigger.dialogueNode;
        int before = DecisionRecord.Instance.GetRelationship(node.npcID);
        bool used = DecisionRecord.Instance.TryUseManzana(node.npcID, node.nodeID);
        if (!used) return;

        DialogueLine[] response = node.GetManzanaGivenResponse(before);
        DialogueManager.Instance.ShowSimpleLines(node.npcName, response);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (promptUI != null) promptUI.SetActive(false);
        }
    }
}