using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EpistolaryResponsePanel : MonoBehaviour
{
    [Header("Cara frontal (carta original)")]
    public GameObject frontSide;
    public TextMeshProUGUI frontAuthorText;
    public TextMeshProUGUI frontCardText;
    public Image frontCardImage;

    [Header("Cara trasera (respuesta del alumno)")]
    public GameObject backSide;
    public TMP_InputField reflectionInput;
    public Transform hseTogglesContainer;
    public GameObject hseTogglePrefab;
    public Button saveButton;

    [Header("Botón único de voltear")]
    public Button flipButton;

    [Header("Lista oficial de HSE (13)")]
    public List<string> hseNames = new List<string>();

    private string currentCardID;
    private List<Toggle> hseToggles = new List<Toggle>();
    private LoginSceneController loginController;
    private bool showingFront = true;


    public static EpistolaryResponsePanel Instance { get; private set; }

    void Start()
    {
        flipButton.onClick.AddListener(() => SetSide(!showingFront));
        saveButton.onClick.AddListener(SaveResponse);
        loginController = FindAnyObjectByType<LoginSceneController>();
        BuildHSEToggles();
    }

    void Awake()
    {
        Instance = this;
    }

    public void OpenFor(string cardID, string authorName, string cardText, Sprite cardSprite)
    {
        currentCardID = cardID;
        frontAuthorText.text = authorName;
        frontCardText.text = cardText;
        frontCardImage.sprite = cardSprite;

        var existing = FindExistingResponse();
        reflectionInput.text = existing != null ? existing.reflectionText : "";
        SetTogglesFrom(existing);

        SetSide(true);
        gameObject.SetActive(true);
    }

    void SetSide(bool showFront)
    {
        showingFront = showFront;
        frontSide.SetActive(showFront);
        backSide.SetActive(!showFront);
    }

    void BuildHSEToggles()
    {
        foreach (Transform child in hseTogglesContainer) Destroy(child.gameObject);
        Canvas.ForceUpdateCanvases();
        hseToggles.Clear();

        foreach (var hse in hseNames)
        {
            GameObject go = Instantiate(hseTogglePrefab, hseTogglesContainer);
            var label = go.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = hse;
            hseToggles.Add(go.GetComponent<Toggle>());
        }
    }

    void SetTogglesFrom(EpistolaryResponse existing)
    {
        for (int i = 0; i < hseToggles.Count; i++)
            hseToggles[i].isOn = existing != null && existing.linkedHSE.Contains(hseNames[i]);
    }

    EpistolaryResponse FindExistingResponse()
    {
        var progress = AccountManager.Instance.CurrentUser.progress;
        return progress.epistolaryResponses.Find(r => r.cardID == currentCardID);
    }

    void SaveResponse()
    {
        var progress = AccountManager.Instance.CurrentUser.progress;
        var existing = FindExistingResponse();

        List<string> linked = new List<string>();
        for (int i = 0; i < hseToggles.Count; i++)
            if (hseToggles[i].isOn) linked.Add(hseNames[i]);

        if (existing != null)
        {
            existing.reflectionText = reflectionInput.text;
            existing.linkedHSE = linked;
        }
        else
        {
            progress.epistolaryResponses.Add(new EpistolaryResponse
            {
                cardID = currentCardID,
                reflectionText = reflectionInput.text,
                linkedHSE = linked
            });
        }

        AccountManager.Instance.SaveProgress();
        if (loginController != null) { loginController.UpdatePendingBadge(); loginController.GoBack(); }
    }
}