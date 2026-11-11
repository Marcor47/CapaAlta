using UnityEngine;

/// <summary>
/// Capa de fondo con parallax. factor = 0 -> fija a la cámara (cielo).
/// factor pequeño -> lejana (casi acompaña a la cámara). factor mayor -> más cercana.
/// Si wrapWidth > 0 la capa se repite sin costura (usar con Draw Mode = Tiled).
/// </summary>
[DefaultExecutionOrder(300)] // después de Cinemachine (200)
public class ParallaxLayer : MonoBehaviour
{
    [Range(0f, 1f)] public float factor = 0f;
    [Tooltip("Ancho del sprite original en unidades (480 px = 15). 0 = sin repetición.")]
    public float wrapWidth = 0f;
    public float yOffset = 0f;
    public bool snapToPixel = true;

    Transform cam;
    const float PPU = 32f;

    void Start() { cam = Camera.main != null ? Camera.main.transform : null; }

    void LateUpdate()
    {
        if (cam == null) { cam = Camera.main != null ? Camera.main.transform : null; if (cam == null) return; }

        Vector3 c = cam.position;
        float shift = 0f;

        if (factor > 0f)
        {
            shift = c.x * factor;
            if (wrapWidth > 0f) shift = Mathf.Repeat(shift, wrapWidth);
            if (snapToPixel) shift = Mathf.Round(shift * PPU) / PPU;   // solo el desplazamiento
        }

        // X e Y siguen a la cámara sin redondear: el offset respecto a la pantalla es constante
        transform.position = new Vector3(c.x - shift, c.y + yOffset, transform.position.z);
    }
}