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

    private bool hooked;
    private bool casting;
    private Vector2 anchorPoint;
    private float ropeLength;
    private DistanceJoint2D joint;

    protected override bool CanStartAiming() => !hooked && !casting;

    protected override void Update()
    {
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