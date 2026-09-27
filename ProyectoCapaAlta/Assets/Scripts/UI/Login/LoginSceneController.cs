using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

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
    public Button dashboardBackButton;

    [Header("Persistentes (engranaje / salir)")]
    public GameObject settingsPanel;
    public Button settingsButton;
    public Button exitButton;

    void Start()
    {
        startButton.onClick.AddListener(ShowLoginForm);
        loginButton.onClick.AddListener(TryLogin);
        addStudentButton.onClick.AddListener(TryAddStudent);
        dashboardBackButton.onClick.AddListener(() => ShowOnly(teacherHomePanel));
        settingsButton.onClick.AddListener(() => settingsPanel.SetActive(true));
        exitButton.onClick.AddListener(HandleExit);

        if (AccountManager.Instance.CurrentUser != null)
        {
            // Ya había una sesión activa (volvió del gameplay vía "Menú Principal") — no repetir login
            if (AccountManager.Instance.CurrentUser.role == "Teacher") { ShowOnly(teacherHomePanel); RefreshStudentList(); }
            else ShowOnly(studentHomePanel);
            return;
        }
        
        ShowOnly(startPanel);
        errorText.text = "";
    }

    void HandleExit()
    {
        if (AccountManager.Instance.CurrentUser != null)
        {
            if (DecisionRecord.Instance != null) DecisionRecord.Instance.ResetState();
            if (CardInventory.Instance != null) CardInventory.Instance.ResetState();
            //if (NotebookManager.Instance != null) NotebookManager.Instance.ResetState();

            AccountManager.Instance.Logout();
            ShowOnly(startPanel);
        }
        else
        {
            Application.Quit(); // no hace nada en WebGL, pero no está de más para una futura build de escritorio
        }
    }

    // ─── NAVEGACIÓN ENTRE VISTAS ─────────────────────────────────
    void ShowOnly(GameObject panelToShow)
    {
        startPanel.SetActive(panelToShow == startPanel);
        loginFormPanel.SetActive(panelToShow == loginFormPanel);
        studentHomePanel.SetActive(panelToShow == studentHomePanel);
        teacherHomePanel.SetActive(panelToShow == teacherHomePanel);
        studentDashboardPanel.SetActive(panelToShow == studentDashboardPanel);
    }

    void ShowLoginForm() => ShowOnly(loginFormPanel);

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
            ShowOnly(teacherHomePanel);
            RefreshStudentList();
        }
        else
        {
            ShowOnly(studentHomePanel);
        }
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

        ShowOnly(studentDashboardPanel);
    }
}