using UnityEngine;
using UnityEngine.InputSystem;

public abstract class AimedTool : ToolBehaviour
{
    [Header("Apuntado")]
    public float aimLineMaxLength = 6f;
    public float cooldown = 0.3f;

    protected bool aiming;
    private float nextAllowedTime;

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
                Vector2 from = OriginPosition;
                Vector2 target = MouseWorldPosition();
                CancelAim();
                nextAllowedTime = Time.time + cooldown;
                OnAimConfirmed(from, target);
            }
            return;
        }

        if (InputBlocked || Time.time < nextAllowedTime) return;

        if (mouse.leftButton.wasPressedThisFrame && CanStartAiming())
            aiming = true;
    }

    void UpdateAimLine()
    {
        var line = controller != null ? controller.aimLine : null;
        if (line == null) return;

        Vector2 from = OriginPosition;
        Vector2 dir = MouseWorldPosition() - from;
        Vector2 end = from + Vector2.ClampMagnitude(dir, aimLineMaxLength);

        line.enabled = true;
        line.SetPosition(0, from);
        line.SetPosition(1, end);
    }

    protected void CancelAim()
    {
        aiming = false;
        if (controller != null && controller.aimLine != null) controller.aimLine.enabled = false;
    }

    protected virtual void OnDisable() => CancelAim();
}