using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

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
    [Header("Respuesta epistolar")]
    public GameObject epistolaryResponsePanel;

    private List<GameObject> navStack = new List<GameObject>();
    private GameObject currentPanel;


    [Header("Vista 3.2 — Profesor")]
    public TMP_InputField newStudentUsernameInput;
    public TMP_InputField newStudentPasswordInput;
    public Button addStudentButton;
    public Transform studentListContainer;
    public GameObject studentListItemPrefab; // ahora necesita Button "Ver" y Button "Eliminar" además del texto

    [Header("Profesor — hub")]
    public GameObject teacherStudentsPanel;
    public GameObject teacherChaptersPanel;
    public Button teacherStudentsButton;
    public Button teacherChaptersButton;
    public TextMeshProUGUI teacherStatusText;


    [Header("Lista de alumnos — controles")]
    public TextMeshProUGUI studentCountText;       // "40 ALUMNOS"
    public TMP_InputField studentSearchInput;
    public Button[] filterButtons;                 // 3: TODOS, PEND, AL DÍA
    public TextMeshProUGUI[] filterLabels;         // 3, el texto de cada filtro
    public Sprite filterOnSprite;
    public Sprite filterOffSprite;
    public Button sortByNameButton;
    public Image sortArrow;
    public Button groupButton;
    public Button openAddStudentButton;
    public GameObject addStudentPanel;
    public GameObject groupDashboardPanel;         // se conecta en la Parte 3
    public GroupDashboardUI groupDashboardUI;
    public TextMeshProUGUI addStudentErrorText;

    enum StudentFilter { All, Pending, UpToDate }
    StudentFilter currentFilter = StudentFilter.All;
    bool sortAscending = true;
    static readonly Color FilterOffText = new Color32(0x96, 0x42, 0x53, 255);


    [Header("Vista — Dashboard de un alumno")]
    public StudentDashboardUI studentDashboardUI;


    [Header("Persistentes (engranaje / salir)")]
    public GameObject settingsPanel;
    public Button settingsButton;
    public Button exitButton;

    [Header("Ayuda (botón i)")]
    public GameObject helpPanel;
    public Button helpButton;
    public HelpPanelUI helpUI;

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

    [Header("Personajes Compendium")]
    public GameObject compendiumCharactersPanel;

    [Header("Capítulos")]
    public GameObject chaptersPanel;

    [Header("Profesor — habilitar capítulos")]
    public Toggle[] chapterToggles; // 4 toggles, índice 0 = Cap1


    

    void Start()
    {
        errorText.text = "";
        startButton.onClick.AddListener(() => NavigateTo(loginFormPanel));
        loginButton.onClick.AddListener(TryLogin);
        addStudentButton.onClick.AddListener(TryAddStudent);
        
        settingsButton.onClick.AddListener(() => NavigateTo(settingsPanel));

        helpButton.onClick.AddListener(() =>
        {
            if (currentPanel == helpPanel) return;
            NavigateTo(helpPanel);
            helpUI.Open();
        });

        exitButton.onClick.AddListener(HandleBackOrExit);
        notificationCloseButton.onClick.AddListener(() => notificationPanel.SetActive(false));
        
        compendiumButton.onClick.AddListener(() => { NavigateTo(compendiumHomePanel); UpdatePendingBadge(); });
        compendiumCardsButton.onClick.AddListener(() => NavigateTo(compendiumCardsPanel));
        compendiumCharactersButton.onClick.AddListener(() => NavigateTo(compendiumCharactersPanel));

        chaptersButton.onClick.AddListener(() => NavigateTo(chaptersPanel));

        teacherStudentsButton.onClick.AddListener(() =>
        {
            NavigateTo(teacherStudentsPanel);
            teacherStatusText.text = "";
            studentSearchInput.SetTextWithoutNotify("");
            sortAscending = true;
            sortArrow.rectTransform.localRotation = Quaternion.identity;
            SetFilter(StudentFilter.All);   // también refresca la lista
        });

        studentSearchInput.onValueChanged.AddListener(_ => RefreshStudentList());
        for (int i = 0; i < filterButtons.Length; i++)
        {
            int idx = i;
            filterButtons[idx].onClick.AddListener(() => SetFilter((StudentFilter)idx));
        }
        sortByNameButton.onClick.AddListener(() =>
        {
            sortAscending = !sortAscending;
            sortArrow.rectTransform.localRotation = Quaternion.Euler(0f, 0f, sortAscending ? 0f : 180f);
            RefreshStudentList();
        });
        groupButton.onClick.AddListener(() =>
        {
            if (groupDashboardPanel == null) return;
            NavigateTo(groupDashboardPanel);
            groupDashboardUI.Open(AccountManager.Instance.GetAllStudents());
        });
        openAddStudentButton.onClick.AddListener(() => { addStudentErrorText.text = ""; NavigateTo(addStudentPanel); });


        teacherChaptersButton.onClick.AddListener(() =>
        {
            NavigateTo(teacherChaptersPanel);
            SetupChapterToggles();
        });

        if (AccountManager.Instance.CurrentUser != null)
        {
            if (AccountManager.Instance.CurrentUser.role == "Teacher")
            {
                ShowTeacherHome();
            }
            else
            {
                GoHomeAfterLogin(studentHomePanel);
                UpdatePendingBadge();
                if (AccountManager.Instance.returnToChapters)
                {
                    AccountManager.Instance.returnToChapters = false;
                    NavigateTo(chaptersPanel);   // Home queda en la pila: Exit vuelve a Home
                }
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
        if (InventorySystem.Instance != null) InventorySystem.Instance.ResetState();
        usernameInput.text = "";
        passwordInput.text = "";
        errorText.text = "";
        navStack.Clear();
        navStack.Add(startPanel);                    // Salir desde el login vuelve al inicio
        NavigateTo(loginFormPanel, pushCurrent: false);
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
        settingsPanel.SetActive(false);
        epistolaryResponsePanel.SetActive(false);
        chaptersPanel.SetActive(false);
        teacherStudentsPanel.SetActive(false);
        teacherChaptersPanel.SetActive(false);
        compendiumCharactersPanel.SetActive(false);

        if (helpPanel != null) helpPanel.SetActive(false);
        addStudentPanel.SetActive(false);
        if (groupDashboardPanel != null) groupDashboardPanel.SetActive(false);
    }

    void NavigateTo(GameObject panel, bool pushCurrent = true)
    {
        if (panel == currentPanel) return;

        // Si el panel ya está en el historial, volvemos a él en vez de apilar otro
        int idx = navStack.LastIndexOf(panel);
        if (idx >= 0)
        {
            navStack.RemoveRange(idx, navStack.Count - idx);
            pushCurrent = false;
        }

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
            ShowTeacherHome();
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
        string newName = newStudentUsernameInput.text;
        bool added = AccountManager.Instance.AddStudent(newName, newStudentPasswordInput.text);

        if (!added)
        {
            addStudentErrorText.text = "No se pudo agregar (¿usuario vacío o ya existe?).";
            return;
        }

        teacherStatusText.text = $"Alumno \"{newName}\" agregado.";
        newStudentUsernameInput.text = "";
        newStudentPasswordInput.text = "";
        addStudentErrorText.text = "";
        HandleBackOrExit();      // vuelve a la lista
        RefreshStudentList();
    }

    void SetFilter(StudentFilter f)
    {
        currentFilter = f;
        for (int i = 0; i < filterButtons.Length; i++)
        {
            bool on = i == (int)f;
            filterButtons[i].image.sprite = on ? filterOnSprite : filterOffSprite;
            filterLabels[i].color = on ? Color.white : FilterOffText;
        }
        RefreshStudentList();
    }

    void RefreshStudentList()
    {
        for (int i = studentListContainer.childCount - 1; i >= 0; i--)
        {
            var child = studentListContainer.GetChild(i);
            child.SetParent(null);
            Destroy(child.gameObject);
        }

        var all = AccountManager.Instance.GetAllStudents();
        string q = studentSearchInput.text.Trim().ToLowerInvariant();

        var students = all
            .Where(s => q.Length == 0 || s.username.ToLowerInvariant().Contains(q))
            .Where(s => currentFilter == StudentFilter.All
                     || (currentFilter == StudentFilter.UpToDate) == StudentStatus.IsAlDia(s.progress))
            .OrderBy(s => s.username, System.StringComparer.CurrentCultureIgnoreCase)
            .ToList();
        if (!sortAscending) students.Reverse();

        string Alumnos(int n) => n == 1 ? "ALUMNO" : "ALUMNOS";

        studentCountText.text = students.Count == all.Count
            ? $"{all.Count} {Alumnos(all.Count)}"
            : $"{students.Count} DE {all.Count} {Alumnos(all.Count)}";

        foreach (var student in students)
        {
            GameObject go = Instantiate(studentListItemPrefab, studentListContainer);
            string captured = student.username;     // evita el bug de closures en loops
            go.GetComponent<StudentRowUI>().Setup(
                student,
                () => OpenStudentDashboard(captured),
                () => { AccountManager.Instance.DeleteStudent(captured); RefreshStudentList(); });
        }
    }

    // ─── DASHBOARD DE UN ALUMNO (stub — datos reales pendientes) ─
    void OpenStudentDashboard(string username)
    {
        var student = AccountManager.Instance.GetStudent(username);
        if (student == null) return;

        NavigateTo(studentDashboardPanel);
        studentDashboardUI.Open(student);
    }

    

    void ShowTeacherHome()
    {
        GoHomeAfterLogin(teacherHomePanel);
    }

    void SetupChapterToggles()
    {
        for (int i = 0; i < chapterToggles.Length; i++)
        {
            int chapter = i + 1;
            var toggle = chapterToggles[i];

            toggle.onValueChanged.RemoveAllListeners();
            toggle.isOn = CourseSettings.Instance.IsChapterUnlocked(chapter);
            toggle.interactable = chapter != 1; // Cap1 siempre habilitado
            toggle.onValueChanged.AddListener(on => CourseSettings.Instance.SetChapterUnlocked(chapter, on));
        }
    }


}