using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GanchoTool : AimedTool
{
    public override InventorySystem.ItemType Type => InventorySystem.ItemType.Gancho;

    [Header("Gancho")]
    public float maxRange = 9f;
    public float castDuration = 0.08f;
    public string grappableTag = "Grappable";
    public LayerMask hookMask = ~0;

    [Header("Carga y enfriamiento")]
    public float holdTime = 1f;
    public float toolCooldown = 0.3f;

    [Header("Límites de balanceo")]
    public float maxAngularSpeed = 150f;   // °/s alrededor del punto de anclaje
    public float maxSwingAngle = 100f;     // ° respecto a la vertical (0 = colgando recto hacia abajo)
    public float swingDamping = 0.4f;      // 1/s, frena el balanceo con el tiempo
    public float tautTolerance = 0.35f;    // m; por debajo de ropeLength - esto la cuerda se considera floja

    private bool hooked;
    private bool casting;
    private Vector2 anchorPoint;
    private float ropeLength;
    private DistanceJoint2D joint;
    private Rigidbody2D rb;

    protected override bool CanStartAiming() => !hooked && !casting;

    protected override void Update()
    {
        SyncValues();
        if (casting) return;

        if (hooked)
        {
            bool torn = Vector2.Distance(OriginPosition, anchorPoint) > ropeLength + 2f; // ej. respawn
            if (torn || (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame))
                Release();
            return;
        }

        base.Update();
    }

    protected override void Awake()
    {
        base.Awake();
        SyncValues();
    }

    void SyncValues()
    {
        chargeTime = holdTime;
        cooldown = toolCooldown;
        aimLineMaxLength = maxRange;
    }

    public override void ResetOnRespawn()
    {
        base.ResetOnRespawn();
        StopAllCoroutines();
        casting = false;
        Release();
    }
    protected override void OnAimConfirmed(Vector2 from, Vector2 target)
    {
        Vector2 dir = target - from;
        if (dir.sqrMagnitude < 0.01f) dir = theo.FacingLeft ? Vector2.left : Vector2.right;
        dir.Normalize();

        Vector2 end = from + dir * maxRange;
        bool attach = false;

        foreach (var hit in Physics2D.RaycastAll(from, dir, maxRange, hookMask))
        {
            if (hit.collider.isTrigger || hit.collider.transform.IsChildOf(theo.transform)) continue;

            end = hit.point;
            attach = hit.collider.CompareTag(grappableTag);
            break;
        }

        StartCoroutine(CastRoutine(from, end, attach));
    }

    IEnumerator CastRoutine(Vector2 from, Vector2 end, bool attach)
    {
        casting = true;
        var line = controller != null ? controller.ropeLine : null;

        float t = 0f;
        while (t < castDuration)
        {
            t += Time.deltaTime;
            if (line != null)
            {
                line.enabled = true;
                line.SetPosition(0, OriginPosition);
                line.SetPosition(1, Vector2.Lerp(from, end, Mathf.Clamp01(t / castDuration)));
            }
            yield return null;
        }

        casting = false;
        if (attach) Attach(end);
        else HideRope();
    }

    void Attach(Vector2 point)
    {
        rb = theo.GetComponent<Rigidbody2D>();
        anchorPoint = point;
        ropeLength = Mathf.Max(0.5f, Vector2.Distance(OriginPosition, point));

        joint = theo.gameObject.AddComponent<DistanceJoint2D>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = point;                       // sin connectedBody = coordenadas de mundo
        joint.anchor = theo.transform.InverseTransformPoint(OriginPosition);
        joint.autoConfigureDistance = false;
        joint.distance = ropeLength;
        joint.maxDistanceOnly = true;                        // cuerda: no se estira, pero tiene holgura
        joint.enableCollision = true;

        hooked = true;
        theo.SetHooked(true);
    }

    void Release()
    {
        if (joint != null) { joint.enabled = false; Destroy(joint); joint = null; }
        hooked = false;
        if (theo != null) theo.SetHooked(false);
        HideRope();
    }

    void HideRope()
    {
        if (controller != null && controller.ropeLine != null) controller.ropeLine.enabled = false;
    }

    void FixedUpdate()
    {
        if (!hooked || casting || rb == null || joint == null) return;

        // Punto real donde la cuerda sujeta a Theo (el mismo que usa el joint)
        Vector2 hand = theo.transform.TransformPoint(joint.anchor);
        Vector2 r = hand - anchorPoint;
        float len = r.magnitude;
        if (len < 0.1f || len < ropeLength - tautTolerance) return;   // cuerda floja: no limitar

        Vector2 u = r / len;                          // dirección radial (del ancla a Theo)
        Vector2 t = new Vector2(-u.y, u.x);           // tangente antihoraria
        Vector2 v = rb.linearVelocity;
        float vr = Vector2.Dot(v, u);
        float vt = Vector2.Dot(v, t);

        // 1) Tope de velocidad angular
        float maxVt = maxAngularSpeed * Mathf.Deg2Rad * len;
        vt = Mathf.Clamp(vt, -maxVt, maxVt);

        // 2) Amortiguación suave del balanceo
        vt *= Mathf.Max(0f, 1f - swingDamping * Time.fixedDeltaTime);

        // 3) Ángulo máximo: no pasar por encima del pivote ni dar vueltas
        float angle = Vector2.SignedAngle(Vector2.down, r);   // + = lado derecho, - = izquierdo
        if (angle > maxSwingAngle && vt > 0f) vt = 0f;
        else if (angle < -maxSwingAngle && vt < 0f) vt = 0f;

        rb.linearVelocity = vr * u + vt * t;
    }

    void LateUpdate()
    {
        if (!hooked) return;
        var line = controller != null ? controller.ropeLine : null;
        if (line == null) return;

        line.enabled = true;
        line.SetPosition(0, OriginPosition);
        line.SetPosition(1, anchorPoint);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        StopAllCoroutines();
        casting = false;
        Release();
    }
}