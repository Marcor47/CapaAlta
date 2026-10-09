using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChaptersPanel : MonoBehaviour
{
    public ChapterCardUI[] cards;               // las 4 tarjetas, en orden
    public TextMeshProUGUI statusText;
    public string sceneNameFormat = "Cap{0}";   // Cap1, Cap2, Cap3, Cap4

    void OnEnable() => Refresh();

    void Refresh()
    {
        statusText.text = "";

        foreach (var card in cards)
        {
            int chapter = card.chapter;
            bool unlocked = CourseSettings.Instance != null && CourseSettings.Instance.IsChapterUnlocked(chapter);
            bool started = AccountManager.Instance.HasStartedChapter(chapter);

            card.titleText.text = $"Capítulo {chapter}";
            card.lockIcon.SetActive(!unlocked);
            card.buttonsGroup.SetActive(unlocked);
            card.startButton.gameObject.SetActive(unlocked && !started);
            card.continueButton.gameObject.SetActive(unlocked && started);
            card.restartButton.gameObject.SetActive(unlocked && started);

            card.startButton.onClick.RemoveAllListeners();
            card.continueButton.onClick.RemoveAllListeners();
            card.restartButton.onClick.RemoveAllListeners();

            int captured = chapter;
            card.startButton.onClick.AddListener(() => Launch(captured, true));
            card.continueButton.onClick.AddListener(() => Launch(captured, false));
            card.restartButton.onClick.AddListener(() => Launch(captured, true));
        }
    }

    void Launch(int chapter, bool fresh)
    {
        string sceneName = string.Format(sceneNameFormat, chapter);

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            statusText.text = $"El Capítulo {chapter} aún está en desarrollo.";
            return;
        }

        if (fresh) AccountManager.Instance.StartChapterFresh(chapter);
        else AccountManager.Instance.PrepareContinue();

        SceneManager.LoadScene(sceneName);
    }
}