using TMPro;
using UnityEngine;

public class CallBubbleUI : MonoBehaviour
{
    public GameObject bubbleRoot;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI messageText;

    void Awake() { if (bubbleRoot != null) bubbleRoot.SetActive(false); }

    public void Show(string speaker, string message)
    {
        if (bubbleRoot == null) return;
        speakerText.text = speaker;
        messageText.text = message;
        bubbleRoot.SetActive(true);
    }

    public void Hide() { if (bubbleRoot != null) bubbleRoot.SetActive(false); }
}