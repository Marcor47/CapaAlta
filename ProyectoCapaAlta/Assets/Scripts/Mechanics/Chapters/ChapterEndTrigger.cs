using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class ChapterEndTrigger : MonoBehaviour
{
    public int chapter;
    public string mainMenuSceneName = "LoginScene";

    [Header("UI")]
    public GameObject promptUI; // ej. "Presiona Enter para terminar el capítulo"

    private bool playerInRange = false;

    void Update()
    {
        if (!playerInRange || promptUI == null) return;

        bool canFinish = DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen;
        promptUI.SetActive(canFinish);

        if (canFinish && Keyboard.current.enterKey.wasPressedThisFrame)
            CompleteChapter();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (promptUI != null) promptUI.SetActive(false);
        }
    }

    public void CompleteChapter()
    {
        if (AccountManager.Instance == null)
        {
            Debug.LogError("¡AccountManager.Instance es NULL! No se puede cambiar de escena.");
            return;
        }

        AccountManager.Instance.MarkChapterCompleted(chapter);
        AccountManager.Instance.ClearCheckpoint(chapter);

        AccountManager.Instance.justFinishedChapter = true;
        AccountManager.Instance.justFinishedChapterNumber = chapter;

        SceneManager.LoadScene(mainMenuSceneName);
    }
}