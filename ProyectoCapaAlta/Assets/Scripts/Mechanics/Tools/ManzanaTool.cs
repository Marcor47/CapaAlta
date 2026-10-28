using UnityEngine;

public class ManzanaTool : AimedTool
{
    public override InventorySystem.ItemType Type => InventorySystem.ItemType.Manzana;

    [Header("Manzana lanzada")]
    public ThrownProjectile applePrefab;
    public float throwSpeed = 10f;

    [Header("Apuntado y enfriamiento")]
    public float maxRange = 6f;
    public float toolCooldown = 0.3f;

    protected override void Update()
    {
        chargeTime = 0f;                 // la manzana no se carga
        cooldown = toolCooldown;
        aimLineMaxLength = maxRange;
        base.Update();
    }

    protected override bool CanStartAiming() =>
        applePrefab != null && DecisionRecord.Instance != null && DecisionRecord.Instance.ManzanasAvailable > 0;

    protected override void OnAimConfirmed(Vector2 from, Vector2 target)
    {
        if (DecisionRecord.Instance == null || !DecisionRecord.Instance.RemoveManzana()) return;

        Vector2 dir = target - from;
        if (dir.sqrMagnitude < 0.01f) dir = theo.FacingLeft ? Vector2.left : Vector2.right;

        var apple = Instantiate(applePrefab, from, Quaternion.identity);
        apple.Launch(dir, throwSpeed, theo.gameObject);
    }
}