using UnityEngine;

public class ToolController : MonoBehaviour
{
    [Header("Puntos y líneas compartidas")]
    public Transform toolOrigin;   // mano de Theo (si queda vacío, usa el centro de Theo)
    public LineRenderer aimLine;   // línea de apuntado
    public LineRenderer ropeLine;  // cuerda del gancho (entrega 2)

    private ToolBehaviour[] tools;
    private bool subscribed;

    void Awake()
    {
        tools = GetComponents<ToolBehaviour>();
        foreach (var t in tools) t.enabled = false;
        if (aimLine != null) aimLine.enabled = false;
        if (ropeLine != null) ropeLine.enabled = false;
    }

    void Start()
    {
        if (InventorySystem.Instance == null) return;
        InventorySystem.Instance.OnEquippedChanged += Apply;
        subscribed = true;
        Apply(InventorySystem.Instance.Equipped);
    }

    void OnDestroy()
    {
        if (subscribed && InventorySystem.Instance != null)
            InventorySystem.Instance.OnEquippedChanged -= Apply;
    }

    void Apply(InventorySystem.ItemType equipped)
    {
        foreach (var tool in tools)
            tool.enabled = tool.Type == equipped;
    }
}