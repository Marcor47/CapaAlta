using UnityEngine;
using UnityEngine.UI;

public class CurvedStatusBars : MonoBehaviour
{
    [Header("Referencias")]
    public TheoController theo;
    public Image motivacionRing;      // relleno (Filled, Radial 360)
    public Image regulacionRing;      // relleno (Filled, Radial 360)
    public GameObject motivacionMarco;   // MotivacionRing_marco
    public GameObject regulacionMarco;   // RegulacionRing_marco

    [Header("Suavizado visual")]
    public float fillLerpSpeed = 6f;

    void Update()
    {
        if (theo == null) return;

        UpdateRing(motivacionRing, motivacionMarco, theo.StaminaPercent01);
        UpdateRing(regulacionRing, regulacionMarco, theo.RegulacionPercent01);
    }

    void UpdateRing(Image ring, GameObject marco, float targetPercent01)
    {
        if (ring == null) return;

        // Marco y relleno solo son visibles cuando la barra está por debajo del máximo
        bool shouldShow = targetPercent01 < 0.999f;

        if (ring.gameObject.activeSelf != shouldShow)
            ring.gameObject.SetActive(shouldShow);
        if (marco != null && marco.activeSelf != shouldShow)
            marco.SetActive(shouldShow);

        if (shouldShow)
            ring.fillAmount = Mathf.Lerp(ring.fillAmount, targetPercent01, Time.deltaTime * fillLerpSpeed);
    }
}