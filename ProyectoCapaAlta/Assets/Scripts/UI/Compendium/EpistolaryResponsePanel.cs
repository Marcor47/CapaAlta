using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EpistolaryResponsePanel : MonoBehaviour
{
    [Header("Estilo")]
    public DashboardSkin skin;
    public CompendiumSkin comp;

    [Header("Contenedor (LibretaAbierta 720x450)")]
    public RectTransform libreta;

    [Header("Cara frontal (carta original)")]
    public GameObject frontSide;
    public TextMeshProUGUI frontAuthorText;
    public TextMeshProUGUI frontCardText;
    public Image frontCardImage;

    [Header("Cara trasera (respuesta del alumno)")]
    public GameObject backSide;
    public TMP_InputField reflectionInput;
    public Transform hseTogglesContainer;
    public Button saveButton;

    [Header("Botón único de voltear")]
    public Button flipButton;

    [Header("Lista oficial de HSE (13)")]
    public List<string> hseNames = new List<string>();

    static readonly Color Brown = new Color32(0x3D, 0x2B, 0x1F, 255);
    static readonly Color Dark = new Color32(0x2B, 0x2B, 0x3F, 255);

    string currentCardID;
    readonly List<Toggle> hseToggles = new List<Toggle>();
    readonly List<Image> hseBoxes = new List<Image>();
    LoginSceneController loginController;
    bool showingFront = true;
    bool initialized = false;

    public static EpistolaryResponsePanel Instance { get; private set; }

    // ─── helpers de posición (origen arriba-izquierda de la libreta) ───
    static void Put(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(x, -y);
        rt.sizeDelta = new Vector2(w, h);
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    void Style(TMP_Text t, float size, Color c, TextAlignmentOptions a, bool wrap)
    {
        if (skin != null && skin.font != null) t.font = skin.font;
        t.fontSize = size; t.color = c; t.alignment = a;
        t.fontStyle = FontStyles.Normal;
        t.enableAutoSizing = false;
        t.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
        t.overflowMode = TextOverflowModes.Overflow;
    }

    void SetLabel(Button b, string text)
    {
        var t = b.GetComponentInChildren<TMP_Text>(true);
        if (t == null) return;
        Style(t, 16, Dark, TextAlignmentOptions.Center, false);
        t.text = text;
    }

    void StyleButton(Button b)
    {
        var img = b.GetComponent<Image>();
        if (img != null) img.type = Image.Type.Sliced;
    }

    // ─── inicialización ───
    void EnsureInitialized()
    {
        if (initialized) return;
        initialized = true;
        Instance = this;

        flipButton.onClick.AddListener(() => SetSide(!showingFront));
        saveButton.onClick.AddListener(SaveResponse);
        loginController = FindAnyObjectByType<LoginSceneController>();

        ApplyLayout();
        BuildHSEToggles();
    }

    void ApplyLayout()
    {
        // Todo cuelga de la libreta para usar las mismas coordenadas del mock
        foreach (var go in new GameObject[] { frontSide, backSide })
        {
            var rt = (RectTransform)go.transform;
            rt.SetParent(libreta, false);
            Stretch(rt);
        }
        ((RectTransform)flipButton.transform).SetParent(libreta, false);
        ((RectTransform)saveButton.transform).SetParent(libreta, false);
        flipButton.transform.SetAsLastSibling();
        saveButton.transform.SetAsLastSibling();
        StyleButton(flipButton); StyleButton(saveButton);

        // --- Frente ---
        Put(frontCardImage.rectTransform, 54, 36, 252, 378);
        frontCardImage.preserveAspect = false;
        frontCardImage.transform.SetAsFirstSibling();

        Put(frontAuthorText.rectTransform, 68, 52, 224, 24);
        Style(frontAuthorText, 16, Brown, TextAlignmentOptions.TopLeft, false);

        Put(frontCardText.rectTransform, 68, 84, 224, 310);
        Style(frontCardText, 16, Brown, TextAlignmentOptions.TopLeft, true);
        frontCardText.enableAutoSizing = true;   // por si una carta es larga
        frontCardText.fontSizeMin = 8;
        frontCardText.fontSizeMax = 16;

        var hint = DashUI.Text(skin, frontSide.transform,
            "VOLTEA LA CARTA PARA ESCRIBIR TU REFLEXIÓN Y RELACIONARLA CON LAS HSE.",
            400, 222, 230, 120, 16, DashUI.Ink, TextAlignmentOptions.Top, FontStyles.Normal, true);
        hint.name = "HintText";

        // --- Reverso ---
        var l1 = DashUI.Text(skin, backSide.transform, "TU REFLEXIÓN", 42, 36, 300, 24, 16, DashUI.Ink);
        var l2 = DashUI.Text(skin, backSide.transform, "HSE RELACIONADAS", 354, 36, 330, 24, 16, DashUI.Ink);


        var inputRT = (RectTransform)reflectionInput.transform;
        Put(inputRT, 42, 64, 300, 298);
        var inputImg = reflectionInput.GetComponent<Image>();
        if (inputImg != null && comp != null && comp.inputField != null)
        { inputImg.sprite = comp.inputField; inputImg.type = Image.Type.Sliced; }
        reflectionInput.lineType = TMP_InputField.LineType.MultiLineNewline;
        Style(reflectionInput.textComponent, 16, Dark, TextAlignmentOptions.TopLeft, true);
        if (reflectionInput.placeholder is TMP_Text ph)
        {
            Style(ph, 16, new Color32(0xC9, 0xA9, 0x9C, 255), TextAlignmentOptions.TopLeft, true);
            ph.text = "ESCRIBE AQUÍ...";
        }

        var cont = (RectTransform)hseTogglesContainer;
        foreach (var b in cont.GetComponents<Behaviour>())
            if (b is LayoutGroup || b is ContentSizeFitter) b.enabled = false;
        Put(cont, 354, 66, 330, 290);

        // Botones
        Put((RectTransform)saveButton.transform, 198, 372, 144, 40);
        SetLabel(saveButton, "GUARDAR");
    }

    // ─── API ───
    public void OpenFor(string cardID, string authorName, string cardText, Sprite cardSprite)
    {
        EnsureInitialized();
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
        saveButton.gameObject.SetActive(!showFront);

        var rt = (RectTransform)flipButton.transform;
        if (showFront) { Put(rt, 400, 150, 230, 52); SetLabel(flipButton, "VOLTEAR"); }
        else { Put(rt, 42, 372, 144, 40); SetLabel(flipButton, "VER CARTA"); }
    }

    void BuildHSEToggles()
    {
        var cont = (RectTransform)hseTogglesContainer;
        for (int i = cont.childCount - 1; i >= 0; i--)
        {
            var child = cont.GetChild(i);
            child.SetParent(null);
            Destroy(child.gameObject);
        }
        hseToggles.Clear(); hseBoxes.Clear();

        for (int i = 0; i < hseNames.Count; i++)
        {
            var go = new GameObject("HSE_" + i, typeof(RectTransform), typeof(Image), typeof(Toggle));
            var rt = (RectTransform)go.transform;
            rt.SetParent(cont, false);
            Put(rt, 0, i * 22f, 330, 20);

            var bg = go.GetComponent<Image>();
            bg.color = new Color(0, 0, 0, 0);

            var box = DashUI.Img(go.transform, comp.checkOff, 0, 1, 18, 18);
            box.type = Image.Type.Simple;
            DashUI.Text(skin, go.transform, hseNames[i], 26, 0, 304, 20, 16, DashUI.Ink);

            var tg = go.GetComponent<Toggle>();
            tg.transition = Selectable.Transition.None;
            tg.targetGraphic = bg;
            tg.isOn = false;
            var capturedBox = box;
            tg.onValueChanged.AddListener(on => capturedBox.sprite = on ? comp.checkOn : comp.checkOff);

            hseToggles.Add(tg);
            hseBoxes.Add(box);
        }
    }

    void SetTogglesFrom(EpistolaryResponse existing)
    {
        for (int i = 0; i < hseToggles.Count; i++)
        {
            bool on = existing != null && existing.linkedHSE.Contains(hseNames[i]);
            hseToggles[i].SetIsOnWithoutNotify(on);
            hseBoxes[i].sprite = on ? comp.checkOn : comp.checkOff;
        }
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

        var linked = new List<string>();
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