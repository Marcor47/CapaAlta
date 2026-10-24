using UnityEngine;
using UnityEngine.InputSystem;

public abstract class ToolBehaviour : MonoBehaviour
{
    public abstract InventorySystem.ItemType Type { get; }

    protected TheoController theo;
    protected ToolController controller;
    private float unblockTime;

    protected virtual void Awake()
    {
        theo = GetComponentInParent<TheoController>();
        controller = GetComponent<ToolController>();
    }

    // No se inician acciones en diálogo/pausa, con la mochila abierta, sentado o en dash,
    // ni durante 0.15 s después (para que el click que cierra un diálogo no active nada)
    protected bool InputBlocked
    {
        get
        {
            bool blockedNow = theo == null || theo.IsInDialogue || theo.IsBackpackOpen
                              || theo.IsSitting || theo.IsDashing;
            if (blockedNow) unblockTime = Time.unscaledTime + 0.15f;
            return blockedNow || Time.unscaledTime < unblockTime;
        }
    }

    protected Vector2 OriginPosition =>
        controller != null && controller.toolOrigin != null
            ? (Vector2)controller.toolOrigin.position
            : (Vector2)theo.transform.position;

    protected static Vector2 MouseWorldPosition()
    {
        var cam = Camera.main;
        if (cam == null || Mouse.current == null) return Vector2.zero;
        Vector3 p = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        return new Vector2(p.x, p.y);
    }
}