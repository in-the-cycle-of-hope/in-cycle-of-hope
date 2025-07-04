using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class PlayerMovement: MonoBehaviour
{
    #region Initialization
    public Rigidbody2D rb;
    bool isFacingRight = true;

    private Collider2D[] groundHitBuffer = new Collider2D[1];
    private Collider2D[] platformHitBuffer = new Collider2D[1];
    private Collider2D[] wallHitBuffer = new Collider2D[1];
    private RaycastHit2D[] lowerRayHitBuffer = new RaycastHit2D[1];
    private RaycastHit2D[] upperRayHitBuffer = new RaycastHit2D[1];
    private WaitForSeconds _dashCooldownWait;
    private WaitForSeconds _wallJumpDurationWait;

    [Header("Movement")]
    public float moveSpeed = 5f;
    float horizontalMovement;
    Vector2 moveInput;
    private bool isOnPlatform;
    public LayerMask decorPlatformLayer;

    [Header("Jumping")]
    public float jumpPower = 10f;
    private bool isGrounded;
    public float wallJumpDirectionBufferTime = 0.15f;

    [Header("Wall setings")]
    [SerializeField] private Transform wallCheckPos;
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.5f, 1f);
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallClimbSpeed = 3f;

    private bool isGrabbingWall = false;
    private bool isWallDetected = false;
    private bool isWallGrabbingActive = false;
    private float defaultGravity;

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float climbStaminaDrain = 25f; // Витрати при підйомі
    [SerializeField] private float descendStaminaDrain = 50f; // НОВА ЗМІННА: Витрати при спуску
    [SerializeField] private float idleStaminaDrain = 5f;
    [SerializeField] private float staminaRegenRate = 30f;

    private float currentStamina;
    private bool isOutOfStamina => currentStamina <= 0f;

    [Header("Wall Jump Settings")]
    [SerializeField] private float wallJumpForce = 12f; // Загальна сила стрибка
    [SerializeField] private Vector2 wallJumpClimb = new Vector2(0.7f, 1.5f); // Вектор для стрибка "вгору-вбік" (від стіни)
    [SerializeField] private float wallJumpDuration = 0.2f; // Як довго гравець знаходиться у стані isWallJumping
    [SerializeField] private float wallJumpStaminaCost = 15f;
    [SerializeField] private float wallJumpGravityMultiplier = 1.5f;
    [SerializeField] private float wallJumpGracePeriod = 0.12f;
    [SerializeField] private float wallJumpInputBufferTime = 0.08f;
    [SerializeField] private float horizontalInputThreshold = 0.2f;

    private bool wasGroundedLastFrame = false;
    private float wallJumpGraceTimer = 0f;
    private float wallJumpInputBufferTimer = 0f;
    private float bufferedWallJumpInputX = 0f;
    private bool isWallJumping = false;
    private float wallJumpTimer;

    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.5f;
    public int maxDashes = 1;

    // Buffer dash
    private float dashBufferTime = 0.15f;
    private float dashBufferTimer = 0f;
    private bool isDashing;
    private bool dashOnCooldown;
    private int remainingDashes;
    private Vector2 dashDirection;

    [Header("GroundCheck")]
    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask groundLayer;

    // Step-up mechanic
    [Header("Player step-up")]
    [SerializeField] Transform stepRayUpper;
    [SerializeField] Transform stepRayLower;
    [SerializeField] float stepHeight = 0.08f;
    [SerializeField] float stepSmooth = 0.04f;
    [SerializeField] LayerMask Ground;
    // buffer jump and coyote time
    private float jumpBufferTime = 0.15f;
    private float jumpBufferTimer = 0f;
    private float coyoteTime = 0.15f;
    private float coyoteTimer = 0f;
    #endregion
    private void Start()
    {
        remainingDashes = maxDashes;
        defaultGravity = rb.gravityScale;
        currentStamina = maxStamina;

        // Ініціалізуй тут
        _dashCooldownWait = new WaitForSeconds(dashCooldown);
        _wallJumpDurationWait = new WaitForSeconds(wallJumpDuration);
    }
    void Update()
    {
        GroundCheck();
        jumpBufferTimer -= Time.deltaTime;
        dashBufferTimer -= Time.deltaTime;
        wallJumpGraceTimer -= Time.deltaTime;
        if (wallJumpInputBufferTimer > 0)
            wallJumpInputBufferTimer -= Time.deltaTime;
        if (isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;
        HandleWallGrabbing();
        if (jumpBufferTimer > 0 && (isGrounded || coyoteTimer > 0) && !isDashing && !isWallJumping)
        {
            DoJump();
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }
        if (dashBufferTimer > 0 && !isDashing && !dashOnCooldown && remainingDashes > 0 && !isWallGrabbingActive)
        {
            Vector2 dashInput = moveInput;
            if (dashInput == Vector2.zero)
                dashInput = isFacingRight ? Vector2.right : Vector2.left;
            StartCoroutine(DashCoroutine(dashInput.normalized));
            dashBufferTimer = 0f;
        }
        Flip();
        if ((isGrounded || isOnPlatform) && remainingDashes < maxDashes && !isDashing)
        {
            remainingDashes = maxDashes;
            dashOnCooldown = false;
        }
        if (isWallJumping)
        {
            wallJumpTimer -= Time.deltaTime;
            if (wallJumpTimer <= 0)
            {
                isWallJumping = false;
            }
        }
        //if (isGrabbingWall && isWallDetected && !isOutOfStamina)
        //{
        //    isWallGrabbingActive = true;
        //    isGrabbingWall = true;
        //}
        //else if (!isGrabbingWall || isOutOfStamina)
        //{
        //    isWallGrabbingActive = false;
        //    isGrabbingWall = false;
        //}
    }
    private void FixedUpdate()

    {
        if (isDashing) { return; }
        if (isWallJumping)
        {
            return;
        }
        if (isWallGrabbingActive)
        {
            rb.linearVelocity = new Vector2(0f, moveInput.y * wallClimbSpeed);
            if (!isWallJumping)
            {
                if (moveInput.y > 0.01f)
                    currentStamina -= climbStaminaDrain * Time.fixedDeltaTime;
                else if (moveInput.y < -0.01f)
                    currentStamina -= descendStaminaDrain * Time.fixedDeltaTime;
                else
                    currentStamina -= idleStaminaDrain * Time.fixedDeltaTime;
            }
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
            if (isOutOfStamina)
            {
                isGrabbingWall = false;
                isWallGrabbingActive = false;
                rb.gravityScale = defaultGravity;
                wallJumpGraceTimer = wallJumpGracePeriod;
            }
            return;
        }
        // Звичайний рух
        rb.linearVelocity = new Vector2(horizontalMovement * moveSpeed, rb.linearVelocity.y);
        Flip();
        StepUp();
    }
    #region Movement
    private float rawHorizontalInput;
    public void Move(InputAction.CallbackContext context)
    {
        Vector2 rawMoveInput = context.ReadValue<Vector2>();
        rawHorizontalInput = rawMoveInput.x;
        if (isWallGrabbingActive)
        {
            moveInput = new Vector2(0f, rawMoveInput.y);
            horizontalMovement = 0f;
        }
        else
        {
            moveInput = rawMoveInput;
            horizontalMovement = moveInput.x;
        }
    }
    void StepUp()
    {
        Vector2 dir = Vector2.right * transform.localScale.x;

        int numLowerHits = Physics2D.RaycastNonAlloc(stepRayLower.position, dir, lowerRayHitBuffer, 0.1f, Ground); // Змінено на Ground
        UnityEngine.Debug.DrawRay(stepRayLower.position, dir * 0.1f, Color.red);

        if (numLowerHits > 0)
        {
            RaycastHit2D hitLower = lowerRayHitBuffer[0];

            int numUpperHits = Physics2D.RaycastNonAlloc(stepRayUpper.position, dir, upperRayHitBuffer, 0.2f, Ground); // Змінено на Ground
            UnityEngine.Debug.DrawRay(stepRayUpper.position, dir * 0.2f, Color.green);

            if (numUpperHits == 0)
            {
                float obstacleHeight = hitLower.point.y - rb.position.y;
                if (obstacleHeight <= stepHeight + 0.01f && Mathf.Abs(moveInput.x) > 0.01f)
                {
                    rb.position += Vector2.up * stepSmooth;
                }
            }
        }
    }
    #endregion
    #region Jumping
    private void DoJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
    }
    public void Jump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            jumpBufferTimer = jumpBufferTime;
            if (!isDashing)
            {
                if ((isWallDetected && isGrabbingWall && !isGrounded) || (wallJumpGraceTimer > 0 && isWallDetected))
                {
                    bufferedWallJumpInputX = rawHorizontalInput;
                    wallJumpInputBufferTimer = wallJumpInputBufferTime;
                    // Якщо не хапаємося за стіну — не стрибаємо
                    if (!isGrabbingWall)
                        return;
                    // Заборона стрибка в напрямку стіни
                    bool intendsWrongDirection =
                        (isFacingRight && bufferedWallJumpInputX > horizontalInputThreshold) ||
                        (!isFacingRight && bufferedWallJumpInputX < -horizontalInputThreshold);
                    if (intendsWrongDirection)
                        return;
                    // Якщо не вистачає витривалості на стрибок вгору — скасовуємо
                    if (moveInput.y > 0.5f && currentStamina < wallJumpStaminaCost)
                        return;
                    // Стрибаємо!
                    wallJumpGraceTimer = 0f;
                    DoWallJump();
                    jumpBufferTimer = 0f;
                    coyoteTimer = 0f;
                    return;
                }
            }
        }
    }
    #endregion
    #region Climb
    public void Grab(InputAction.CallbackContext context)
    {
        if (context.started)
            isGrabbingWall = true;
        else if (context.canceled)
            isGrabbingWall = false;
    }
    private void HandleWallGrabbing()
    {
        int numWallHits = Physics2D.OverlapBoxNonAlloc(wallCheckPos.position, wallCheckSize, 0f, wallLayer, wallHitBuffer);
        isWallDetected = numWallHits > 0;
        // Вихід, якщо в момент стрибка від стіни або грейс-період
        if (isWallJumping || wallJumpGraceTimer > 0)
        {
            if (isWallGrabbingActive)
            {
                isWallGrabbingActive = false;
                rb.gravityScale = defaultGravity;
            }
            return;
        }
        // ✅ Перевірка хапання з урахуванням тимчасової заборони
        bool canActivateWallGrab =
            isGrabbingWall &&
            isWallDetected &&
            !isGrounded &&
            !isDashing &&
            !isOutOfStamina &&
            !isWallGrabbingTemporarilyDisabled;
        if (canActivateWallGrab)
        {
            if (!isWallGrabbingActive)
            {
                isWallGrabbingActive = true;
                rb.gravityScale = 0f;
                rb.linearVelocity = Vector2.zero;
            }
        }
        else
        {
            if (isWallGrabbingActive)
            {
                isWallGrabbingActive = false;
                rb.gravityScale = defaultGravity;
            }
        }
        // Відновлення витривалості на землі
        if (isGrounded && currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        }
    }
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 20), $"Stamina: {Mathf.FloorToInt(currentStamina)}");
    }
    private bool isWallGrabbingTemporarilyDisabled = false;
    private void DoWallJump()
    {
        isWallJumping = true;
        wallJumpTimer = wallJumpDuration;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = defaultGravity;
        int wallDir = transform.localScale.x > 0 ? -1 : 1;
        bool isPressingAwayFromWall =
            (isFacingRight && bufferedWallJumpInputX < -horizontalInputThreshold) ||
            (!isFacingRight && bufferedWallJumpInputX > horizontalInputThreshold);
        bool isUpPressed = moveInput.y > 0.5f;
        Vector2 jumpForceVector;
        if (isUpPressed)
        {
            jumpForceVector = Vector2.up * wallJumpForce;
            currentStamina -= wallJumpStaminaCost;
        }
        else if (isPressingAwayFromWall)
        {
            jumpForceVector = new Vector2(wallJumpClimb.x * wallDir, wallJumpClimb.y);
        }
        else
        {
            // 👇 НОВА УМОВА — якщо нічого не натиснуто, все одно стрибаємо ВГОРУ
            jumpForceVector = Vector2.up * wallJumpForce;
            currentStamina -= wallJumpStaminaCost;
        }
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        rb.AddForce(jumpForceVector, ForceMode2D.Impulse);
        // Якщо стрибок вбік — фліпаємось
        if (isPressingAwayFromWall)
        {
            if ((isFacingRight && wallDir == -1) || (!isFacingRight && wallDir == 1))
            {
                FlipImmediate();
            }
        }
        StopCoroutine(nameof(WallJumpRoutine));
        StartCoroutine(WallJumpRoutine());
        wallJumpGraceTimer = wallJumpGracePeriod;
        bufferedWallJumpInputX = 0f;
        wallJumpInputBufferTimer = 0f;
    }
    private IEnumerator WallJumpRoutine()
    {
        yield return _wallJumpDurationWait;
        isWallJumping = false;
        rb.gravityScale = defaultGravity;
    }
    #endregion
    #region Dashing
    public void Dash(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            dashBufferTimer = dashBufferTime;
        }
    }
    private IEnumerator DashCoroutine(Vector2 inputDirection)
    {
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = defaultGravity;
        dashDirection = inputDirection == Vector2.zero
            ? (isFacingRight ? Vector2.right : Vector2.left)
            : inputDirection.normalized;
        float dashTime = 0f;
        while (dashTime < dashDuration)
        {
            rb.MovePosition(rb.position + dashDirection * dashSpeed * Time.fixedDeltaTime);
            dashTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        rb.gravityScale = originalGravity;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        isDashing = false;
        remainingDashes--;
        if (remainingDashes <= 0)
        {
            dashOnCooldown = true;
            yield return _dashCooldownWait;
            dashOnCooldown = false;
        }
    }
    #endregion
    private void GroundCheck()
    {
        // Для isGrounded
        int numGroundHits = Physics2D.OverlapBoxNonAlloc(groundCheckPos.position, groundCheckSize, 0, groundLayer, groundHitBuffer);
        isGrounded = numGroundHits > 0;

        // Для isOnPlatform
        int numPlatformHits = Physics2D.OverlapBoxNonAlloc(groundCheckPos.position, groundCheckSize, 0, decorPlatformLayer, platformHitBuffer);
        isOnPlatform = numPlatformHits > 0;

        if (isGrounded && rb.gravityScale != defaultGravity)
        {
            rb.gravityScale = defaultGravity;
        }
        if (isGrounded && !wasGroundedLastFrame)
        {
            currentStamina = maxStamina;
        }
        wasGroundedLastFrame = isGrounded;
    }
    private void Flip()
    {
        if (moveInput.x > 0 && !isFacingRight)
            FlipImmediate();
        else if (moveInput.x < 0 && isFacingRight)
            FlipImmediate();
    }
    private void FlipImmediate()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(wallCheckPos.position, wallCheckSize);
    }
}