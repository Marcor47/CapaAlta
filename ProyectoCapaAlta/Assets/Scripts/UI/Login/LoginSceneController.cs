using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginSceneController : MonoBehaviour
{
    [Header("Paneles (vistas)")]
    public GameObject startPanel;
    public GameObject loginFormPanel;
    public GameObject studentHomePanel;
    public GameObject teacherHomePanel;
    public GameObject studentDashboardPanel;

    [Header("Vista 1 — Inicio")]
    public Button startButton;

    [Header("Vista 2 — Login")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public TextMeshProUGUI errorText;

    [Header("Vista 3.1 — Alumno")]
    public Button chaptersButton; // TODO: conectar cuando exista el panel de capítulos
    public Button compendiumButton; // TODO: conectar cuando exista el compendio

    [Header("Vista 3.2 — Profesor")]
    public TMP_InputField newStudentUsernameInput;
    public TMP_InputField newStudentPasswordInput;
    public Button addStudentButton;
    public Transform studentListContainer;
    public GameObject studentListItemPrefab; // ahora necesita Button "Ver" y Button "Eliminar" además del texto

    [Header("Vista — Dashboard de un alumno")]
    public TextMeshProUGUI dashboardStudentName;
    public TextMeshProUGUI dashboardChapters;
    public TextMeshProUGUI dashboardCards;
    public TextMeshProUGUI dashboardDecisions;

    public Transform dashboardResponsesContainer; // Content de un Scroll View, con Grid o Vertical Layout + Content Size Fitter
    public GameObject dashboardResponsePrefab;    // 3 TextMeshProUGUI: carta/autor, reflexión, HSE vinculadas
    public TextMeshProUGUI dashboardNoResponsesText; // "Sin respuestas registradas todavía" — opcional

    public DashboardHSEChart hseChart;

    [Header("Persistentes (engranaje / salir)")]
    public GameObject settingsPanel;
    public Button settingsButton;
    public Button exitButton;

    [Header("Notificación fin de capítulo")]
    public GameObject notificationPanel;
    public TextMeshProUGUI notificationText;
    public Button notificationCloseButton;

    [Header("Compendio — indicador pendiente")]
    public GameObject compendiumPendingBadge;


    [Header("Compendio")]
    public GameObject compendiumHomePanel; // botones "Cartas" y "Personajes"
    public Button compendiumCardsButton;
    public Button compendiumCharactersButton; // TODO: sin funcionalidad aún
    public GameObject compendiumCardsPanel;
    


    [Header("Respuesta epistolar")]
    public GameObject epistolaryResponsePanel;

    private List<GameObject> navStack = new List<GameObject>();
    private GameObject currentPanel;

    void Start()
    {
        startButton.onClick.AddListener(() => NavigateTo(loginFormPanel));
        loginButton.onClick.AddListener(TryLogin);
        addStudentButton.onClick.AddListener(TryAddStudent);
        
        settingsButton.onClick.AddListener(() => NavigateTo(settingsPanel));
        exitButton.onClick.AddListener(HandleBackOrExit);
        notificationCloseButton.onClick.AddListener(() => notificationPanel.SetActive(false));
        
        compendiumButton.onClick.AddListener(() => { NavigateTo(compendiumHomePanel); UpdatePendingBadge(); });
        compendiumCardsButton.onClick.AddListener(() => NavigateTo(compendiumCardsPanel));

        if (AccountManager.Instance.CurrentUser != null)
        {
            if (AccountManager.Instance.CurrentUser.role == "Teacher")
            {
                GoHomeAfterLogin(teacherHomePanel);
                RefreshStudentList();
            }
            else
            {
                GoHomeAfterLogin(studentHomePanel);
                UpdatePendingBadge();
                if (AccountManager.Instance.justFinishedChapter)
                {
                    ShowChapterEndNotification();
                    AccountManager.Instance.justFinishedChapter = false;
                }
            }
            return;
        }

        NavigateTo(startPanel, pushCurrent: false); // en vez de ShowOnly(startPanel)
        errorText.text = "";
    }

    void HandleBackOrExit()
    {
        if (currentPanel == startPanel)
        {
            Application.Quit(); // no hace nada en WebGL, sí en una futura build de escritorio
            return;
        }

        if (currentPanel == studentHomePanel || currentPanel == teacherHomePanel)
        {
            DoLogout();
            return;
        }

        if (navStack.Count > 0)
        {
            GameObject previous = navStack[navStack.Count - 1];
            navStack.RemoveAt(navStack.Count - 1);
            NavigateTo(previous, pushCurrent: false);
        }
    }

    void DoLogout()
    {
        if (DecisionRecord.Instance != null) DecisionRecord.Instance.ResetState();
        if (CardInventory.Instance != null) CardInventory.Instance.ResetState();

        AccountManager.Instance.Logout();
        usernameInput.text = "";
        passwordInput.text = "";
        navStack.Clear();
        NavigateTo(loginFormPanel, pushCurrent: false); // antes: startPanel
    }

    // ─── NAVEGACIÓN ENTRE VISTAS ─────────────────────────────────
    void HideAllPanels()
    {
        startPanel.SetActive(false);
        loginFormPanel.SetActive(false);
        studentHomePanel.SetActive(false);
        teacherHomePanel.SetActive(false);
        studentDashboardPanel.SetActive(false);
        compendiumHomePanel.SetActive(false);
        compendiumCardsPanel.SetActive(false);
        settingsPanel.SetActive(false);          // NUEVO
        epistolaryResponsePanel.SetActive(false); // NUEVO
    }

    void NavigateTo(GameObject panel, bool pushCurrent = true)
    {
        if (pushCurrent && currentPanel != null)
            navStack.Add(currentPanel);

        HideAllPanels();
        panel.SetActive(true);
        currentPanel = panel;
    }

    void GoHomeAfterLogin(GameObject homePanel)
    {
        navStack.Clear(); // el Home es un "punto de partida" — Atrás desde acá significa cerrar sesión, no volver al login
        NavigateTo(homePanel, pushCurrent: false);
    }


    // ─── LOGIN ────────────────────────────────────────────────
    void TryLogin()
    {
        if (!AccountManager.Instance.TryLogin(usernameInput.text, passwordInput.text))
        {
            errorText.text = "Usuario o contraseña incorrectos.";
            return;
        }

        errorText.text = "";

        if (AccountManager.Instance.CurrentUser.role == "Teacher")
        {
            GoHomeAfterLogin(teacherHomePanel);
            RefreshStudentList();
        }
        else
        {
            GoHomeAfterLogin(studentHomePanel);
            UpdatePendingBadge();

            if (AccountManager.Instance.justFinishedChapter)
            {
                ShowChapterEndNotification();
                AccountManager.Instance.justFinishedChapter = false;
            }
        }
    }


    public void OpenPanel(GameObject panel) => NavigateTo(panel);
    public void GoBack() => HandleBackOrExit();


    void ShowChapterEndNotification()
    {
        int pending = CountPendingCards();
        if (pending <= 0) return;

        notificationText.text = $"Recolectaste {pending} carta(s) en el Capítulo {AccountManager.Instance.justFinishedChapterNumber}. ¡Respóndelas en el Compendio!";
        notificationPanel.SetActive(true);
    }

    int CountPendingCards()
    {
        var progress = AccountManager.Instance.CurrentUser.progress;
        int count = 0;
        foreach (var card in progress.allTimeCollectedCards)
            if (!progress.epistolaryResponses.Exists(r => r.cardID == card.cardID)) count++;
        return count;
    }

    public void UpdatePendingBadge()
    {
        if (compendiumPendingBadge != null)
            compendiumPendingBadge.SetActive(CountPendingCards() > 0);
    }



    // ─── GESTIÓN DE ALUMNOS ─────────────────────────────────────
    void TryAddStudent()
    {
        bool added = AccountManager.Instance.AddStudent(newStudentUsernameInput.text, newStudentPasswordInput.text);
        if (!added) { errorText.text = "No se pudo agregar (¿usuario vacío o ya existe?)."; return; }

        newStudentUsernameInput.text = "";
        newStudentPasswordInput.text = "";
        RefreshStudentList();
    }

    void RefreshStudentList()
    {
        foreach (Transform child in studentListContainer) Destroy(child.gameObject);

        foreach (var student in AccountManager.Instance.GetAllStudents())
        {
            GameObject go = Instantiate(studentListItemPrefab, studentListContainer);

            var texts = go.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length > 0) texts[0].text = student.username;

            var buttons = go.GetComponentsInChildren<Button>();
            string capturedUsername = student.username; // evita el bug de closures en loops

            // Se asume: buttons[0] = "Ver progreso", buttons[1] = "Eliminar"
            if (buttons.Length > 0) buttons[0].onClick.AddListener(() => OpenStudentDashboard(capturedUsername));
            if (buttons.Length > 1) buttons[1].onClick.AddListener(() => { AccountManager.Instance.DeleteStudent(capturedUsername); RefreshStudentList(); });
        }
    }

    // ─── DASHBOARD DE UN ALUMNO (stub — datos reales pendientes) ─
    void OpenStudentDashboard(string username)
    {
        var student = AccountManager.Instance.GetStudent(username);
        if (student == null) return;

        dashboardStudentName.text = student.username;
        dashboardChapters.text = $"Capítulos completados: {student.progress.chaptersCompleted.Count} / 4";
        dashboardCards.text = $"Cartas recolectadas: {student.progress.allTimeCollectedCards.Count}";
        dashboardDecisions.text = $"Decisiones registradas: {student.progress.decisionsCount}";

        PopulateDashboardResponses(student);
        if (hseChart != null) hseChart.Populate(student);
        NavigateTo(studentDashboardPanel);
    }

    void PopulateDashboardResponses(UserAccount student)
    {
        foreach (Transform child in dashboardResponsesContainer) Destroy(child.gameObject);
        Canvas.ForceUpdateCanvases();

        var responses = student.progress.epistolaryResponses;

        if (dashboardNoResponsesText != null)
            dashboardNoResponsesText.gameObject.SetActive(responses.Count == 0);

        foreach (var response in responses)
        {
            var cardData = CardDatabase.Instance?.GetByID(response.cardID);
            GameObject go = Instantiate(dashboardResponsePrefab, dashboardResponsesContainer);

            var texts = go.GetComponentsInChildren<TextMeshProUGUI>();
            // Se asume: texts[0] = carta/autor, texts[1] = reflexión, texts[2] = HSE vinculadas
            if (texts.Length > 0) texts[0].text = cardData != null ? $"Carta de {cardData.authorName}" : response.cardID;
            if (texts.Length > 1) texts[1].text = response.reflectionText;
            if (texts.Length > 2) texts[2].text = response.linkedHSE.Count > 0 ? string.Join(", ", response.linkedHSE) : "(sin HSE vinculada)";
        }
    }
}