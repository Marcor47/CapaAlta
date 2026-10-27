using UnityEngine;
using UnityEngine.Serialization;

public class OndaTool : AimedTool
{
    public override InventorySystem.ItemType Type => InventorySystem.ItemType.Onda;

    [Header("Pelota")]
    [FormerlySerializedAs("rockPrefab")] public ThrownProjectile ballPrefab;
    [FormerlySerializedAs("rockSpeed")] public float ballSpeed = 14f;
    public float maxRange = 7f;            // alcance máximo (también limita la línea de apuntado)

    [Header("Carga y enfriamiento")]
    public float holdTime = 2f;            // mantener el click; al soltar sale al instante
    public float toolCooldown = 0.5f;      // desde que se lanza hasta poder cargar otra

    void SyncValues()
    {
        chargeTime = holdTime;
        cooldown = toolCooldown;
        aimLineMaxLength = maxRange;
    }

    protected override void Update()
    {
        SyncValues();
        base.Update();
    }

    protected override bool CanStartAiming() => ballPrefab != null;

    protected override void OnAimConfirmed(Vector2 from, Vector2 target)
    {
        Vector2 dir = target - from;
        if (dir.sqrMagnitude < 0.01f) dir = theo.FacingLeft ? Vector2.left : Vector2.right;

        var ball = Instantiate(ballPrefab, from, Quaternion.identity);
        ball.lifetime = maxRange / ballSpeed;      // la pelota desaparece al agotar el alcance
        ball.Launch(dir, ballSpeed, theo.gameObject);
    }
}