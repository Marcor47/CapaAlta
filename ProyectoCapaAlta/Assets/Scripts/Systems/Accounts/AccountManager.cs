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

    public List<UserAccount> GetAllStudents() => accounts.FindAll(a => a.role == "Student");

    public void Logout() => CurrentUser = null;

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
}

[System.Serializable]
public class UserAccount
{
    public string username;
    public string password;
    public string role; // "Teacher" o "Student"
}

[System.Serializable]
public class AccountListWrapper
{
    public List<UserAccount> accounts;
}