using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChapterCardUI : MonoBehaviour
{
    public int chapter;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI rutaText;        // NUEVO: Árido / Frondoso / Rocoso / Helado
    public string rutaName;                 // NUEVO: se escribe en el Inspector
    public Image background;                // NUEVO: el Image de la ficha
    public Image statusIcon;                // NUEVO: medalla o candado
    public GameObject lockIcon;
    public GameObject buttonsGroup;
    public Button startButton;
    public Button continueButton;
    public Button restartButton;
}