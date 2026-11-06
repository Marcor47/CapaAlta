using System.Collections;
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

    [Header("Salida")]
    public float maxWalkTime = 3f;     // tope por si la cámara sigue a Theo y no sale de pantalla
    public float fadeOutTime = 0.5f;

    private bool playerInRange = false;
    private bool finishing;

    void Update()
    {
        if (finishing) return;
        if (!playerInRange || promptUI == null) return;

        bool canFinish = DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen;
        promptUI.SetActive(canFinish);

        var kb = Keyboard.current;
        if (canFinish && (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame))
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
        if (finishing) return;
        finishing = true;
        StartCoroutine(FinishRoutine());
    }

    IEnumerator FinishRoutine()
    {
        if (promptUI != null) promptUI.SetActive(false);

        var theo = FindAnyObjectByType<TheoController>();
        if (theo != null)
        {
            theo.SetMovementLocked(true);
            theo.BeginAutoWalk(1f);

            var cam = Camera.main;
            float t = 0f;
            while (t < maxWalkTime)   // sale por la derecha de la pantalla
            {
                t += Time.unscaledDeltaTime;
                float rightEdge = cam.transform.position.x + cam.orthographicSize * cam.aspect;
                if (theo.transform.position.x > rightEdge + 0.8f) break;
                yield return null;
            }
        }

        yield return ScreenFader.Instance.FadeTo(1f, fadeOutTime);
        SaveAndExit();
    }

    void SaveAndExit()
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