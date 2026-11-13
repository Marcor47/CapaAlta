using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

/// <summary>
/// Zona de nieve del final de Cap4. Al entrar Theo:
/// camina solo y despacio -> comentario -> prompt Shift -> empujón -> siguiente comentario...
/// Tras el último empujón sale por la derecha, fundido a negro y carga la casa del padre.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SnowWalkSequence : MonoBehaviour
{
    [Header("Comentarios de Theo (uno por Shift)")]
    [TextArea(2, 4)]
    public string[] comments =
    {
        "Hace tanto que salí de casa...",
        "Al principio todo me daba miedo.",
        "Conocí a gente que no esperaba conocer.",
        "Algunos me ayudaron. A otros los ayudé yo.",
        "Hubo días en que quise rendirme.",
        "Pero seguí caminando, aunque fuera despacio.",
        "Ya casi llego."
    };

    [Header("Caminata")]
    public float walkSpeed = 1.2f;          // velocidad lenta constante
    public float dashSpeed = 5f;            // empujón al pulsar Shift
    public float dashTime = 0.35f;
    public float pauseBeforeComment = 0.6f; // respiro entre empujón y siguiente comentario
    
    [Tooltip("Distancia máxima que camina en cada paso (comentario + Shift). Cuenta durante el diálogo y la espera.")]
    public float maxWalkPerStep = 2f;

    [Tooltip("Lo que se pasa del límite al frenar (solo para calcular el piso)")]
    public float stopMargin = 0.25f;

    [Header("Prompt Shift")]
    public GameObject shiftPrompt;          // panel con icono Shift + texto
    public float pulseSpeed = 4f;
    public float pulseAmount = 0.08f;

    [Header("Nieve")]
    public ParticleSystem snow;
    public float snowRateStart = 30f;
    public float snowRateHeavy = 180f;
    public float snowRampTime = 6f;

    [Header("Salida")]
    public CinemachineCamera virtualCamera; // se le quita el Follow al final
    public float fadeOutTime = 1.5f;
    public float maxExitTime = 8f;
    public string nextSceneName = "Cap4_CasaPadre";
    public float exitWalkSpeed = 2f;        // un poco más rápido al salir de pantalla
    public float exitGroundMargin = 6f;     // piso extra para salir (media pantalla + margen)

    bool started;

    void Reset() { GetComponent<Collider2D>().isTrigger = true; }

    void Start()
    {
        if (shiftPrompt != null) shiftPrompt.SetActive(false);
        if (snow != null)
        {
            var e = snow.emission; e.rateOverTime = snowRateStart;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (started || !other.CompareTag("Player")) return;
        var theo = other.GetComponent<TheoController>();
        if (theo == null) return;
        started = true;
        StartCoroutine(Run(theo));
    }

    IEnumerator Run(TheoController theo)
    {
        theo.SetMovementLocked(true);
        theo.autoWalkSpeed = walkSpeed;
        theo.BeginAutoWalk(1f);
        
        if (snow != null) snow.gameObject.SetActive(true);
        StartCoroutine(RampSnow());

        var dm = DialogueManager.Instance;

        for (int i = 0; i < comments.Length; i++)
        {
            theo.autoWalkMaxX = theo.transform.position.x + maxWalkPerStep;
            yield return new WaitForSeconds(pauseBeforeComment);

            // comentario (mientras sigue caminando)
            dm.ShowTheoLines(new[] { comments[i] });
            yield return null;
            while (dm.IsOpen) yield return null;

            // prompt Shift
            if (shiftPrompt != null) shiftPrompt.SetActive(true);
            Vector3 baseScale = shiftPrompt != null ? shiftPrompt.transform.localScale : Vector3.one;
            yield return null; // evita que un Shift previo cuente
            while (!(Keyboard.current != null && Keyboard.current.leftShiftKey.wasPressedThisFrame))
            {
                if (shiftPrompt != null)
                    shiftPrompt.transform.localScale = baseScale * (1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount);
                yield return null;
            }
            if (shiftPrompt != null)
            {
                shiftPrompt.transform.localScale = baseScale;
                shiftPrompt.SetActive(false);
            }

            // empujón
            theo.AutoDash(dashTime, dashSpeed);
            yield return new WaitForSeconds(dashTime);
        }

        theo.autoWalkMaxX = float.PositiveInfinity;
        theo.autoWalkSpeed = exitWalkSpeed;
        // salida por la derecha (cámara quieta)
        if (virtualCamera != null) virtualCamera.Follow = null;
        Camera cam = Camera.main;
        float t = 0f;
        while (t < maxExitTime)
        {
            float edge = cam.transform.position.x + cam.orthographicSize * cam.aspect + 0.2f;
            if (theo.transform.position.x > edge) break;
            t += Time.deltaTime;
            yield return null;
        }

        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeTo(1f, fadeOutTime);

        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator RampSnow()
    {
        if (snow == null) yield break;
        var e = snow.emission;
        float t = 0f;
        while (t < snowRampTime)
        {
            t += Time.deltaTime;
            e.rateOverTime = Mathf.Lerp(snowRateStart, snowRateHeavy, t / snowRampTime);
            yield return null;
        }
        e.rateOverTime = snowRateHeavy;
    }


    // Distancia máxima de la secuencia y piso mínimo necesario (desde el borde izquierdo de SnowZone)
    public float TotalDistance => comments.Length * (maxWalkPerStep + dashSpeed * dashTime + stopMargin);
    public float RequiredGround => TotalDistance + exitGroundMargin;

    void OnDrawGizmosSelected()
    {
        var col = GetComponent<Collider2D>();
        if (col == null) return;
        Vector3 a = new Vector3(col.bounds.min.x, col.bounds.center.y, 0f);
        Vector3 b = a + Vector3.right * TotalDistance;
        Vector3 c = a + Vector3.right * RequiredGround;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(a, b);
        Gizmos.DrawWireSphere(b, 0.2f);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(b, c);
        Gizmos.DrawWireSphere(c, 0.3f);
    }
}
