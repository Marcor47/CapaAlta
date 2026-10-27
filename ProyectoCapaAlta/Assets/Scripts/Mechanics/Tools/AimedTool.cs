using UnityEngine;
using UnityEngine.InputSystem;

public abstract class AimedTool : ToolBehaviour
{
    [HideInInspector] public float aimLineMaxLength = 6f;
    [HideInInspector] public float cooldown = 0.3f;
    [HideInInspector] public float chargeTime = 0f;

    protected bool aiming;
    private float nextAllowedTime;
    private float aimStartTime;

    private static Color lineBaseColor;
    private static bool lineBaseCached;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => lineBaseCached = false;

    public override void ResetOnRespawn() => CancelAim();

    protected float Charge01 => chargeTime <= 0f ? 1f : Mathf.Clamp01((Time.time - aimStartTime) / chargeTime);

    protected abstract bool CanStartAiming();
    protected abstract void OnAimConfirmed(Vector2 from, Vector2 target);

    protected virtual void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (aiming)
        {
            if (InputBlocked || mouse.rightButton.wasPressedThisFrame) { CancelAim(); return; }

            UpdateAimLine();

            if (mouse.leftButton.wasReleasedThisFrame)
            {
                bool charged = Charge01 >= 1f;
                Vector2 from = OriginPosition;
                Vector2 target = MouseWorldPosition();
                CancelAim();
                if (!charged) return;              // soltó antes de tiempo: no pasa nada
                nextAllowedTime = Time.time + cooldown;
                OnAimConfirmed(from, target);      // sale en este mismo frame
            }
            return;
        }

        if (InputBlocked || ToolUnlockCall.AnyRinging || Time.time < nextAllowedTime) return;

        if (mouse.leftButton.wasPressedThisFrame && CanStartAiming())
        {
            aiming = true;
            aimStartTime = Time.time;
            CursorManager.Instance?.SetAim();
        }
    }

    void UpdateAimLine()
    {
        var line = controller != null ? controller.aimLine : null;
        if (line == null) return;

        Vector2 from = OriginPosition;
        Vector2 dir = MouseWorldPosition() - from;
        Vector2 end = from + Vector2.ClampMagnitude(dir, aimLineMaxLength);

        if (!lineBaseCached) { lineBaseColor = line.startColor; lineBaseCached = true; }
        Color c = lineBaseColor;
        if (chargeTime > 0f)
        {
            Color dim = new Color(c.r, c.g, c.b, c.a * 0.3f);
            c = Color.Lerp(dim, c, Charge01);
        }
        line.startColor = c;
        line.endColor = c;

        line.enabled = true;
        line.SetPosition(0, from);
        line.SetPosition(1, end);
    }

    protected void CancelAim()
    {
        aiming = false;
        if (controller != null && controller.aimLine != null) controller.aimLine.enabled = false;
        CursorManager.Instance?.SetDefault();
    }

    protected virtual void OnDisable() => CancelAim();
}