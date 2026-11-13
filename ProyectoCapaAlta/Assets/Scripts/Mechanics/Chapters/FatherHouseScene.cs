using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Casa del padre: Theo en cama y Jason sentado (idle). Con E inicia Cap4_Padre_Enc1.
/// Al terminar: marca Cap4 completado, fundido a negro y carga la escena de finales.
/// </summary>
public class FatherHouseScene : MonoBehaviour
{
    [Header("Conversación")]
    public DialogueNode fatherNode;          // Cap4_Padre_Enc1
    public GameObject promptE;               // "E" sobre el padre/cama

    [Header("Siguiente")]
    public string endingsSceneName = "Finales";
    public float fadeInTime = 1.2f;
    public float fadeOutTime = 1.5f;
    public int chapter = 4;

    bool started, finished;

    IEnumerator Start()
    {
        if (promptE != null) promptE.SetActive(false);
        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.SetAlpha(1f);
            yield return ScreenFader.Instance.FadeTo(0f, fadeInTime);
        }
        if (promptE != null) promptE.SetActive(true);
    }

    void Update()
    {
        if (started || finished) return;
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            started = true;
            if (promptE != null) promptE.SetActive(false);
            DialogueManager.Instance.StartDialogue(fatherNode, OnDialogueComplete);
        }
    }

    void OnDialogueComplete(int _)
    {
        if (finished) return;
        finished = true;

        if (DecisionRecord.Instance != null) DecisionRecord.Instance.MarkNodeCompleted(fatherNode.nodeID);
        if (AccountManager.Instance != null)
        {
            AccountManager.Instance.MarkNpcMet(fatherNode.npcID);
            AccountManager.Instance.MarkChapterCompleted(chapter);
            AccountManager.Instance.ClearCheckpoint(chapter);
        }
        StartCoroutine(Exit());
    }

    IEnumerator Exit()
    {
        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeTo(1f, fadeOutTime);
        SceneManager.LoadScene(endingsSceneName);
    }
}
