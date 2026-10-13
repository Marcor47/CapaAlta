using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DashboardDonutChart : MonoBehaviour
{
    [Header("Segmentos (Image, Filled, Radial 360)")]
    public Image positiveSegment;
    public Image neutralSegment;
    public Image negativeSegment;

    [Header("Textos")]
    public TextMeshProUGUI centerText;
    public TextMeshProUGUI legendText;

    public void SetValues(int positive, int neutral, int negative)
    {
        int total = positive + neutral + negative;

        if (total == 0)
        {
            positiveSegment.gameObject.SetActive(false);
            neutralSegment.gameObject.SetActive(false);
            negativeSegment.gameObject.SetActive(false);
            if (centerText != null) centerText.text = "0";
            if (legendText != null) legendText.text = "Sin decisiones registradas";
            return;
        }

        float p = (float)positive / total;
        float n = (float)neutral / total;
        float g = (float)negative / total;

        ApplySegment(positiveSegment, 0f, p);
        ApplySegment(neutralSegment, p, n);
        ApplySegment(negativeSegment, p + n, g);

        if (centerText != null) centerText.text = total.ToString();
        if (legendText != null)
            legendText.text = $"Positivas: {positive} ({Mathf.RoundToInt(p * 100)}%)\n" +
                              $"Neutrales: {neutral} ({Mathf.RoundToInt(n * 100)}%)\n" +
                              $"Negativas: {negative} ({Mathf.RoundToInt(g * 100)}%)";
    }

    void ApplySegment(Image segment, float start, float fraction)
    {
        segment.gameObject.SetActive(fraction > 0f);
        segment.fillAmount = fraction;
        segment.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -start * 360f);
    }
}