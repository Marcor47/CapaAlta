using UnityEngine;
using UnityEngine.InputSystem;

public class TheoController : MonoBehaviour
{
    // ─── MOVIMIENTO ────────────────────────────────────────────
    [Header("Movimiento")]
    public float moveSpeed = 7f;
    public float groundAcceleration = 80f;
    public float airAcceleration = 40f;
    public float groundDeceleration = 100f;
    public float airDeceleration = 30f;

    // ─── SALTO ─────────────────────────────────────────────────
    [Header("Salto")]
    public float jumpForce = 17f;
    public float fallGravityMultiplier = 1.5f;
    public float lowJumpMultiplier = 1.6f;
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.10f;

    // ─── DASH ──────────────────────────────────────────────────
    [Header("Dash")]
    public bool dashEnabled = true;
    public float dashSpeed = 18f;
    public float dashDuration = 0.18f;
    public float dashStaminaCost = 34f; // ej: ~3 dashes con la barra llena
    public float dashCooldown = 0.6f;  // espacio mínimo entre un dash y el siguiente, estilo Hollow Knight

    // ─── ESTAMINA / MOTIVACIÓN ───────────────────────────────────
    [Header("Estamina / Motivación")]
    public float maxStamina = 100f;
    public float staminaRegenRate = 8f;         // regen pasiva por segundo
    public float staminaRegenRateSitting = 25f; // regen acelerada al sentarse (notebook)

    // ─── MOCHILA (E) ───────────────────────────────────────────
    [Header("Mochila")]
    public Key backpackKey = Key.E;

    // ─── SENTARSE / LIBRETA (S hold o ESC) ────────────────────
    [Header("Sentarse / Libreta")]
    public float sitHoldTime = 1.0f;

    // ─── DETECCIÓN DE SUELO ────────────────────────────────────
    [Header("Detección de suelo")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.08f;
    public LayerMask groundLayer;

    // ─── REFERENCIAS ───────────────────────────────────────────
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;

    // ─── ESTADO INTERNO ────────────────────────────────────────
    private float moveInput;
    private bool isGrounded;
    private bool wasGrounded;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool jumpHeld;

    private bool isDashing;
    private float dashTimeCounter;
    private float dashCooldownCounter;
    private float dashDirection;
    private float currentStamina;


    private float originalGravityScale;
    private bool facingLeft;

    private bool isBackpackOpen;
    private bool isSitting;
    private float sitHoldCounter;

    private bool canMove
    {
        get
        {
            if (isSitting)
                return false;

            if (isBackpackOpen && isGrounded)
                return false;

            return true;
        }
    }

    // ──────────────────────────────────────────────────────────
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        originalGravityScale = rb.gravityScale;
        facingLeft = sr.flipX;
        currentStamina = maxStamina;
    }

    void Update()
    {
        HandleStaminaRegen(); // corre siempre, incluso durante el dash (afecta muy poco por lo corto que es)

        if (dashCooldownCounter > 0f)
            dashCooldownCounter -= Time.deltaTime;

        if (isDashing) return;

        ReadInput();
        HandleJumpBuffer();
        HandleDashInput();   // ahora primero
        HandleFlip();        // ahora acá: si isDashing ya es true, se salta el giro este mismo frame
        HandleBackpackInput();
        HandleSitInput();

        HandleAnimations();
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            HandleDash();
            return;
        }

        CheckGround();
        HandleCoyoteTime();

        if (canMove)
        {
            ApplyMovement();
        }
        else
        {
            // Sentado (notebook) o mochila abierta en el suelo: corte instantáneo, sin deslizamiento
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        ApplyBetterGravity();
    }

    // ─── INPUT ─────────────────────────────────────────────────
    void ReadInput()
    {
        if (!canMove) { moveInput = 0f; return; }

        moveInput = 0f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            moveInput = -1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            moveInput = 1f;

        if (Keyboard.current.spaceKey.wasPressedThisFrame ||
            Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            jumpBufferCounter = jumpBufferTime;
            jumpHeld = true;
        }

        if (Keyboard.current.spaceKey.wasReleasedThisFrame ||
            Keyboard.current.upArrowKey.wasReleasedThisFrame)
            jumpHeld = false;

        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
            Jump();
    }

    // ─── SUELO ─────────────────────────────────────────────────
    void CheckGround()
    {
        wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position, groundCheckRadius, groundLayer);
    }

    void HandleCoyoteTime()
    {
        if (isGrounded) coyoteTimeCounter = coyoteTime;
        else coyoteTimeCounter -= Time.fixedDeltaTime;
    }

    void HandleJumpBuffer()
    {
        if (jumpBufferCounter > 0f)
            jumpBufferCounter -= Time.deltaTime;
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
    }

    // ─── MOVIMIENTO LATERAL ────────────────────────────────────
    void ApplyMovement()
    {
        float targetSpeed = moveInput * moveSpeed;
        float acceleration = moveInput != 0
            ? (isGrounded ? groundAcceleration : airAcceleration)
            : (isGrounded ? groundDeceleration : airDeceleration);

        rb.linearVelocity = new Vector2(
            Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed,
                acceleration * Time.fixedDeltaTime),
            rb.linearVelocity.y);
    }

    // ─── GRAVEDAD MEJORADA ─────────────────────────────────────
    void ApplyBetterGravity()
    {
        if (rb.linearVelocity.y < 0f)
            rb.linearVelocity += Vector2.up *
                Physics2D.gravity.y * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime;
        else if (rb.linearVelocity.y > 0f && !jumpHeld)
            rb.linearVelocity += Vector2.up *
                Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
    }

    // ─── FLIP ──────────────────────────────────────────────────
    void HandleFlip()
    {
        if (!canMove || isDashing)
            return;

        var state = anim.GetCurrentAnimatorStateInfo(0);
        if (state.IsName("TheoDash"))
            return; // el bool ya cambió pero el Animator todavía no transicionó visualmente

        if (moveInput > 0f)
        {
            sr.flipX = false;
            facingLeft = false;
        }
        else if (moveInput < 0f)
        {
            sr.flipX = true;
            facingLeft = true;
        }
    }

    // ─── DASH ──────────────────────────────────────────────────
    void HandleDashInput()
    {
        if (!dashEnabled || !canMove) return;
        if (currentStamina < dashStaminaCost) return; // sin estamina suficiente, no puede dashear
        if (dashCooldownCounter > 0f) return;         // muy pronto desde el último dash

        if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
        {
            dashDirection = facingLeft ? -1f : 1f;
            isDashing = true;
            dashTimeCounter = dashDuration;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.gravityScale = 0f;
            currentStamina -= dashStaminaCost;
            dashCooldownCounter = dashCooldown;
        }
    }

    void HandleDash()
    {
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);
        dashTimeCounter -= Time.fixedDeltaTime;

        if (dashTimeCounter <= 0f)
        {
            isDashing = false;
            rb.gravityScale = originalGravityScale;
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x * 0.3f, rb.linearVelocity.y);
        }
    }

    // ─── ESTAMINA / MOTIVACIÓN ───────────────────────────────────
    void HandleStaminaRegen()
    {
        if (currentStamina >= maxStamina) return;

        float regenRate = isSitting ? staminaRegenRateSitting : staminaRegenRate;
        currentStamina = Mathf.Min(maxStamina, currentStamina + regenRate * Time.deltaTime);
    }

    // ─── MOCHILA (E) ───────────────────────────────────────────
    void HandleBackpackInput()
    {
        if (isSitting)
        {
            anim.SetBool("IsBackpack", false);
            return;
        }

        isBackpackOpen = Keyboard.current[backpackKey].isPressed;
        anim.SetBool("IsBackpack", isBackpackOpen);
    }

    // ─── SENTARSE / LIBRETA (S hold) ───────────────────────────
    void HandleSitInput()
    {
        if (!isGrounded || isBackpackOpen) return;

        bool holdingDown = Keyboard.current.sKey.isPressed ||
                           Keyboard.current.downArrowKey.isPressed;

        if (holdingDown && !isSitting)
        {
            sitHoldCounter += Time.deltaTime;
            if (sitHoldCounter >= sitHoldTime)
            {
                isSitting = true;
                sitHoldCounter = 0f;
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y); // corte inmediato
                anim.SetBool("IsSitting", true);
                // TODO: EmotionSystem.Instance.StartRecharge();
                // TODO: MotivationSystem.Instance.StartFastRecharge();
            }
        }
        else if (!holdingDown)
            sitHoldCounter = 0f;

        if (isSitting &&
        (
            Keyboard.current.aKey.wasPressedThisFrame ||
            Keyboard.current.dKey.wasPressedThisFrame ||
            Keyboard.current.leftArrowKey.wasPressedThisFrame ||
            Keyboard.current.rightArrowKey.wasPressedThisFrame
        ))
        {
            StandUp();
        }
    }

    // ─── MÉTODOS PÚBLICOS (para NotebookMenu y otros) ──────────
    public void TriggerNotebook()
    {
        if (!isGrounded || isBackpackOpen) return;
        isSitting = true;
        sitHoldCounter = 0f;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y); // corte inmediato
        anim.SetBool("IsSitting", true);
    }

    public void StandUp()
    {
        isSitting = false;
        sitHoldCounter = 0f;
        anim.SetBool("IsSitting", false);
        // TODO: EmotionSystem.Instance.StopRecharge();
        // TODO: MotivationSystem.Instance.StopFastRecharge();
    }

    // ─── ANIMACIONES ───────────────────────────────────────────
    void HandleAnimations()
    {
        float v = Mathf.Abs(rb.linearVelocity.y) < 0.05f ? 0f : rb.linearVelocity.y;
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        anim.SetFloat("VerticalSpeed", v);
        anim.SetBool("IsGrounded", isGrounded);
        anim.SetBool("IsDashing", isDashing);
        anim.SetBool("IsSitting", isSitting);
        // IsBackpack ya se setea en HandleBackpackInput(), no hace falta repetirlo acá
    }

    // ─── PROPIEDADES PÚBLICAS ───────────────────────────────────
    public bool IsGrounded => isGrounded;
    public bool IsSitting => isSitting;
    public bool IsBackpackOpen => isBackpackOpen;
    public bool IsDashing => isDashing;
    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;
    public float StaminaPercent01 => maxStamina > 0f ? currentStamina / maxStamina : 0f; // para el fill de una barra UI


    // ─── DEBUG ─────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}