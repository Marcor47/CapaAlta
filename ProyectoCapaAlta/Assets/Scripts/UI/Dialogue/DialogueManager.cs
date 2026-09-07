using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    // ─── SINGLETON ─────────────────────────────────────────────
    public static DialogueManager Instance { get; private set; }

    // ─── REFERENCIAS UI ────────────────────────────────────────
    [Header("Panel principal")]
    public GameObject dialoguePanel;

    [Header("Contenido")]
    public TextMeshProUGUI npcNameText;
    public TextMeshProUGUI theoNameText;
    public TextMeshProUGUI dialogueText;

    [Header("Opciones")]
    public GameObject optionsPanel;
    public Button[] optionButtons;
    public TextMeshProUGUI[] optionTexts;

    // ─── ESTADO ────────────────────────────────────────────────
    private DialogueNode currentNode;
    private System.Action<int> onDialogueComplete;

    private List<DialogueLine> currentLines = new List<DialogueLine>();
    private int currentLineIndex = 0;
    private int chosenOptionIndex = -1;

    private bool isOpen = false;
    private bool waitingInput = false;
    private bool choosingOption = false;
    private float inputCooldown = 0f;

    private TheoController theo;

    private enum DialogueState
    {
        Opening, Choosing, Responding, Manzana, Done
    }
    private DialogueState state;

    // ──────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    void Start()
    {
        theo = FindAnyObjectByType<TheoController>();

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i;
            optionButtons[i].onClick.AddListener(() => ChooseOption(index));
        }

        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    void Update()
    {
        if (!isOpen) return;

        if (inputCooldown > 0f) { inputCooldown -= Time.deltaTime; return; }

        // Avanzar con click izquierdo (solo cuando no está eligiendo)
        if (!choosingOption && waitingInput)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
                AdvanceLine();
        }
    }

    // ─── INICIAR DIÁLOGO ───────────────────────────────────────
    public void StartDialogue(DialogueNode node, System.Action<int> callback)
    {
        currentNode = node;
        onDialogueComplete = callback;
        chosenOptionIndex = -1;
        currentLineIndex = 0;
        state = DialogueState.Opening;
        isOpen = true;
        inputCooldown = 0.15f;

        // Bloquear movimiento de Theo
        if (theo != null) theo.SetDialogueState(true);

        dialoguePanel.SetActive(true);
        optionsPanel.SetActive(false);
        SetNPCSpeaking(true);

        currentLines.Clear();
        if (node.openingLines != null)
            currentLines.AddRange(node.openingLines);

        ShowCurrentLine();
    }

    // ─── MOSTRAR LÍNEA ACTUAL ──────────────────────────────────
    void ShowCurrentLine()
    {
        if (currentLineIndex < currentLines.Count)
        {
            DialogueLine line = currentLines[currentLineIndex];

            // Speaker: si la línea tiene nombre propio lo usa, sino usa npcName del nodo
            string speaker = currentNode.isSingleSpeaker
            ? currentNode.npcName
            : (string.IsNullOrEmpty(line.speakerName) ? currentNode.npcName : line.speakerName);

            npcNameText.text = speaker;
            dialogueText.text = line.text;
            waitingInput = true;
        }
        else
        {
            AdvanceState();
        }
    }

    // ─── AVANZAR LÍNEA ─────────────────────────────────────────
    void AdvanceLine()
    {
        currentLineIndex++;
        ShowCurrentLine();
    }

    // ─── AVANZAR ESTADO ────────────────────────────────────────
    void AdvanceState()
    {
        switch (state)
        {
            case DialogueState.Opening:
                ShowOptions();
                break;

            case DialogueState.Responding:
                if (currentNode.givesManzanaOnEnd &&
                    currentNode.manzanaTutorialLines != null &&
                    currentNode.manzanaTutorialLines.Length > 0)
                {
                    state = DialogueState.Manzana;
                    currentLineIndex = 0;
                    currentLines.Clear();
                    currentLines.AddRange(currentNode.manzanaTutorialLines);
                    ShowCurrentLine();
                }
                else EndDialogue();
                break;

            case DialogueState.Manzana:
                EndDialogue();
                break;
        }
    }

    // ─── MOSTRAR OPCIONES ──────────────────────────────────────
    void ShowOptions()
    {
        SetNPCSpeaking(false);
        state = DialogueState.Choosing;
        choosingOption = true;
        waitingInput = false;

        optionsPanel.SetActive(true);
        dialogueText.text = "";

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < currentNode.options.Length)
            {
                optionButtons[i].gameObject.SetActive(true);
                optionTexts[i].text = currentNode.options[i].optionText;
            }
            else optionButtons[i].gameObject.SetActive(false);
        }
    }

    // ─── ELEGIR OPCIÓN ─────────────────────────────────────────
    void ChooseOption(int index)
    {
        if (index >= currentNode.options.Length) return;

        SetNPCSpeaking(true);
        chosenOptionIndex = index;
        choosingOption = false;
        optionsPanel.SetActive(false);
        inputCooldown = 0.15f;

        DialogueOption chosen = currentNode.options[index];

        if (DecisionRecord.Instance != null)
            DecisionRecord.Instance.SetRelationship(currentNode.npcID, chosen.relationshipDelta);

        state = DialogueState.Responding;
        currentLineIndex = 0;
        currentLines.Clear();

        if (chosen.npcResponseLines != null)
            currentLines.AddRange(chosen.npcResponseLines);
        if (chosen.closingLines != null)
            currentLines.AddRange(chosen.closingLines);

        ShowCurrentLine();
    }

    // ─── CERRAR DIÁLOGO ────────────────────────────────────────
    void EndDialogue()
    {
        state = DialogueState.Done;
        isOpen = false;

        dialoguePanel.SetActive(false);
        optionsPanel.SetActive(false);

        // Desbloquear movimiento de Theo
        if (theo != null) theo.SetDialogueState(false);

        onDialogueComplete?.Invoke(chosenOptionIndex);
    }

    // ─── HELPER NPC / THEO SPEAKING ────────────────────────────
    void SetNPCSpeaking(bool npcTalking)
    {
        npcNameText.gameObject.SetActive(npcTalking);
        theoNameText.gameObject.SetActive(!npcTalking);
        dialogueText.gameObject.SetActive(npcTalking);
    }

    // ─── PROPIEDADES PÚBLICAS ──────────────────────────────────
    public bool IsOpen => isOpen;
}
