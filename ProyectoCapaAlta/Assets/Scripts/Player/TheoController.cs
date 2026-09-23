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
    public float dashStaminaCost = 34f;
    public float dashCooldown = 0.6f;

    // ─── ESTAMINA / MOTIVACIÓN — agregar ───────────────────────
    [Header("Estamina / Motivación")]
    public float maxStamina = 100f;
    public float staminaRegenRate = 8f;
    public float staminaRegenRateSitting = 25f;
    public float jumpStaminaCost = 5f; // NUEVO: costo por salto

    // ─── REGULACIÓN EMOCIONAL (M9) — NUEVO ─────────────────────
    [Header("Regulación Emocional")]
    public float maxRegulacion = 100f;
    public float regulacionRegenRateSitting = 20f;
    public float regulacionRegenRateCallingMom = 45f;
    private float currentRegulacion;


    public float CurrentRegulacion => currentRegulacion;
    public float MaxRegulacion => maxRegulacion;
    public float RegulacionPercent01 => maxRegulacion > 0f ? currentRegulacion / maxRegulacion : 0f;
    public bool IsCallingMom => isCallingMom;


    // ─── TELÉFONO SATELITAL (temporal, hasta Bloque 6) ─────────
    [Header("Teléfono satelital (temporal)")]
    [Tooltip("Esto lo controlará el inventario cuando exista el Bloque 6")]
    public bool phoneEquipped = false;
    public Key callMomKey = Key.T;
    private bool isCallingMom = false;

    // ─── MOCHILA (E) ───────────────────────────────────────────
    [Header("Mochila")]
    public Key backpackKey = Key.E;

    // ─── SENTARSE / LIBRETA ────────────────────────────────────
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
    private bool isInDialogue;

    #if UNITY_EDITOR
        [ContextMenu("DEBUG: Bajar Regulación 20")]
        void DebugDecreaseRegulacion() => DecreaseRegulacion(20f);

        [ContextMenu("DEBUG: Restaurar Estamina y Regulación")]
        void DebugRestoreAll()
        {
            currentStamina = maxStamina;
            currentRegulacion = maxRegulacion;
        }
    #endif


    private bool canMove
    {
        get
        {
            if (isSitting) return false;
            if (isBackpackOpen && isGrounded) return false;
            if (isInDialogue) return false;
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
        currentRegulacion = maxRegulacion;
    }

    void Update()
    {
        HandleStaminaRegen();

        if (dashCooldownCounter > 0f)
            dashCooldownCounter -= Time.deltaTime;

        if (isDashing) return;

        ReadInput();
        HandleJumpBuffer();
        HandleDashInput();
        HandleFlip();
        HandleBackpackInput();
        HandleSitInput();
        HandleCallMomInput();
        HandleAnimations();
    }

    void FixedUpdate()
    {
        if (isDashing) { HandleDash(); return; }

        CheckGround();
        HandleCoyoteTime();

        if (canMove)
            ApplyMovement();
        else
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

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
        if (currentStamina <= 0f) return; // solo bloquea completamente en 0

        float staminaPercent = currentStamina / maxStamina;
        float jumpMultiplier = staminaPercent < 0.25f
            ? Mathf.Clamp01(staminaPercent / 0.25f) // de 0% a 25% escala linealmente de 0 a 1
            : 1f; // por encima de 25%, salto normal

        float effectiveJumpForce = jumpForce * jumpMultiplier;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, effectiveJumpForce);
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
        currentStamina = Mathf.Max(0f, currentStamina - jumpStaminaCost);
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
        if (!canMove || isDashing) return;

        var state = anim.GetCurrentAnimatorStateInfo(0);
        if (state.IsName("TheoDash")) return;

        if (moveInput > 0f) { sr.flipX = false; facingLeft = false; }
        else if (moveInput < 0f) { sr.flipX = true; facingLeft = true; }
    }

    // ─── DASH ──────────────────────────────────────────────────
    void HandleDashInput()
    {
        if (!dashEnabled || !canMove) return;
        if (currentStamina < dashStaminaCost) return;
        if (dashCooldownCounter > 0f) return;

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

    // ─── ESTAMINA ──────────────────────────────────────────────
    void HandleStaminaRegen()
    {
        if (currentStamina < maxStamina)
        {
            float regenRate = isSitting ? staminaRegenRateSitting : staminaRegenRate;
            currentStamina = Mathf.Min(maxStamina, currentStamina + regenRate * Time.deltaTime);
        }

        // NUEVO: Regulación Emocional NO se recarga sola — solo al sentarse o llamar a mamá
        if (currentRegulacion < maxRegulacion)
        {
            if (isSitting)
                currentRegulacion = Mathf.Min(maxRegulacion, currentRegulacion + regulacionRegenRateSitting * Time.deltaTime);
            else if (isCallingMom)
                currentRegulacion = Mathf.Min(maxRegulacion, currentRegulacion + regulacionRegenRateCallingMom * Time.deltaTime);
        }
    }

    // ─── MOCHILA (E) ───────────────────────────────────────────
    void HandleBackpackInput()
    {
        if (isSitting || isInDialogue)
        {
            anim.SetBool("IsBackpack", false);
            return;
        }

        isBackpackOpen = Keyboard.current[backpackKey].isPressed;
        anim.SetBool("IsBackpack", isBackpackOpen);
    }

    // ─── SENTARSE / LIBRETA ────────────────────────────────────
    void HandleSitInput()
    {
        if (!isGrounded || isBackpackOpen || isInDialogue) return;

        bool holdingDown = Keyboard.current.sKey.isPressed ||
                           Keyboard.current.downArrowKey.isPressed;

        if (holdingDown && !isSitting)
        {
            sitHoldCounter += Time.deltaTime;
            if (sitHoldCounter >= sitHoldTime)
            {
                isSitting = true;
                sitHoldCounter = 0f;
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                anim.SetBool("IsSitting", true);
                // TODO: EmotionSystem.Instance.StartRecharge();
                // TODO: MotivationSystem.Instance.StartFastRecharge();
            }
        }
        else if (!holdingDown)
            sitHoldCounter = 0f;

        if (isSitting && (
            Keyboard.current.aKey.wasPressedThisFrame ||
            Keyboard.current.dKey.wasPressedThisFrame ||
            Keyboard.current.leftArrowKey.wasPressedThisFrame ||
            Keyboard.current.rightArrowKey.wasPressedThisFrame))
            StandUp();
    }

    // ─── LLAMAR MADRE  ────────────────────────────────────
    void HandleCallMomInput()
    {
        if (!phoneEquipped || isBackpackOpen || isInDialogue) { isCallingMom = false; return; }

        bool holdingCall = Keyboard.current[callMomKey].isPressed;

        if (holdingCall && isGrounded && !isSitting)
        {
            if (!isCallingMom)
            {
                isCallingMom = true;
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
        }
        else
        {
            isCallingMom = false;
        }
    }

    // ─── MÉTODOS PÚBLICOS ──────────────────────────────────────
    public void TriggerNotebook()
    {
        if (!isGrounded || isBackpackOpen) return;
        isSitting = true;
        sitHoldCounter = 0f;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
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

    public void SetDialogueState(bool inDialogue)
    {
        isInDialogue = inDialogue;
        if (inDialogue)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            if (isBackpackOpen)
            {
                isBackpackOpen = false;
                anim.SetBool("IsBackpack", false);
            }
        }
    }

    public void DecreaseRegulacion(float amount)
    {
        currentRegulacion = Mathf.Max(0f, currentRegulacion - amount);
    }

    public void IncreaseRegulacionPermanent(float amount) // llamar cuando se recolecta una carta del padre
    {
        maxRegulacion += amount;
    }

    public void IncreaseMotivacionPermanent(float amount) // llamar cuando se completa el set de un personaje secundario
    {
        maxStamina += amount;
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
    }

    // ─── PROPIEDADES PÚBLICAS ──────────────────────────────────
    public bool IsGrounded => isGrounded;
    public bool IsSitting => isSitting;
    public bool IsBackpackOpen => isBackpackOpen;
    public bool IsDashing => isDashing;
    public bool IsInDialogue => isInDialogue;
    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;
    public float StaminaPercent01 => maxStamina > 0f ? currentStamina / maxStamina : 0f;

    // ─── DEBUG ─────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
