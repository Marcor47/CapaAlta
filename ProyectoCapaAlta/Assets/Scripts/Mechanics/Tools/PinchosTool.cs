using UnityEngine;
using UnityEngine.InputSystem;

public class PinchosTool : ToolBehaviour
{
    public override InventorySystem.ItemType Type => InventorySystem.ItemType.Pinchos;

    [Header("Detección de pared (ver el cubo celeste en Scene)")]
    public string climbableTag = "Climbable";
    public Vector2 wallCheckOffset = new Vector2(0.4f, 0.4f); // X = hacia donde mira, Y = altura
    public Vector2 wallCheckSize = new Vector2(0.4f, 0.6f);

    [Header("Escalada")]
    public float climbSpeed = 3.5f;
    public float climbStaminaPerSecond = 20f;
    public float hangStaminaPerSecond = 4f;
    public Vector2 topOutImpulse = new Vector2(3f, 5f);   // empujón al llegar al borde superior

    private Rigidbody2D rb;
    private bool climbing;
    private float climbDir = 1f;
    private float verticalInput;
    private float savedGravity;

    protected override void Awake()
    {
        base.Awake();
        if (theo != null) rb = theo.GetComponent<Rigidbody2D>();
    }

    public override void ResetOnRespawn() => StopClimbing();

    Vector2 WallCheckCenter(float dir)
    {
        Vector2 pos = theo.transform.position;
        return pos + new Vector2(wallCheckOffset.x * dir, wallCheckOffset.y);
    }

    bool WallAhead(float dir)
    {
        foreach (var col in Physics2D.OverlapBoxAll(WallCheckCenter(dir), wallCheckSize, 0f))
            if (col.CompareTag(climbableTag)) return true;
        return false;
    }

    void Update()
    {
        var mouse = Mouse.current;
        var kb = Keyboard.current;
        if (mouse == null || kb == null || theo == null || rb == null) return;

        if (!climbing)
        {
            float facing = theo.FacingLeft ? -1f : 1f;
            if (!InputBlocked && !ToolUnlockCall.AnyRinging && mouse.leftButton.wasPressedThisFrame
                && theo.CurrentStamina > 0f && WallAhead(facing))
                StartClimbing(facing);
            return;
        }

        // Soltarse: click derecho, sin estamina, o diálogo/pausa
        if (mouse.rightButton.wasPressedThisFrame || theo.CurrentStamina <= 0f || theo.IsInDialogue)
        {
            StopClimbing();
            return;
        }

        // Salto desde la pared: suelta y recibe el impulso normal de salto
        if (kb.spaceKey.wasPressedThisFrame)
        {
            StopClimbing();
            theo.JumpFromTool();
            return;
        }

        // Se acabó la pared: si iba subiendo, un empujón para subirse al borde
        if (!WallAhead(climbDir))
        {
            bool goingUp = verticalInput > 0f;
            StopClimbing();
            if (goingUp) rb.linearVelocity = new Vector2(climbDir * topOutImpulse.x, topOutImpulse.y);
            return;
        }

        // Moverse solo mientras se mantiene el click izquierdo
        float v = 0f;
        if (mouse.leftButton.isPressed && !theo.IsBackpackOpen)
        {
            if (kb.wKey.isPressed) v += 1f;
            if (kb.sKey.isPressed) v -= 1f;
        }
        verticalInput = v;

        float rate = Mathf.Abs(v) > 0f ? climbStaminaPerSecond : hangStaminaPerSecond;
        theo.ConsumeStamina(rate * Time.deltaTime);
    }

    void FixedUpdate()
    {
        if (climbing && rb != null)
            rb.linearVelocity = new Vector2(0f, verticalInput * climbSpeed);
    }

    void StartClimbing(float dir)
    {
        

        climbing = true;
        climbDir = dir;       // se fija al engancharse: girar el sprite no suelta la pared
        verticalInput = 0f;
        savedGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        theo.SetClimbing(true);
    }

    void StopClimbing()
    {
        if (!climbing) return;
        climbing = false;
        verticalInput = 0f;
        if (rb != null) rb.gravityScale = savedGravity;
        if (theo != null) theo.SetClimbing(false);
    }

    void OnDisable() => StopClimbing();

    void OnDrawGizmosSelected()
    {
        var t = GetComponentInParent<TheoController>();
        if (t == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube((Vector2)t.transform.position + new Vector2(wallCheckOffset.x, wallCheckOffset.y), wallCheckSize);
    }
}