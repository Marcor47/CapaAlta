using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Roca pintada de azul (Onda). Ciclo: Intacta -> (golpe) Agrietada -> Escombros flotando
// -> (otro golpe) vuelve a crecer desde abajo -> Intacta. Los escombros no bloquean a Theo.
//
// Montaje: objeto raíz con BoxCollider2D (tag "Breakable", layer Ground) + este script.
//          Hijo "Visual" con SpriteRenderer (pivot Bottom Center) -> arrastrarlo a 'visual'.
//          Sin Rigidbody2D (o Static).
[RequireComponent(typeof(BoxCollider2D))]
public class BreakableObject : MonoBehaviour
{
    public enum State { Intact, Cracking, Rubble, Regrowing }

    [Header("Visual (hijo con SpriteRenderer)")]
    public SpriteRenderer visual;
    public Sprite intactSprite;
    public Sprite crackedSprite;
    public Sprite rubbleSprite;

    [Header("Tiempos")]
    public float crackDuration = 0.25f;   // se ve agrietada antes de romperse
    public float regrowDuration = 0.6f;   // crece desde abajo
    public float hitCooldown = 0.35f;     // golpes ignorados justo después de un cambio
    public bool startBroken = false;      // empieza como escombros

    [Header("Escombros flotando")]
    public float bobAmplitude = 0.06f;    // unidades
    public float bobSpeed = 2f;
    [Tooltip("Layer que usa mientras son escombros (NO debe estar en groundLayer de Theo)")]
    public string rubbleLayerName = "Default";

    [Header("Efectos (opcionales)")]
    public GameObject breakFx;            // p. ej. puf
    public GameObject regrowFx;
    public UnityEvent onBroken;
    public UnityEvent onRestored;

    public State Current { get; private set; } = State.Intact;

    private BoxCollider2D box;
    private Vector2 fullSize, fullOffset;
    private int originalLayer;
    private Vector3 visualBasePos;
    private SpriteMask growMask;
    private float nextHitTime;
    private float bobPhase;
    private bool busy;

    void Awake()
    {
        box = GetComponent<BoxCollider2D>();
        fullSize = box.size;
        fullOffset = box.offset;
        originalLayer = gameObject.layer;

        if (visual == null) visual = GetComponent<SpriteRenderer>();
        visualBasePos = visual != null ? visual.transform.localPosition : Vector3.zero;
        bobPhase = Random.value * 6.28f;

        // Máscara para el crecimiento desde abajo (recorta el sprite, no lo deforma)
        if (visual != null)
        {
            var go = new GameObject("GrowMask");
            go.transform.SetParent(visual.transform, false);
            growMask = go.AddComponent<SpriteMask>();
            growMask.enabled = false;
        }

        if (visual != null && intactSprite != null) visual.sprite = intactSprite;
    }

    void Start()
    {
        if (startBroken) ApplyRubble(silent: true);
    }

    // Lo llama la pelota (ThrownProjectile) al impactar
    public void Hit()
    {
        if (busy || Time.time < nextHitTime) return;

        if (Current == State.Intact) StartCoroutine(BreakRoutine());
        else if (Current == State.Rubble) StartCoroutine(RegrowRoutine());
    }

    IEnumerator BreakRoutine()
    {
        busy = true;
        Current = State.Cracking;
        if (visual != null && crackedSprite != null) visual.sprite = crackedSprite;

        float t = 0f;
        while (t < crackDuration)
        {
            t += Time.deltaTime;
            // temblor de 1 píxel (1/32 u) mientras se agrieta
            if (visual != null)
                visual.transform.localPosition = visualBasePos + new Vector3(((int)(t * 30f) % 2 == 0 ? 1 : -1) / 32f, 0f, 0f);
            yield return null;
        }

        ApplyRubble(silent: false);
        busy = false;
    }

    void ApplyRubble(bool silent)
    {
        Current = State.Rubble;

        box.isTrigger = true;              // Theo los atraviesa; la pelota los detecta por trigger
        box.size = fullSize;
        box.offset = fullOffset;

        int rubbleLayer = LayerMask.NameToLayer(rubbleLayerName);
        if (rubbleLayer >= 0) gameObject.layer = rubbleLayer;

        if (visual != null)
        {
            if (rubbleSprite != null) visual.sprite = rubbleSprite;
            visual.maskInteraction = SpriteMaskInteraction.None;
        }

        nextHitTime = Time.time + hitCooldown;

        if (!silent)
        {
            if (breakFx != null) Instantiate(breakFx, transform.position + (Vector3)(fullOffset), Quaternion.identity);
            onBroken?.Invoke();
        }
    }
    bool PlayerInside()
    {
        Vector2 center = transform.TransformPoint(fullOffset);
        Vector2 size = Vector2.Scale(fullSize, transform.lossyScale);
        foreach (var col in Physics2D.OverlapBoxAll(center, size, transform.eulerAngles.z))
            if (col.GetComponentInParent<TheoController>() != null) return true;
        return false;
    }
    IEnumerator RegrowRoutine()
    {
        busy = true;
        while (PlayerInside()) yield return null;   // espera a que Theo salga del espacio de la roca
        Current = State.Regrowing;

        gameObject.layer = originalLayer;
        box.isTrigger = false;

        if (visual != null)
        {
            visual.transform.localPosition = visualBasePos;
            visual.sprite = intactSprite;
            visual.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }
        if (growMask != null)
        {
            growMask.sprite = intactSprite;
            growMask.transform.localScale = new Vector3(1f, 0f, 1f);
            growMask.enabled = true;
        }
        if (regrowFx != null) Instantiate(regrowFx, transform.position + (Vector3)(fullOffset), Quaternion.identity);

        float pixelsTall = intactSprite != null ? intactSprite.rect.height : 16f;
        float t = 0f;
        while (t < regrowDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / regrowDuration);
            k = Mathf.Floor(k * pixelsTall) / pixelsTall;          // crece de a pixel

            SetGrowth(k);
            yield return null;
        }

        SetGrowth(1f);
        if (growMask != null) growMask.enabled = false;
        if (visual != null) visual.maskInteraction = SpriteMaskInteraction.None;

        Current = State.Intact;
        nextHitTime = Time.time + hitCooldown;
        onRestored?.Invoke();
        busy = false;
    }

    // k = 0..1 de la altura; el collider crece desde la base
    void SetGrowth(float k)
    {
        if (growMask != null) growMask.transform.localScale = new Vector3(1f, k, 1f);

        float h = Mathf.Max(0.02f, fullSize.y * k);
        float bottom = fullOffset.y - fullSize.y * 0.5f;
        box.size = new Vector2(fullSize.x, h);
        box.offset = new Vector2(fullOffset.x, bottom + h * 0.5f);
    }

    void Update()
    {
        if (Current != State.Rubble || visual == null) return;

        float y = Mathf.Sin(Time.time * bobSpeed + bobPhase) * bobAmplitude;
        y = Mathf.Round(y * 32f) / 32f;                              // pixel perfect (PPU 32)
        visual.transform.localPosition = visualBasePos + new Vector3(0f, y, 0f);
    }
}
