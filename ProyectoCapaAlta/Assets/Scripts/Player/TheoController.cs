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
    public float maxFallSpeed = 18f;   // velocidad máxima de caída (unidades/seg)
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

    [Header("Agotamiento emocional")]
    [Range(0f, 1f)] public float drainedStaminaCap = 0.5f;       // tope de Motivación mientras está agotado
    [Range(0f, 1f)] public float drainRecoverThreshold = 0.5f;   // fracción de Regulación que hay que superar
    private bool isEmotionallyDrained;
    private bool momConnected;
    public bool IsEmotionallyDrained => isEmotionallyDrained;
    public float EffectiveMaxStamina => isEmotionallyDrained ? maxStamina * drainedStaminaCap : maxStamina;





    public float CurrentRegulacion => currentRegulacion;
    public float MaxRegulacion => maxRegulacion;
    public float RegulacionPercent01 => maxRegulacion > 0f ? currentRegulacion / maxRegulacion : 0f;
    public bool IsCallingMom => isCallingMom;


    // ─── TELÉFONO SATELITAL (temporal, hasta Bloque 6) ─────────
    [Header("Teléfono satelital (temporal)")]
    [Tooltip("Esto lo controlará el inventario cuando exista el Bloque 6")]
    public bool phoneEquipped = false;
    
    private bool isCallingMom = false;

    // ─── MOCHILA (E) ───────────────────────────────────────────
    [Header("Mochila")]
    public Key backpackKey = Key.E;

    // ─── HERRAMIENTAS ───────────────────────────────────────────
    [Header("Herramientas (gancho / pinchos)")]
    public float swingForce = 30f;
    public float maxSwingSpeed = 16f;
    private bool isHooked;
    private bool isClimbing;

    // ─── SENTARSE / LIBRETA ────────────────────────────────────
    [Header("Sentarse / Libreta")]
    public float sitHoldTime = 1.0f;

    // ─── DETECCIÓN DE SUELO ────────────────────────────────────
    [Header("Detección de suelo")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.3f, 0.1f);
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
    private bool isNotebookLoopActive = false;

    public bool FacingLeft => facingLeft;

    public event System.Action OnNotebookLoopStarted;


    public bool IsHooked => isHooked;
    public bool IsClimbing => isClimbing;
    public void SetHooked(bool value) { isHooked = value; }
    public void SetClimbing(bool value) { isClimbing = value; }
    public void ConsumeStamina(float amount) { currentStamina = Mathf.Max(0f, currentStamina - amount); }
    public bool JumpFromTool() => PerformJump();


    private bool movementLocked;
    public bool IsMovementLocked => movementLocked;
    public void SetMovementLocked(bool value)
    {
        movementLocked = value;
        if (value) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }




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
            if (IsRespawning) return false;
            if (movementLocked) return false;
            if (isSitting) return false;
            if (isBackpackOpen && isGrounded) return false;
            if (isInDialogue) return false;
            if (isCallingMom) return false; // NUEVO
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

        if (AccountManager.Instance != null && AccountManager.Instance.CurrentUser != null)
        {
            maxStamina += AccountManager.Instance.CurrentUser.progress.staminaBonusPermanent;
            maxRegulacion += AccountManager.Instance.CurrentUser.progress.regulacionBonusPermanent;
        }
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

        // Ejecutar el salto en el ciclo de física.
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
            Jump();

        if (isClimbing) return;

        if (isHooked && !isGrounded)
        {
            ApplySwingControl();
            return;
        }

        if (canMove)
            ApplyMovement();
        else
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        ApplyBetterGravity();
    }

    void ApplySwingControl()
    {
        if (canMove) rb.AddForce(new Vector2(moveInput * swingForce, 0f));
        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSwingSpeed);
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

        //if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        //    Jump();
    }

    // ─── SUELO ─────────────────────────────────────────────────
    void CheckGround()
    {
        wasGrounded = isGrounded;

        bool touchingGround = Physics2D.OverlapBox(
            groundCheck.position,
            groundCheckSize,
            0f,
            groundLayer
        );

        // Solo consideramos que está en el suelo si no está subiendo.
        isGrounded = touchingGround && rb.linearVelocity.y <= 0.05f;
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
        if (isClimbing) return; // los pinchos manejan su propio salto
        PerformJump();
    }

    bool PerformJump()
    {
        if (currentStamina <= 0f) return false;

        float staminaPercent = currentStamina / maxStamina;
        float jumpMultiplier = staminaPercent < 0.25f ? Mathf.Clamp01(staminaPercent / 0.25f) : 1f;
        float effectiveJumpForce = jumpForce * jumpMultiplier;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, effectiveJumpForce);
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
        currentStamina = Mathf.Max(0f, currentStamina - jumpStaminaCost);
        return true;
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
        if (rb.linearVelocity.y < -maxFallSpeed)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
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
        if (isHooked || isClimbing) return;
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
        // Con Regulación en 0, la Motivación queda limitada hasta que la Regulación supere el umbral
        if (currentRegulacion <= 0f) isEmotionallyDrained = true;
        else if (currentRegulacion > maxRegulacion * drainRecoverThreshold) isEmotionallyDrained = false;

        float cap = EffectiveMaxStamina;
        if (currentStamina > cap) currentStamina = cap;

        bool activelyMeditating = isSitting && isNotebookLoopActive;
        if (isClimbing) return;
        if (currentStamina < cap)
        {
            float regenRate = activelyMeditating ? staminaRegenRateSitting : staminaRegenRate;
            currentStamina = Mathf.Min(cap, currentStamina + regenRate * Time.deltaTime);
        }

        if (currentRegulacion < maxRegulacion)
        {
            if (activelyMeditating)
                currentRegulacion = Mathf.Min(maxRegulacion, currentRegulacion + regulacionRegenRateSitting * Time.deltaTime);
            else if (isCallingMom && momConnected)
                currentRegulacion = Mathf.Min(maxRegulacion, currentRegulacion + regulacionRegenRateCallingMom * Time.deltaTime);
        }
    }

    // ─── MOCHILA (E) ───────────────────────────────────────────
    void HandleBackpackInput()
    {
        if (IsRespawning) { isBackpackOpen = false; anim.SetBool("IsBackpack", false); return; }
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
        if (isClimbing) { sitHoldCounter = 0f; return; }
        if (!isGrounded || isBackpackOpen || isInDialogue || IsRespawning) return;

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

    public void SetCallingMom(bool value)
    {
        isCallingMom = value;
        if (!value) momConnected = false;
        if (value) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    public void SetMomConnected(bool value) { momConnected = value; }

    // ─── MÉTODOS PÚBLICOS ──────────────────────────────────────
    public void TriggerNotebook()
    {
        if (!isGrounded || isBackpackOpen) return;
        isSitting = true;
        sitHoldCounter = 0f;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        anim.SetBool("IsSitting", true);
    }

    public void OnNotebookLoopReached()
    {
        isNotebookLoopActive = true;
        OnNotebookLoopStarted?.Invoke();
    }

    public void StandUp()
    {
        isSitting = false;
        isNotebookLoopActive = false; // NUEVO
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

    // ─── RESPAWN ───────────────────────────────────────────────
    public bool IsRespawning { get; private set; }
    private readonly System.Collections.Generic.List<SpriteRenderer> hiddenRenderers = new System.Collections.Generic.List<SpriteRenderer>();

    public void BeginRespawn(bool freezeAndHide)
    {
        IsRespawning = true;

        isDashing = false;
        rb.gravityScale = originalGravityScale;
        moveInput = 0f;
        jumpBufferCounter = 0f;
        if (isSitting) StandUp();
        isBackpackOpen = false;
        anim.SetBool("IsBackpack", false);

        foreach (var tool in GetComponentsInChildren<ToolBehaviour>(true))
            tool.ResetOnRespawn();                 // suelta gancho, cancela escalada/carga/apuntado

        if (freezeAndHide)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            foreach (var r in GetComponentsInChildren<SpriteRenderer>())
                if (r.enabled) { r.enabled = false; hiddenRenderers.Add(r); }   // incluye el ítem en la mano
        }
    }

    public void EndRespawn(Vector3 position)
    {
        transform.position = position;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = originalGravityScale;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        foreach (var r in hiddenRenderers) if (r != null) r.enabled = true;
        hiddenRenderers.Clear();

        
    }

    public void FinishRespawn() => IsRespawning = false;

    // Hundimiento (agua): Theo cae despacio y se desvanece; al terminar queda oculto y congelado hasta EndRespawn
    public void BeginSink(float duration, float sinkSpeed)
    {
        StartCoroutine(SinkRoutine(duration, sinkSpeed));
    }

    System.Collections.IEnumerator SinkRoutine(float duration, float sinkSpeed)
    {
        var rends = GetComponentsInChildren<SpriteRenderer>();
        var startA = new float[rends.Length];
        for (int i = 0; i < rends.Length; i++) startA[i] = rends[i].color.a;

        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(0f, -sinkSpeed);

        anim.Play("TheoFall");   // nombre del estado, tal como aparece en tu Animator
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = 1f - Mathf.Clamp01(t / duration);
            for (int i = 0; i < rends.Length; i++)
            {
                if (!rends[i].enabled) continue;
                var c = rends[i].color; c.a = startA[i] * k; rends[i].color = c;
            }
            rb.linearVelocity = new Vector2(0f, -sinkSpeed);   // sin deriva horizontal
            anim.SetBool("IsGrounded", false);
            anim.SetFloat("VerticalSpeed", -10f);   // fuerza la pose de caída aunque se hunda lento
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        for (int i = 0; i < rends.Length; i++)
        {
            if (!rends[i].enabled) continue;
            var c = rends[i].color; c.a = startA[i]; rends[i].color = c;   // devuelve su alpha original
            rends[i].enabled = false;
            hiddenRenderers.Add(rends[i]);                                  // EndRespawn los vuelve a activar
        }
    }

    public void RestoreStamina() => currentStamina = EffectiveMaxStamina;

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
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }
}
