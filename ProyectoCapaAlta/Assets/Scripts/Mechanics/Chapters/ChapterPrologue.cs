using System.Collections;
using UnityEngine;

/// <summary>
/// Prólogo del Cap1: pantalla en negro, timbre del teléfono (2 "ring ring") y llamada Theo-Mamá.
/// Lo llama FallRespawn al entrar al capítulo desde el inicio (no al continuar desde un checkpoint).
/// </summary>
public class ChapterPrologue : MonoBehaviour
{
    [Header("Llamada")]
    public string speakerName = "Mamá";
    public DialogueLine[] lines;

    [Header("Timbre")]
    public int ringRepeats = 2;
    public float ringGap = 0.2f;           // pausa entre los dos clicks de un "ring ring"
    public float timeBetweenRings = 1f;    // pausa entre un "ring ring" y el siguiente
    public float blackHold = 0.8f;         // negro antes del primer ring
    public float afterRingsPause = 0.8f;   // pausa tras el último ring, antes del diálogo

    public IEnumerator Play()
    {
        yield return new WaitForSecondsRealtime(blackHold);

        for (int i = 0; i < ringRepeats; i++)
        {
            AudioManager.PlayClick();
            yield return new WaitForSecondsRealtime(ringGap);
            AudioManager.PlayClick();
            bool last = i == ringRepeats - 1;
            yield return new WaitForSecondsRealtime(last ? afterRingsPause : timeBetweenRings);
        }

        var dm = DialogueManager.Instance;
        if (dm == null || dm.dialoguePanel == null || lines == null || lines.Length == 0) yield break;
        // El ScreenFader (orden 1000) taparía el diálogo: subo el UICanvas por encima solo durante el prólogo.
        var uiCanvas = dm.GetComponentInParent<Canvas>();
        Canvas root = uiCanvas != null ? uiCanvas.rootCanvas : null;
        int prevOrder = root != null ? root.sortingOrder : 0;
        if (root != null) root.sortingOrder = 1001;

        dm.ShowSimpleLines(speakerName, lines);
        yield return null;
        yield return new WaitWhile(() => dm.IsOpen);

        if (root != null) root.sortingOrder = prevOrder;
    }
}