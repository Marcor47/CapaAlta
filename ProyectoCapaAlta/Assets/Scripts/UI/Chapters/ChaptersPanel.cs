using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChaptersPanel : MonoBehaviour
{
    public ChapterCardUI[] cards;               // las 4 tarjetas, en orden
    public TextMeshProUGUI statusText;
    public string sceneNameFormat = "Cap{0}";   // Cap1, Cap2, Cap3, Cap4

    [Header("Sprites de ficha")]
    public Sprite fichaAbierta;
    public Sprite fichaBloqueada;
    public Sprite medallaPendiente;
    public Sprite medallaCurso;
    public Sprite medallaCompleto;
    public Sprite candado;

    void OnEnable() => Refresh();

    void Refresh()
    {
        statusText.text = "";

        foreach (var card in cards)
        {
            int chapter = card.chapter;
            bool unlocked = CourseSettings.Instance != null && CourseSettings.Instance.IsChapterUnlocked(chapter);
            bool started = AccountManager.Instance.HasStartedChapter(chapter);

            card.titleText.text = $"CAP. {chapter}";



            bool completed = AccountManager.Instance.CurrentUser.progress.chaptersCompleted.Contains(chapter);

            card.background.sprite = unlocked ? fichaAbierta : fichaBloqueada;
            card.rutaText.text = card.rutaName;
            card.statusIcon.sprite = !unlocked ? candado : completed ? medallaCompleto : started ? medallaCurso : medallaPendiente;





            bool hasCheckpoint = AccountManager.Instance.TryGetCheckpoint(chapter, out _);
            bool replay = unlocked && completed && !hasCheckpoint;   // terminado, nada en curso
            bool inProgress = unlocked && started && !replay;

            card.buttonsGroup.SetActive(unlocked);
            card.startButton.gameObject.SetActive(unlocked && (!started || replay));
            card.continueButton.gameObject.SetActive(inProgress);
            card.restartButton.gameObject.SetActive(inProgress);

            var startLabel = card.startButton.GetComponentInChildren<TextMeshProUGUI>();
            if (startLabel != null) startLabel.text = replay ? "Rejugar" : "Iniciar";

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

        AccountManager.Instance.returnToChapters = true;
        if (!launching) StartCoroutine(LaunchRoutine(sceneName));
    }

    private bool launching;

    System.Collections.IEnumerator LaunchRoutine(string sceneName)
    {
        launching = true;
        yield return ScreenFader.Instance.FadeTo(1f, 0.4f);
        SceneManager.LoadScene(sceneName);
    }
}