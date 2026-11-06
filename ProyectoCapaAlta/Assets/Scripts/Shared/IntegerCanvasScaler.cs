using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class IntegerCanvasScaler : MonoBehaviour
{
    public float referenceWidth = 960f;
    public float referenceHeight = 540f;

    CanvasScaler scaler;
    int lastW, lastH;

    void Awake()
    {
        scaler = GetComponent<CanvasScaler>();
        Apply();
    }

    void Update()
    {
        if (Screen.width != lastW || Screen.height != lastH) Apply();
    }

    void Apply()
    {
        lastW = Screen.width;
        lastH = Screen.height;

        float fit = Mathf.Min(Screen.width / referenceWidth, Screen.height / referenceHeight);

        // Ventana >= 960x540: escala entera (arte y texto nítidos).
        // Ventana más chica: se ajusta continua para que nada quede cortado.
        float scale = fit >= 1f ? Mathf.Floor(fit) : Mathf.Max(0.25f, fit);

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.scaleFactor = scale;
    }
}