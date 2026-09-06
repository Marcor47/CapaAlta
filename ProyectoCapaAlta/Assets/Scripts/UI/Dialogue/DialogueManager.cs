using System.Collections;
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
    public TextMeshProUGUI theoNameText;  // ← nuevo
    public TextMeshProUGUI dialogueText;

    [Header("Opciones")]
    public GameObject optionsPanel;
    public Button[] optionButtons;         // 3 botones de opción
    public TextMeshProUGUI[] optionTexts;  // textos de cada botón

    // ─── ESTADO ────────────────────────────────────────────────
    private DialogueNode currentNode;
    private System.Action<int> onDialogueComplete;

    private List<string> currentLines = new List<string>();
    private int currentLineIndex = 0;
    private int chosenOptionIndex = -1;

    private bool isOpen = false;
    private bool waitingInput = false;
    private bool choosingOption = false;
    private float inputCooldown = 0f;

    // Estados del diálogo
    private enum DialogueState
    {
        Opening,    // mostrando líneas iniciales del NPC
        Choosing,   // jugador elige opción
        Responding, // NPC responde según opción
        Manzana,    // líneas tutorial de manzana (Ryland y Astro)
        Done
    }
    private DialogueState state;

    // ──────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    void Start()
    {
        // Conectar botones de opciones
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

        if (inputCooldown > 0f)
        {
            inputCooldown -= Time.deltaTime;
            return;
        }

        // Avanzar diálogo con E (solo cuando no está eligiendo)
        if (!choosingOption && waitingInput)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
                AdvanceLine();
        }
    }

    // ─── INICIAR DIÁLOGO ───────────────────────────────────────
    public void StartDialogue(DialogueNode node, System.Action<int> callback)
    {
        SetNPCSpeaking(true);
        currentNode = node;
        onDialogueComplete = callback;
        chosenOptionIndex = -1;
        currentLineIndex = 0;
        state = DialogueState.Opening;
        isOpen = true;
        inputCooldown = 0.15f;

        dialoguePanel.SetActive(true);
        optionsPanel.SetActive(false);

        npcNameText.text = node.npcName;

        // Cargar líneas iniciales
        currentLines.Clear();
        currentLines.AddRange(node.openingLines);

        ShowCurrentLine();
    }

    // ─── MOSTRAR LÍNEA ACTUAL ──────────────────────────────────
    void ShowCurrentLine()
    {
        if (currentLineIndex < currentLines.Count)
        {
            dialogueText.text = currentLines[currentLineIndex];
            waitingInput = true;
        }
        else
        {
            // Terminó el bloque actual — avanzar al siguiente estado
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
                // Terminaron las líneas iniciales → mostrar opciones
                ShowOptions();
                break;

            case DialogueState.Responding:
                // Terminó la respuesta del NPC → pasar a manzana o cerrar
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
                else
                {
                    EndDialogue();
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

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < currentNode.options.Length)
            {
                optionButtons[i].gameObject.SetActive(true);
                optionTexts[i].text = currentNode.options[i].optionText;
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }
    }

    // ─── ELEGIR OPCIÓN ─────────────────────────────────────────
    void ChooseOption(int index)
    {
        SetNPCSpeaking(true);
        if (index >= currentNode.options.Length) return;

        chosenOptionIndex = index;
        choosingOption = false;
        optionsPanel.SetActive(false);
        inputCooldown = 0.15f;

        DialogueOption chosen = currentNode.options[index];

        // Registrar cambio de relación
        if (DecisionRecord.Instance != null)
            DecisionRecord.Instance.SetRelationship(
                currentNode.npcID, chosen.relationshipDelta);

        // Cargar respuesta del NPC
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

        onDialogueComplete?.Invoke(chosenOptionIndex);
    }

    // ─── HelperCambioNPCTalking ────────────────────────────────────────
    void SetNPCSpeaking(bool npcTalking)
    {
        npcNameText.gameObject.SetActive(npcTalking);
        theoNameText.gameObject.SetActive(!npcTalking);
        dialogueText.gameObject.SetActive(npcTalking);
    }


    // ─── PROPIEDADES PÚBLICAS ──────────────────────────────────
    public bool IsOpen => isOpen;
}