using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioSettingsPanel : MonoBehaviour
{
    [Header("UI")]
    public GameObject settingsPanel;
    public Button volumeUpButton;
    public Button volumeDownButton;
    public Button backButton;
    public TextMeshProUGUI volumeText;

    [Header("Configuración")]
    public float volumeStep = 0.1f;

    void Start()
    {
        volumeUpButton.onClick.AddListener(() => ChangeVolume(volumeStep));
        volumeDownButton.onClick.AddListener(() => ChangeVolume(-volumeStep));
        backButton.onClick.AddListener(() => settingsPanel.SetActive(false));
        UpdateVolumeText();
    }

    void ChangeVolume(float delta)
    {
        AudioListener.volume = Mathf.Clamp01(AudioListener.volume + delta);
        UpdateVolumeText();
    }

    void UpdateVolumeText()
    {
        if (volumeText != null)
            volumeText.text = Mathf.RoundToInt(AudioListener.volume * 100f) + "%";
    }
}