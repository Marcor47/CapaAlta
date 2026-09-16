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

    private int currentRoundIndex = 0;
    private int pendingRelationshipDelta = 0;
    private List<int> chosenOptionsThisNode = new List<int>();
    private int entryRelationship = 0;

    // Modo simple: solo líneas, sin rondas ni opciones (usado por ManzanaGiver)
    private bool simpleMode = false;
    private string simpleSpeakerName;

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

    private DialogueRound CurrentRound => currentNode.rounds[currentRoundIndex];

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

        if (!choosingOption && waitingInput)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
                AdvanceLine();
        }
    }

    // ─── INICIAR DIÁLOGO (encuentro normal, con nodo) ──────────
    // StartDialogue() — reemplaza cómo se llena currentLines al inicio:
    public void StartDialogue(DialogueNode node, System.Action<int> callback)
    {
        currentNode = node;
        onDialogueComplete = callback;
        chosenOptionIndex = -1;
        currentLineIndex = 0;
        currentRoundIndex = 0;
        pendingRelationshipDelta = 0;
        chosenOptionsThisNode.Clear();

        entryRelationship = DecisionRecord.Instance != null
            ? DecisionRecord.Instance.GetRelationship(node.npcID)
            : 0;

        state = DialogueState.Opening;
        isOpen = true;
        inputCooldown = 0.15f;

        if (theo != null) theo.SetDialogueState(true);

        dialoguePanel.SetActive(true);
        optionsPanel.SetActive(false);
        SetNPCSpeaking(true);

        currentLines.Clear();
        DialogueLine[] opening = CurrentRound.GetOpeningLines(entryRelationship);
        if (opening != null)
            currentLines.AddRange(opening);

        ShowCurrentLine();
    }

    // ─── MOSTRAR LÍNEAS SIMPLES (sin nodo, sin opciones) ───────
    // Usado por ManzanaGiver.cs para la reacción al dar una manzana
    public void ShowSimpleLines(string speakerName, DialogueLine[] lines)
    {
        simpleMode = true;
        simpleSpeakerName = speakerName;
        currentNode = null;
        onDialogueComplete = null;
        chosenOptionIndex = -1;
        currentLineIndex = 0;
        state = DialogueState.Responding;
        isOpen = true;
        inputCooldown = 0.15f;

        if (theo != null) theo.SetDialogueState(true);

        dialoguePanel.SetActive(true);
        optionsPanel.SetActive(false);
        SetNPCSpeaking(true);

        currentLines.Clear();
        if (lines != null) currentLines.AddRange(lines);

        ShowCurrentLine();
    }

    // ─── MOSTRAR LÍNEA ACTUAL ──────────────────────────────────
    void ShowCurrentLine()
    {
        if (currentLineIndex < currentLines.Count)
        {
            DialogueLine line = currentLines[currentLineIndex];

            string speaker;
            if (simpleMode)
            {
                speaker = string.IsNullOrEmpty(line.speakerName) ? simpleSpeakerName : line.speakerName;
            }
            else
            {
                speaker = currentNode.isSingleSpeaker
                    ? currentNode.npcName
                    : (string.IsNullOrEmpty(line.speakerName) ? currentNode.npcName : line.speakerName);
            }

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
        if (simpleMode)
        {
            EndDialogue();
            return;
        }

        switch (state)
        {
            case DialogueState.Opening:
                ShowOptions();
                break;

            case DialogueState.Responding:
                // AdvanceState() — dentro del case Responding, cuando pasa a la siguiente ronda:
                if (currentRoundIndex < currentNode.rounds.Length - 1)
                {
                    currentRoundIndex++;
                    state = DialogueState.Opening;
                    currentLineIndex = 0;
                    currentLines.Clear();

                    DialogueLine[] opening = CurrentRound.GetOpeningLines(entryRelationship);
                    if (opening != null)
                        currentLines.AddRange(opening);

                    SetNPCSpeaking(true);
                    ShowCurrentLine();
                }
                else
                {
                    if (DecisionRecord.Instance != null)
                    {
                        int finalDelta = Mathf.Clamp(pendingRelationshipDelta, -1, 1);
                        DecisionRecord.Instance.SetRelationship(currentNode.npcID, finalDelta);
                    }

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
                }
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

        DialogueOption[] roundOptions = CurrentRound.options;

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < roundOptions.Length)
            {
                optionButtons[i].gameObject.SetActive(true);
                optionTexts[i].text = roundOptions[i].optionText;
            }
            else optionButtons[i].gameObject.SetActive(false);
        }
    }

    // ─── ELEGIR OPCIÓN ─────────────────────────────────────────
    void ChooseOption(int index)
    {
        DialogueOption[] roundOptions = CurrentRound.options;
        if (index >= roundOptions.Length) return;

        SetNPCSpeaking(true);
        chosenOptionIndex = index;
        chosenOptionsThisNode.Add(index);
        choosingOption = false;
        optionsPanel.SetActive(false);
        inputCooldown = 0.15f;

        DialogueOption chosen = roundOptions[index];
        pendingRelationshipDelta += chosen.relationshipDelta;

        if (DecisionRecord.Instance != null)
            DecisionRecord.Instance.RecordChoice(
                currentNode.nodeID, currentNode.npcID,
                currentRoundIndex, index, chosen.relationshipDelta);

        state = DialogueState.Responding;
        currentLineIndex = 0;
        currentLines.Clear();

        if (chosen.npcResponseLines != null)
            currentLines.AddRange(chosen.npcResponseLines);

        // NUEVO: agrega el cierre común de la ronda, si lo tiene
        if (CurrentRound.sharedFollowUpLines != null)
            currentLines.AddRange(CurrentRound.sharedFollowUpLines);

        ShowCurrentLine();
    }

    // ─── CERRAR DIÁLOGO ────────────────────────────────────────
    void EndDialogue()
    {
        state = DialogueState.Done;
        isOpen = false;
        simpleMode = false;

        dialoguePanel.SetActive(false);
        optionsPanel.SetActive(false);

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
    public List<int> LastNodeChoices => new List<int>(chosenOptionsThisNode);
}