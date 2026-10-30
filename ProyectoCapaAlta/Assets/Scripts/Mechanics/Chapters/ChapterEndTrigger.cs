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

        if (canFinish && Keyboard.current.eKey.wasPressedThisFrame)
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
            // Solo para pruebas: abriste Cap1 sin pasar por LoginScene
            Debug.LogWarning("[ChapterEndTrigger] No hay AccountManager (¿abriste Cap1 directo?). Se vuelve al menú sin guardar progreso.");
            SceneManager.LoadScene(mainMenuSceneName);
            return;
        }

        AccountManager.Instance.MarkChapterCompleted(chapter);
        AccountManager.Instance.ClearCheckpoint(chapter);

        AccountManager.Instance.justFinishedChapter = true;
        AccountManager.Instance.justFinishedChapterNumber = chapter;

        SceneManager.LoadScene(mainMenuSceneName);
    }
}