using System.Collections.Generic;
using UnityEngine;

public class AccountManager : MonoBehaviour
{

    [HideInInspector] public bool justFinishedChapter = false;
    [HideInInspector] public int justFinishedChapterNumber = -1;


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

    [HideInInspector] public bool resumeFromCheckpoint = false;

    public bool HasStartedChapter(int chapter)
        => CurrentUser != null && CurrentUser.progress.chaptersStarted.Contains(chapter);

    // "Iniciar" (primera vez) y "Reiniciar"
    public void StartChapterFresh(int chapter)
    {
        var p = CurrentUser.progress;

        if (!p.chaptersStarted.Contains(chapter))
        {
            p.chaptersStarted.Add(chapter);
            TakeSnapshot(chapter); // foto de las relaciones al entrar por primera vez
        }
        else
        {
            RestoreSnapshot(chapter); // reinicio: vuelve al estado de relaciones del inicio del capítulo
            p.completedNodesSaved.RemoveAll(id => id.StartsWith($"Cap{chapter}_"));
        }

        p.checkpoints.RemoveAll(c => c.chapter == chapter);
        resumeFromCheckpoint = false;
        SaveProgress();

        // Los singletons de gameplay sobreviven entre escenas: hay que refrescarlos
        if (DecisionRecord.Instance != null) DecisionRecord.Instance.LoadFromAccount();
        if (CardInventory.Instance != null) CardInventory.Instance.ResetState();
    }

    public void PrepareContinue() => resumeFromCheckpoint = true;

    void TakeSnapshot(int chapter)
    {
        var p = CurrentUser.progress;
        p.chapterSnapshots.RemoveAll(s => s.chapter == chapter);
        p.chapterSnapshots.Add(new ChapterSnapshot
        {
            chapter = chapter,
            npcKeys = new List<string>(p.npcRelationshipKeys),
            npcValues = new List<int>(p.npcRelationshipValues),
            manzanasAvailable = p.manzanasAvailable,
            manzanasUsedOn = new List<string>(p.manzanasUsedOnSaved)
        });
    }

    void RestoreSnapshot(int chapter)
    {
        var p = CurrentUser.progress;
        var s = p.chapterSnapshots.Find(x => x.chapter == chapter);
        if (s == null) return;
        p.npcRelationshipKeys = new List<string>(s.npcKeys);
        p.npcRelationshipValues = new List<int>(s.npcValues);
        p.manzanasAvailable = s.manzanasAvailable;
        p.manzanasUsedOnSaved = new List<string>(s.manzanasUsedOn);
    }

    public void SaveCheckpoint(int chapter, Vector3 pos)
    {
        if (CurrentUser == null) return;
        var list = CurrentUser.progress.checkpoints;
        var existing = list.Find(c => c.chapter == chapter);
        if (existing == null) { existing = new ChapterCheckpointSave { chapter = chapter }; list.Add(existing); }
        existing.x = pos.x;
        existing.y = pos.y;
        SaveProgress();
    }

    public bool TryGetCheckpoint(int chapter, out Vector2 pos)
    {
        pos = default;
        if (CurrentUser == null) return false;
        var c = CurrentUser.progress.checkpoints.Find(x => x.chapter == chapter);
        if (c == null) return false;
        pos = new Vector2(c.x, c.y);
        return true;
    }

    public void ClearCheckpoint(int chapter)
    {
        if (CurrentUser == null) return;
        CurrentUser.progress.checkpoints.RemoveAll(c => c.chapter == chapter);
        SaveProgress();
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
    public List<int> chaptersStarted = new List<int>();
    public List<ChapterCheckpointSave> checkpoints = new List<ChapterCheckpointSave>();
    public List<ChapterSnapshot> chapterSnapshots = new List<ChapterSnapshot>();

    public List<int> chaptersCompleted = new List<int>();
    public List<CollectedCardRecord> allTimeCollectedCards = new List<CollectedCardRecord>();
    public int decisionsCount = 0;
    public string lastPlayed = "";


    public List<EpistolaryResponse> epistolaryResponses = new List<EpistolaryResponse>();

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
public class ChapterCheckpointSave { public int chapter; public float x; public float y; }

[System.Serializable]
public class ChapterSnapshot
{
    public int chapter;
    public List<string> npcKeys = new List<string>();
    public List<int> npcValues = new List<int>();
    public int manzanasAvailable;
    public List<string> manzanasUsedOn = new List<string>();
}

[System.Serializable]
public class EpistolaryResponse
{
    public string cardID;
    public string reflectionText;
    public List<string> linkedHSE = new List<string>();
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