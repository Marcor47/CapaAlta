using UnityEngine;

public class OndaTool : AimedTool
{
    public override InventorySystem.ItemType Type => InventorySystem.ItemType.Onda;

    [Header("Roca")]
    public ThrownProjectile rockPrefab;
    public float rockSpeed = 14f;
    public float maxRange = 8f;

    protected override bool CanStartAiming() => rockPrefab != null;

    protected override void OnAimConfirmed(Vector2 from, Vector2 target)
    {
        Vector2 dir = target - from;
        if (dir.sqrMagnitude < 0.01f) dir = theo.FacingLeft ? Vector2.left : Vector2.right;

        var rock = Instantiate(rockPrefab, from, Quaternion.identity);
        rock.lifetime = maxRange / rockSpeed;
        rock.Launch(dir, rockSpeed, theo.gameObject);
    }
}