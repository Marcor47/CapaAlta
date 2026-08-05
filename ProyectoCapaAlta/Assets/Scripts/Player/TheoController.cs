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
    public float dashCooldown = 0.6f;

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
    private float originalGravityScale;

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
    }

    void Update()
    {
        HandleAnimations();
        if (isDashing) return;

        ReadInput();
        HandleJumpBuffer();
        HandleFlip();
        HandleDashInput();
        HandleBackpackInput();
        HandleSitInput();
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
            ApplyMovement();
        else
            rb.linearVelocity = new Vector2(
                Mathf.MoveTowards(rb.linearVelocity.x, 0f,
                    groundDeceleration * Time.fixedDeltaTime),
                rb.linearVelocity.y);

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
        if (!canMove) return;
        if (moveInput > 0f) sr.flipX = false;
        else if (moveInput < 0f) sr.flipX = true;
    }

    // ─── DASH ──────────────────────────────────────────────────
    void HandleDashInput()
    {
        if (!dashEnabled || !canMove) return;
        if (dashCooldownCounter > 0f) { dashCooldownCounter -= Time.deltaTime; return; }

        if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
        {
            dashDirection = sr.flipX ? -1f : 1f;
            isDashing = true;
            dashTimeCounter = dashDuration;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.gravityScale = 0f;
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
            dashCooldownCounter = dashCooldown;
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x * 0.3f, rb.linearVelocity.y);
        }
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
        if (!isGrounded) return;
        isSitting = true;
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
        anim.SetBool("IsBackpack", isBackpackOpen);
    }

    // ─── PROPIEDADES PÚBLICAS ───────────────────────────────────
    public bool IsGrounded => isGrounded;
    public bool IsSitting => isSitting;
    public bool IsBackpackOpen => isBackpackOpen;
    public bool IsDashing => isDashing;

    // ─── DEBUG ─────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}