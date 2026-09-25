using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoginSceneController : MonoBehaviour
{
    [Header("Login")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public TextMeshProUGUI errorText;

    [Header("Panel Profesor — gestión de alumnos")]
    public GameObject teacherPanel;
    public TMP_InputField newStudentUsernameInput;
    public TMP_InputField newStudentPasswordInput;
    public Button addStudentButton;
    public Transform studentListContainer; // con Vertical Layout Group
    public GameObject studentListItemPrefab; // prefab con un TextMeshProUGUI

    [Header("Navegación")]
    public string mainMenuSceneName = "MainMenu";

    void Start()
    {
        loginButton.onClick.AddListener(TryLogin);
        addStudentButton.onClick.AddListener(TryAddStudent);
        if (teacherPanel != null) teacherPanel.SetActive(false);
        if (errorText != null) errorText.text = "";
    }

    void TryLogin()
    {
        string user = usernameInput.text;
        string pass = passwordInput.text;

        if (!AccountManager.Instance.TryLogin(user, pass))
        {
            errorText.text = "Usuario o contraseña incorrectos.";
            return;
        }

        errorText.text = "";

        if (AccountManager.Instance.CurrentUser.role == "Teacher")
            OpenTeacherPanel();
        else
            SceneManager.LoadScene(mainMenuSceneName);
    }

    void OpenTeacherPanel()
    {
        teacherPanel.SetActive(true);
        RefreshStudentList();
    }

    void TryAddStudent()
    {
        bool added = AccountManager.Instance.AddStudent(newStudentUsernameInput.text, newStudentPasswordInput.text);
        if (added)
        {
            newStudentUsernameInput.text = "";
            newStudentPasswordInput.text = "";
            RefreshStudentList();
        }
        else
        {
            errorText.text = "No se pudo agregar (¿usuario vacío o ya existe?).";
        }
    }

    void RefreshStudentList()
    {
        foreach (Transform child in studentListContainer) Destroy(child.gameObject);

        foreach (var student in AccountManager.Instance.GetAllStudents())
        {
            GameObject go = Instantiate(studentListItemPrefab, studentListContainer);
            var text = go.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = student.username;
        }
    }
}