using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EmotionalVignetteController : MonoBehaviour
{
    public Volume globalVolume; // el Volume de la escena con un Vignette override agregado
    private Vignette vignette;
    private TheoController theo;

    void Start()
    {
        theo = FindAnyObjectByType<TheoController>();
        globalVolume.profile.TryGet(out vignette);
    }

    void Update()
    {
        if (theo == null || vignette == null) return;
        // A menos regulación, más viñeta (círculo más cerrado)
        vignette.intensity.value = 1f - theo.RegulacionPercent01;
    }
}