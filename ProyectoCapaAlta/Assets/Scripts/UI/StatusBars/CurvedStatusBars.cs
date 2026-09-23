using UnityEngine;
using UnityEngine.UI;

public class CurvedStatusBars : MonoBehaviour
{
    [Header("Referencias")]
    public TheoController theo;
    public Image motivacionRing;   // Image Type = Filled, Radial 360
    public Image regulacionRing;   // Image Type = Filled, Radial 360

    [Header("Suavizado visual")]
    public float fillLerpSpeed = 6f;

    void Update()
    {
        if (theo == null) return;

        UpdateRing(motivacionRing, theo.StaminaPercent01);
        UpdateRing(regulacionRing, theo.RegulacionPercent01);
    }

    void UpdateRing(Image ring, float targetPercent01)
    {
        if (ring == null) return;

        // Solo visible cuando está por debajo del máximo
        bool shouldShow = targetPercent01 < 0.999f;
        if (ring.gameObject.activeSelf != shouldShow)
            ring.gameObject.SetActive(shouldShow);

        if (shouldShow)
            ring.fillAmount = Mathf.Lerp(ring.fillAmount, targetPercent01, Time.deltaTime * fillLerpSpeed);
    }
}