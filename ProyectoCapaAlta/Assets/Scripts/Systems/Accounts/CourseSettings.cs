using System.Collections.Generic;
using UnityEngine;

public class CourseSettings : MonoBehaviour
{
    public static CourseSettings Instance { get; private set; }
    private const string SaveKey = "CapaAlta_CourseSettings";

    private HashSet<int> unlockedChapters = new HashSet<int> { 1 }; // Cap1 siempre habilitado

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public bool IsChapterUnlocked(int chapter) => chapter == 1 || unlockedChapters.Contains(chapter);

    public void SetChapterUnlocked(int chapter, bool unlocked)
    {
        if (chapter == 1) return; // Cap1 no se bloquea nunca
        if (unlocked) unlockedChapters.Add(chapter);
        else unlockedChapters.Remove(chapter);
        Save();
    }

    void Save()
    {
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(new ChapterListWrapper { chapters = new List<int>(unlockedChapters) }));
        PlayerPrefs.Save();
    }

    void Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return;
        var wrapper = JsonUtility.FromJson<ChapterListWrapper>(PlayerPrefs.GetString(SaveKey));
        if (wrapper?.chapters != null) unlockedChapters = new HashSet<int>(wrapper.chapters);
    }
}

[System.Serializable]
public class ChapterListWrapper { public List<int> chapters; }