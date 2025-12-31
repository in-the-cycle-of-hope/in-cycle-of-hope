using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using Fungus;

public class PlayerMovement : MonoBehaviour
{
    #region Initialization

    public Flowchart flowchart;
    public DialogTrigger currentInteractable;

    public Rigidbody2D rb;
    bool isFacingRight = true;
    public Vector3 respawnPoint;
    public Animator fadeAnimator;
    public Animator fadeAnimator2;
    public GameObject blackScreen;
    public GameObject blackScreen2;
    public float fadeDelay = 0.2f;
    public PlayerInput playerInput;
    public bool isControlBlocked;

    private RaycastHit2D[] lowerRayHitBuffer = new RaycastHit2D[1];
    private RaycastHit2D[] upperRayHitBuffer = new RaycastHit2D[1];
    private WaitForSeconds _dashCooldownWait;
    private WaitForSeconds _wallJumpDurationWait;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float horizontalMovement;
    public Vector2 moveInput;
    private bool isOnPlatform;

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
    [SerializeField] private float climbStaminaDrain = 25f;
    [SerializeField] private float descendStaminaDrain = 50f;
    [SerializeField] private float idleStaminaDrain = 5f;
    [SerializeField] private float staminaRegenRate = 30f;

    [SerializeField] private Vector3 staminaLabelOffset = new Vector3(0f, 1.5f, 0f);

    private float currentStamina;
    private bool isOutOfStamina => currentStamina <= 0f;

    [Header("Wall Jump Settings")]
    [SerializeField] private float wallJumpForce = 12f;
    [SerializeField] private Vector2 wallJumpClimb = new Vector2(0.7f, 1.5f);
    [SerializeField] private float wallJumpDuration = 0.2f;
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

    [Header("Ledge Climb")]
    [SerializeField] private float ledgeCheckUp = 1.0f;
    [SerializeField] private float ledgeCheckForward = 0.25f;
    [SerializeField] private float ledgeCheckDownDist = 1.5f;
    [SerializeField] private float ledgeClimbTime = 0.15f;
    [SerializeField] private float ledgeClimbYOffset = 0.35f;
    [SerializeField] private float ledgePullBack = 0.10f;

    private bool isClimbingLedge = false;


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

    [Header("Player step-up")]
    [SerializeField] Transform stepRayUpper;
    [SerializeField] Transform stepRayLower;
    [SerializeField] float stepHeight = 0.08f;
    [SerializeField] float stepSmooth = 0.04f;
    [SerializeField] LayerMask Ground;

    private float jumpBufferTime = 0.15f;
    private float jumpBufferTimer = 0f;
    private float coyoteTime = 0.15f;
    private float coyoteTimer = 0f;

    //[Header("Tutorial Abilities")]
    //public bool canJump = false;
    //public bool canDash = false;
    //public bool canGrabWall = false;
    #endregion
    private RigidbodyConstraints2D originalConstraints;
    public void BlockControl()
    {
        isControlBlocked = true;
        originalConstraints = rb.constraints;
        rb.gravityScale = defaultGravity;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }


    public void UnblockControl()
    {
        rb.constraints = originalConstraints;

        moveInput = Vector2.zero;
        horizontalMovement = 0f;
        rawHorizontalInput = 0f;

        isControlBlocked = false;
    }
    public void ResetMovementState()
    {
        moveInput = Vector2.zero;
        horizontalMovement = 0f;
        rawHorizontalInput = 0f;

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        rb.angularVelocity = 0f;
    }
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }
    public void RespawnNow()
    {
        transform.position = respawnPoint;
    }

    public IEnumerator FadeRespawnTo(Vector3 newPosition)
    {
        playerInput.enabled = false;

        // --- ЕТАП 1: ЗАТУХАННЯ ---
        if (blackScreen != null)
        {
            blackScreen.SetActive(true);
            fadeAnimator.Play("BlackScreenIn", -1, 0f);
        }

        yield return new WaitForSecondsRealtime(1.0f);

        // --- ЕТАП 2: ТЕЛЕПОРТАЦІЯ ТА ВІДНОВЛЕННЯ ФІЗИКИ ---
        // Спочатку міняємо позицію
        transform.position = newPosition;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Повертаємо рухливість ТІЛЬКИ після переміщення
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero; // або rb.velocity для старих версій
        }

        yield return new WaitForSecondsRealtime(0.2f);

        if (blackScreen != null) blackScreen.SetActive(false);

        // --- ЕТАП 3: ПРОЯСНЕННЯ ---
        if (blackScreen2 != null)
        {
            blackScreen2.SetActive(true);
            fadeAnimator2.Play("BlackScreenOut", -1, 0f);
        }

        yield return new WaitForSecondsRealtime(1.0f);

        if (blackScreen2 != null) blackScreen2.SetActive(false);

        playerInput.enabled = true;
    }
    private void Start()
    {
        if (SaveSystem.CanContinue())
        {
            transform.position = SaveSystem.LoadPosition();
        }
        remainingDashes = maxDashes;
        defaultGravity = rb.gravityScale;
        currentStamina = maxStamina;
        _dashCooldownWait = new WaitForSeconds(dashCooldown);
        _wallJumpDurationWait = new WaitForSeconds(wallJumpDuration);
    }
    void Update()
    {
        if (isControlBlocked)
        {
            moveInput = Vector2.zero;
            return;
        }
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
    }
    private void FixedUpdate()
    {
        if (isControlBlocked)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }
        if (isDashing) { return; }
        if (isWallJumping)
        {
            return;
        }
        if (isWallGrabbingActive && !isClimbingLedge)
        {
            TryLedgeClimb();
        }

        if (isWallGrabbingActive)
        {
            if (Mathf.Abs(moveInput.y) > 0.01f)
            {
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                rb.linearVelocity = new Vector2(0f, moveInput.y * wallClimbSpeed);
                if (moveInput.y > 0.01f)
                    currentStamina -= climbStaminaDrain * Time.fixedDeltaTime;
                else if (moveInput.y < -0.01f)
                    currentStamina -= descendStaminaDrain * Time.fixedDeltaTime;
            }
            else
            {
                rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;
                rb.linearVelocity = Vector2.zero;
                currentStamina -= idleStaminaDrain * Time.fixedDeltaTime;
            }

            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

            if (isOutOfStamina)
            {
                isGrabbingWall = false;
                isWallGrabbingActive = false;
                rb.gravityScale = defaultGravity;

                rb.constraints = RigidbodyConstraints2D.FreezeRotation;

                wallJumpGraceTimer = wallJumpGracePeriod;
            }

            return;
        }
        rb.linearVelocity = new Vector2(horizontalMovement * moveSpeed, rb.linearVelocity.y);
        StepUp();
    }

    //#region Tutorial API (Fungus)
    //public void EnableJump()
    //{
    //    canJump = true;
    //}
    //public void EnableDash()
    //{
    //    canDash = true;
    //}
    //public void EnableWallGrab()
    //{
    //    canGrabWall = true;
    //}
    //#endregion

    #region Movement
    private float rawHorizontalInput;
    public void Move(InputAction.CallbackContext context)
    {
        if (isControlBlocked) return;
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
        Flip();
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
        //if (!canJump) return;
        if (context.started)
        {
            jumpBufferTimer = jumpBufferTime;
            if (!isDashing)
            {
                if ((isWallDetected && isGrabbingWall && !isGrounded) || (wallJumpGraceTimer > 0 && isWallDetected))
                {
                    bufferedWallJumpInputX = rawHorizontalInput;
                    wallJumpInputBufferTimer = wallJumpInputBufferTime;

                    if (!isGrabbingWall)
                        return;

                    bool intendsWrongDirection =
                        (isFacingRight && bufferedWallJumpInputX > horizontalInputThreshold) ||
                        (!isFacingRight && bufferedWallJumpInputX < -horizontalInputThreshold);
                    if (intendsWrongDirection)
                        return;

                    if (moveInput.y > 0.5f && currentStamina < wallJumpStaminaCost)
                        return;

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
        //if (!canGrabWall) return;
        if (context.started)
        {
            isGrabbingWall = true;
        }
        else if (context.canceled)
        {
            isGrabbingWall = false;

            if (isWallGrabbingActive)
            {
                isWallGrabbingActive = false;
                rb.gravityScale = defaultGravity;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }
    }
    private void HandleWallGrabbing()
    {
        bool shouldCheckWall =
            !isGrounded && (isGrabbingWall || wallJumpGraceTimer > 0 || wallJumpInputBufferTimer > 0);

        if (shouldCheckWall)
        {
            Collider2D wallCollider = Physics2D.OverlapBox(wallCheckPos.position, wallCheckSize, 0f, wallLayer);
            isWallDetected = wallCollider != null;
        }
        else
        {
            isWallDetected = false;
        }

        if (isWallJumping || wallJumpGraceTimer > 0)
        {
            if (isWallGrabbingActive)
            {
                isWallGrabbingActive = false;
                rb.gravityScale = defaultGravity;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
            return;
        }

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

                rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;
            }
        }
        else
        {
            if (isWallGrabbingActive)
            {
                isWallGrabbingActive = false;
                rb.gravityScale = defaultGravity;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }

        if (isGrounded && currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        }
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
            jumpForceVector = Vector2.up * wallJumpForce;
            currentStamina -= wallJumpStaminaCost;
        }
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        rb.AddForce(jumpForceVector, ForceMode2D.Impulse);

        if (isPressingAwayFromWall)
        {
            if ((isFacingRight && wallDir == -1) || (!isFacingRight && wallDir == 1) && !isControlBlocked)
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
    private void TryLedgeClimb()
    {
        Vector2 dir = isFacingRight ? Vector2.right : Vector2.left;

        bool wallBelow = Physics2D.OverlapBox(wallCheckPos.position, wallCheckSize, 0f, wallLayer);
        if (!wallBelow)
        {
            return;
        }

        Vector2 headCheckPos = (Vector2)wallCheckPos.position + Vector2.up * ledgeCheckUp;
        Vector2 headBoxSize = new Vector2(0.4f, 0.6f);
        bool blockedAbove = Physics2D.OverlapBox(headCheckPos, headBoxSize, 0f, wallLayer);
        if (blockedAbove)
        {
            return;
        }

        Vector2 probePoint = (Vector2)wallCheckPos.position + dir * ledgeCheckForward + Vector2.up * ledgeCheckUp;

        RaycastHit2D hitDown = Physics2D.Raycast(probePoint, Vector2.down, ledgeCheckDownDist, groundLayer);
        if (!hitDown)
        {
            return;
        }

        Vector3 targetPos = new Vector3(
            hitDown.point.x - (isFacingRight ? ledgePullBack : -ledgePullBack),
            hitDown.point.y + ledgeClimbYOffset,
            transform.position.z
        );

        StartCoroutine(LedgeClimbRoutine(targetPos));
    }

    private IEnumerator LedgeClimbRoutine(Vector3 targetPos)
    {
        isClimbingLedge = true;
        isWallGrabbingActive = false;
        isGrabbingWall = false;

        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        Vector3 start = transform.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / ledgeClimbTime;
            transform.position = Vector3.Lerp(start, targetPos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        transform.position = targetPos;

        rb.gravityScale = defaultGravity;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        yield return new WaitForSeconds(0.05f);

        isClimbingLedge = false;
    }
    void OnGUI()
    {
        if (isControlBlocked) return;
        if (Time.timeScale == 0) return;

        // Показуємо ТІЛЬКИ коли натиснуто Shift
        if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
            return;

        Camera cam = Camera.main;
        if (cam == null) return;

        // Позиція над гравцем у world space
        Vector3 worldPos = transform.position + staminaLabelOffset;

        // Переводимо у screen space
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        // Якщо гравець за камерою — не малюємо
        if (screenPos.z < 0) return;

        // Інверсія Y для GUI
        float guiY = Screen.height - screenPos.y;

        string staminaText = $"{Mathf.FloorToInt(currentStamina)}";
        Vector2 size = GUI.skin.label.CalcSize(new GUIContent(staminaText));

        Rect rect = new Rect(
            screenPos.x - size.x / 2f,
            guiY - size.y,
            size.x,
            size.y
        );

        // Зберігаємо попередній колір GUI
        Color prevColor = GUI.color;

        // 🔲 ЧОРНЕ ОБВЕДЕННЯ (outline)
        GUI.color = Color.black;
        GUI.Label(new Rect(rect.x - 1, rect.y, rect.width, rect.height), staminaText);
        GUI.Label(new Rect(rect.x + 1, rect.y, rect.width, rect.height), staminaText);
        GUI.Label(new Rect(rect.x, rect.y - 1, rect.width, rect.height), staminaText);
        GUI.Label(new Rect(rect.x, rect.y + 1, rect.width, rect.height), staminaText);

        // ✨ ОСНОВНИЙ ТЕКСТ
        GUI.color = Color.white;
        GUI.Label(rect, staminaText);

        // Повертаємо колір назад
        GUI.color = prevColor;
    }

    #endregion

    #region Dashing
    public void Dash(InputAction.CallbackContext context)
    {
        //if (!canDash) return;
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
            StepUp();
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
        isGrounded = Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer);

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
        if (isControlBlocked || Time.timeScale == 0f) return;
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