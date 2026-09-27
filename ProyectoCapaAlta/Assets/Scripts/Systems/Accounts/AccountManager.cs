using System.Collections.Generic;
using UnityEngine;

public class AccountManager : MonoBehaviour
{
    public static AccountManager Instance { get; private set; }

    private const string SaveKey = "CapaAlta_Accounts";
    private const string TeacherUsername = "Profesor Martin";
    private const string TeacherPassword = "12345";

    private List<UserAccount> accounts = new List<UserAccount>();
    public UserAccount CurrentUser { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAccounts();
        EnsureTeacherExists();
    }

    void EnsureTeacherExists()
    {
        if (accounts.Exists(a => a.username == TeacherUsername)) return;
        accounts.Add(new UserAccount { username = TeacherUsername, password = TeacherPassword, role = "Teacher" });
        SaveAccounts();
    }

    public bool TryLogin(string username, string password)
    {
        var account = accounts.Find(a => a.username == username && a.password == password);
        if (account == null) return false;
        CurrentUser = account;
        return true;
    }

    public bool AddStudent(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) return false;
        if (accounts.Exists(a => a.username == username)) return false; // ya existe

        accounts.Add(new UserAccount { username = username, password = password, role = "Student" });
        SaveAccounts();
        return true;
    }



    public bool DeleteStudent(string username)
    {
        var account = accounts.Find(a => a.username == username && a.role == "Student");
        if (account == null) return false;

        accounts.Remove(account);
        SaveAccounts();
        return true;
    }

    public void SaveProgress() => SaveAccounts();

    public void MarkChapterCompleted(int chapter)
    {
        if (CurrentUser == null) return;
        if (!CurrentUser.progress.chaptersCompleted.Contains(chapter))
            CurrentUser.progress.chaptersCompleted.Add(chapter);
        SaveProgress();
    }


    public UserAccount GetStudent(string username)
    => accounts.Find(a => a.username == username && a.role == "Student");


    public List<UserAccount> GetAllStudents() => accounts.FindAll(a => a.role == "Student");



    // ─── PERSISTENCIA LOCAL (PlayerPrefs) ───────────────────────
    void SaveAccounts()
    {
        var wrapper = new AccountListWrapper { accounts = accounts };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    void LoadAccounts()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return;
        string json = PlayerPrefs.GetString(SaveKey);
        var wrapper = JsonUtility.FromJson<AccountListWrapper>(json);
        if (wrapper != null && wrapper.accounts != null)
            accounts = wrapper.accounts;
    }

    // Agregar a AccountManager:
    public void Logout()
    {
        CurrentUser = null;
    }
}

[System.Serializable]
public class UserAccount
{
    public string username;
    public string password;
    public string role; // "Teacher" o "Student"
    public StudentProgress progress = new StudentProgress();
}

[System.Serializable]
public class StudentProgress
{
    public List<int> chaptersCompleted = new List<int>();
    public List<CollectedCardRecord> allTimeCollectedCards = new List<CollectedCardRecord>();
    public int decisionsCount = 0;
    public string lastPlayed = "";

    // NUEVO — relaciones con NPCs
    public List<string> npcRelationshipKeys = new List<string>();
    public List<int> npcRelationshipValues = new List<int>();
    public List<string> completedNodesSaved = new List<string>();
    public int manzanasAvailable = 0;
    public List<string> manzanasUsedOnSaved = new List<string>();

    // NUEVO — libreta
    //public List<NotebookEntry> notebookEntries = new List<NotebookEntry>();
    public List<int> chaptersUnlocked = new List<int> { 1 };

    // NUEVO — bonos permanentes de barras (Theo no es singleton, así que el bono vive acá)
    public float staminaBonusPermanent = 0f;
    public float regulacionBonusPermanent = 0f;
}

[System.Serializable]
public class CollectedCardRecord
{
    public string cardID;
    public string authorName;
}

[System.Serializable]
public class AccountListWrapper
{
    public List<UserAccount> accounts;
}