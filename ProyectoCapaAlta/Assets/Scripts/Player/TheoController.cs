using UnityEngine;
using UnityEngine.InputSystem;

public class TheoController : MonoBehaviour
{
    // ─── MOVIMIENTO ────────────────────────────────────────────
    [Header("Movimiento")]
    public float moveSpeed = 7f;

    public float groundAcceleration = 80;
    public float airAcceleration = 40;

    public float groundDeceleration = 100;
    public float airDeceleration = 30;

    // ─── SALTO ─────────────────────────────────────────────────
    [Header("Salto")]
    public float jumpForce = 17f;
    [Tooltip("Multiplicador de gravedad al caer (caída más rápida)")]
    public float fallGravityMultiplier = 1.5f;
    [Tooltip("Multiplicador cuando suelta el salto antes de llegar al tope")]
    public float lowJumpMultiplier = 1.6f;
    [Tooltip("Segundos de gracia para saltar después de caer del borde")]
    public float coyoteTime = 0.12f;
    [Tooltip("Segundos de buffer si presiona salto antes de aterrizar")]
    public float jumpBufferTime = 0.10f;

    // ─── DASH ──────────────────────────────────────────────────
    [Header("Dash")]
    public bool dashEnabled = true;
    public float dashSpeed = 18f;
    public float dashDuration = 0.18f;
    public float dashCooldown = 0.6f;
    [Tooltip("Tecla de dash (Shift izquierdo por defecto)")]

    // ─── DETECCIÓN DE SUELO ────────────────────────────────────
    [Header("Detección de suelo")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.08f;
    public LayerMask groundLayer;

    // ─── REFERENCIAS ───────────────────────────────────────────
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;  // Descomenta cuando tengas el Animator configurado

    // ─── ESTADO INTERNO ────────────────────────────────────────
    private float moveInput;
    private bool isGrounded;
    private bool wasGrounded;

    // Coyote time
    private float coyoteTimeCounter;

    // Jump buffer
    private float jumpBufferCounter;

    // Variable jump height
    private bool jumpHeld;


    // Dash
    private bool isDashing;
    private float dashTimeCounter;
    private float dashCooldownCounter;
    private float dashDirection;


    //Otros
    private float originalGravityScale;



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
        // Si está dasheando, no procesar otras entradas
        if (isDashing) return;

        ReadInput();
        
        HandleJumpBuffer();
        HandleFlip();
        HandleDashInput();
        
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            HandleDash();
            HandleAnimations();
            return;
        }

        CheckGround();
        HandleCoyoteTime();

        ApplyMovement();
        ApplyBetterGravity();

        HandleAnimations();
    }

    // ─── INPUT ─────────────────────────────────────────────────
    void ReadInput()
    {
        // Movimiento lateral
        moveInput = 0f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            moveInput = -1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            moveInput = 1f;

        // Salto — detecta pulsación y si mantiene
        if (Keyboard.current.spaceKey.wasPressedThisFrame ||
            Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            jumpBufferCounter = jumpBufferTime;
            jumpHeld = true;
        }

        if (Keyboard.current.spaceKey.wasReleasedThisFrame ||
            Keyboard.current.upArrowKey.wasReleasedThisFrame)
            jumpHeld = false;

        // Ejecutar salto si hay buffer y coyote time disponibles
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            Jump();
        }
    }

    // ─── SUELO ─────────────────────────────────────────────────
    void CheckGround()
    {
        wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position, groundCheckRadius, groundLayer);

        
    }

    // ─── COYOTE TIME ───────────────────────────────────────────
    void HandleCoyoteTime()
    {
        if (isGrounded)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.fixedDeltaTime;
    }

    // ─── JUMP BUFFER ───────────────────────────────────────────
    void HandleJumpBuffer()
    {
        if (jumpBufferCounter > 0f)
            jumpBufferCounter -= Time.deltaTime;
    }

    // ─── SALTO ─────────────────────────────────────────────────
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

        float acceleration;

        if (moveInput != 0)
        {
            acceleration = isGrounded ? groundAcceleration : airAcceleration;
        }
        else
        {
            acceleration = isGrounded ? groundDeceleration : airDeceleration;
        }

        float newVelocityX = Mathf.MoveTowards(
            rb.linearVelocity.x,
            targetSpeed,
            acceleration * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(newVelocityX, rb.linearVelocity.y);
    }

    // ─── GRAVEDAD MEJORADA ─────────────────────────────────────
    // Caída más rápida que la subida + salto corto si sueltas pronto
    void ApplyBetterGravity()
    {
        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector2.up *
                Physics2D.gravity.y *
                (fallGravityMultiplier - 1f) *
                Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0f && !jumpHeld)
        {
            rb.linearVelocity += Vector2.up *
                Physics2D.gravity.y *
                (lowJumpMultiplier - 1f) *
                Time.fixedDeltaTime;
        }
    }

    // ─── FLIP DEL SPRITE ───────────────────────────────────────
    void HandleFlip()
    {
        if (moveInput > 0f)
            sr.flipX = false;
        else if (moveInput < 0f)
            sr.flipX = true;
    }

    // ─── DASH ──────────────────────────────────────────────────
    void HandleDashInput()
    {
        if (!dashEnabled) return;
        if (dashCooldownCounter > 0f)
        {
            dashCooldownCounter -= Time.deltaTime;
            return;
        }

        if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
        {
            // Dirección del dash según donde mira
            dashDirection = sr.flipX ? -1f : 1f;
            isDashing = true;
            dashTimeCounter = dashDuration;
            // Cancela velocidad vertical al dashear
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.gravityScale = 0f; // Sin gravedad durante el dash
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
                rb.linearVelocity.x * 0.3f,
                rb.linearVelocity.y);
        }
    }

    // ─── ANIMACIONES (placeholder) ─────────────────────────────
    void HandleAnimations()
    {
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        anim.SetBool("IsGrounded", isGrounded);
        float v = rb.linearVelocity.y;

        if (Mathf.Abs(v) < 0.05f)
            v = 0f;

        anim.SetFloat("VerticalSpeed", v);
        anim.SetBool("IsDashing", isDashing);
    }

    // ─── DEBUG ─────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
