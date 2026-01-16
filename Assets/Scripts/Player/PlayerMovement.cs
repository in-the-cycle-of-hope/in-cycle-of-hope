using Fungus;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class PlayerMovement : MonoBehaviour
{
    #region Initialization
    public Flowchart flowchart;
    public DialogTrigger currentInteractable;
    public Rigidbody2D rb;
    public Vector3 respawnPoint;
    public Animator fadeAnimator;
    public Animator fadeAnimator2;
    public GameObject blackScreen;
    public GameObject blackScreen2;
    public PlayerInput playerInput;
    public bool isControlBlocked;

    [Header("GroundCheck")]
    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask groundLayer;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float horizontalMovement;
    public Vector2 moveInput;
    private float rawHorizontalInput;

    [Header("Jumping")]
    public float jumpPower = 10f;
    private bool isGrounded;
    private bool wasGroundedLastFrame;
    private float jumpBufferTime = 0.15f;
    private float jumpBufferTimer;
    private float coyoteTime = 0.15f;
    private float coyoteTimer;

    [Header("Wall settings")]
    [SerializeField] private Transform wallCheckPos;
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.5f, 1f);
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallClimbSpeed = 3f;
    private bool isGrabbingWall;
    private bool isWallDetected;
    private bool isWallGrabbingActive;
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
    [SerializeField] private float wallJumpGracePeriod = 0.12f;
    [SerializeField] private float wallJumpInputBufferTime = 0.08f;
    [SerializeField] private float horizontalInputThreshold = 0.2f;
    private float wallJumpGraceTimer;
    private float wallJumpInputBufferTimer;
    private float bufferedWallJumpInputX;
    private bool isWallJumping;
    private float wallJumpTimer;

    [Header("Ledge Climb")]
    [SerializeField] private float ledgeCheckUp = 1.0f;
    [SerializeField] private float ledgeCheckForward = 0.25f;
    [SerializeField] private float ledgeCheckDownDist = 1.5f;
    [SerializeField] private float ledgeClimbTime = 0.15f;
    [SerializeField] private float ledgeClimbYOffset = 0.35f;
    [SerializeField] private float ledgePullBack = 0.10f;
    private bool isClimbingLedge;

    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.5f;
    public int maxDashes = 1;
    private float dashBufferTime = 0.15f;
    private float dashBufferTimer;
    private bool isDashing;
    private bool dashOnCooldown;
    private int remainingDashes;
    private Vector2 dashDirection;

    [Header("Player step-up")]
    [SerializeField] private Transform stepRayUpper;
    [SerializeField] private Transform stepRayLower;
    [SerializeField] private float stepHeight = 0.08f;
    [SerializeField] private float stepSmooth = 0.04f;
    [SerializeField] private LayerMask Ground;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private float idleLong1Delay = 5f;
    [SerializeField] private float idleLong2Delay = 5f;
    private float idleTimer;
    private bool idleLong1Played;
    private bool idleLong2Played;
    private bool idleForced;
    public bool isDead;
    private int deathLayerIndex;
    private bool isRespawning;
    public bool isIntroPlaying = false;

    [Header("Death")]
    [SerializeField] private float deathAnimationDuration = 0.5f;

    [Header("Tutorial Abilities")]
    public bool canJump;
    public bool canDash;
    public bool canGrabWall;

    [Header("Dialogue Lock")]
    public bool isInDialogue;
    public TextMeshProUGUI staminaTextDisplay;

    private bool isFacingRight = true;
    private RaycastHit2D[] lowerRayHitBuffer = new RaycastHit2D[1];
    private RaycastHit2D[] upperRayHitBuffer = new RaycastHit2D[1];
    private WaitForSeconds _dashCooldownWait;
    private WaitForSeconds _wallJumpDurationWait;
    #endregion

    private void Awake()
    {
        SaveSystem.LoadAbilities(this);
        playerInput = GetComponent<PlayerInput>();
        deathLayerIndex = animator.GetLayerIndex("DeathLayer");
        if (deathLayerIndex == -1) deathLayerIndex = 1;

        _dashCooldownWait = new WaitForSeconds(dashCooldown);
        _wallJumpDurationWait = new WaitForSeconds(wallJumpDuration);
        defaultGravity = rb.gravityScale;
    }

    private void Start()
    {
        remainingDashes = maxDashes;
        currentStamina = maxStamina;

        if (SaveSystem.CanContinue())
        {
            transform.position = SaveSystem.LoadPosition();
            animator.SetBool("isGrounded", true);
            animator.SetFloat("yVelocity", 0f);
            animator.SetBool("isRunning", false);
            animator.Play("Idle", 0, 0f);
            animator.Update(0f);
        }
    }

    private void Update()
    {
        if (isControlBlocked)
        {
            moveInput = Vector2.zero;
            GroundCheck();
            if (isInDialogue && !isIntroPlaying)
            {
                if (isGrounded) ForceIdleAnimation();
                else
                {
                    animator.SetBool("isGrounded", false);
                    animator.SetFloat("yVelocity", rb.linearVelocity.y);
                }
            }
            return;
        }

        GroundCheck();
        jumpBufferTimer -= Time.deltaTime;
        dashBufferTimer -= Time.deltaTime;
        wallJumpGraceTimer -= Time.deltaTime;
        if (wallJumpInputBufferTimer > 0) wallJumpInputBufferTimer -= Time.deltaTime;

        if (isGrounded) coyoteTimer = coyoteTime;
        else coyoteTimer -= Time.deltaTime;

        HandleWallGrabbing();

        if (jumpBufferTimer > 0 && (isGrounded || coyoteTimer > 0) && !isDashing && !isWallJumping)
        {
            DoJump();
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }

        if (dashBufferTimer > 0 && !isDashing && !dashOnCooldown && remainingDashes > 0 && !isWallGrabbingActive)
        {
            Vector2 dashInput = moveInput == Vector2.zero ? (isFacingRight ? Vector2.right : Vector2.left) : moveInput;
            StartCoroutine(DashCoroutine(dashInput.normalized));
            dashBufferTimer = 0f;
        }

        if ((isGrounded || Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer)) && remainingDashes < maxDashes && !isDashing)
        {
            remainingDashes = maxDashes;
            dashOnCooldown = false;
        }

        if (isWallJumping)
        {
            wallJumpTimer -= Time.deltaTime;
            if (wallJumpTimer <= 0) isWallJumping = false;
        }

        UpdateAnimations();
        UpdateStaminaUI();
    }

    private void FixedUpdate()
    {
        if (isControlBlocked)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }
        if (isDashing || isWallJumping) return;

        if (isWallGrabbingActive && !isClimbingLedge) TryLedgeClimb();

        if (isWallGrabbingActive)
        {
            if (Mathf.Abs(moveInput.y) > 0.01f)
            {
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                rb.linearVelocity = new Vector2(0f, moveInput.y * wallClimbSpeed);
                if (moveInput.y > 0.01f) currentStamina -= climbStaminaDrain * Time.fixedDeltaTime;
                else if (moveInput.y < -0.01f) currentStamina -= descendStaminaDrain * Time.fixedDeltaTime;
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

    #region Tutorial API (Fungus)
    public void EnableJump() { canJump = true; SaveSystem.SaveAbilities(canJump, canDash, canGrabWall); }
    public void EnableDash() { canDash = true; SaveSystem.SaveAbilities(canJump, canDash, canGrabWall); }
    public void EnableWallGrab() { canGrabWall = true; SaveSystem.SaveAbilities(canJump, canDash, canGrabWall); }
    #endregion

    #region Movement
    public void Move(InputAction.CallbackContext context)
    {
        if (isControlBlocked) return;
        Vector2 rawInput = context.ReadValue<Vector2>();
        rawHorizontalInput = rawInput.x;
        if (isWallGrabbingActive)
        {
            moveInput = new Vector2(0f, rawInput.y);
            horizontalMovement = 0f;
        }
        else
        {
            moveInput = rawInput;
            horizontalMovement = moveInput.x;
        }
        Flip();
    }

    void StepUp()
    {
        Vector2 dir = Vector2.right * transform.localScale.x;
        if (Physics2D.RaycastNonAlloc(stepRayLower.position, dir, lowerRayHitBuffer, 0.1f, Ground) > 0)
        {
            if (Physics2D.RaycastNonAlloc(stepRayUpper.position, dir, upperRayHitBuffer, 0.2f, Ground) == 0)
            {
                if (lowerRayHitBuffer[0].point.y - rb.position.y <= stepHeight + 0.01f && Mathf.Abs(moveInput.x) > 0.01f)
                {
                    rb.position += Vector2.up * stepSmooth;
                }
            }
        }
    }

    private void Flip()
    {
        if (isControlBlocked || Time.timeScale == 0f) return;
        if ((moveInput.x > 0 && !isFacingRight) || (moveInput.x < 0 && isFacingRight)) FlipImmediate();
    }

    private void FlipImmediate()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    #endregion

    #region Jumping
    private void DoJump() => rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);

    public void Jump(InputAction.CallbackContext context)
    {
        if (!canJump || !context.started || isDashing) return;
        jumpBufferTimer = jumpBufferTime;
        if ((isWallDetected && isGrabbingWall && !isGrounded) || (wallJumpGraceTimer > 0 && isWallDetected))
        {
            bufferedWallJumpInputX = rawHorizontalInput;
            wallJumpInputBufferTimer = wallJumpInputBufferTime;
            if (!isGrabbingWall) return;

            bool intendsWrong = (isFacingRight && bufferedWallJumpInputX > horizontalInputThreshold) || (!isFacingRight && bufferedWallJumpInputX < -horizontalInputThreshold);
            if (intendsWrong || (moveInput.y > 0.5f && currentStamina < wallJumpStaminaCost)) return;

            wallJumpGraceTimer = 0f;
            DoWallJump();
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }
    }
    #endregion

    #region Climb
    public void Grab(InputAction.CallbackContext context)
    {
        if (!canGrabWall) return;
        if (context.started) isGrabbingWall = true;
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
        bool shouldCheckWall = !isGrounded && (isGrabbingWall || wallJumpGraceTimer > 0 || wallJumpInputBufferTimer > 0);
        isWallDetected = shouldCheckWall && Physics2D.OverlapBox(wallCheckPos.position, wallCheckSize, 0f, wallLayer);

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

        if (isGrabbingWall && isWallDetected && !isGrounded && !isDashing && !isOutOfStamina)
        {
            if (!isWallGrabbingActive)
            {
                isWallGrabbingActive = true;
                rb.gravityScale = 0f;
                rb.linearVelocity = Vector2.zero;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;
            }
        }
        else if (isWallGrabbingActive)
        {
            isWallGrabbingActive = false;
            rb.gravityScale = defaultGravity;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        if (isGrounded && currentStamina < maxStamina)
        {
            currentStamina = Mathf.Min(currentStamina + staminaRegenRate * Time.deltaTime, maxStamina);
        }
    }

    private void DoWallJump()
    {
        isWallJumping = true;
        wallJumpTimer = wallJumpDuration;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = defaultGravity;
        int wallDir = transform.localScale.x > 0 ? -1 : 1;
        bool isPressingAway = (isFacingRight && bufferedWallJumpInputX < -horizontalInputThreshold) || (!isFacingRight && bufferedWallJumpInputX > horizontalInputThreshold);
        Vector2 force;
        if (moveInput.y > 0.5f || !isPressingAway)
        {
            force = Vector2.up * wallJumpForce;
            currentStamina -= wallJumpStaminaCost;
        }
        else force = new Vector2(wallJumpClimb.x * wallDir, wallJumpClimb.y);

        currentStamina = Mathf.Max(0, currentStamina);
        rb.AddForce(force, ForceMode2D.Impulse);
        if (isPressingAway) FlipImmediate();
        StartCoroutine(WallJumpRoutine());
        wallJumpGraceTimer = wallJumpGracePeriod;
        bufferedWallJumpInputX = 0f;
        wallJumpInputBufferTimer = 0f;
    }

    private IEnumerator WallJumpRoutine()
    {
        yield return _wallJumpDurationWait;
        isWallJumping = false;
    }

    private void TryLedgeClimb()
    {
        Vector2 dir = isFacingRight ? Vector2.right : Vector2.left;
        if (!Physics2D.OverlapBox(wallCheckPos.position, wallCheckSize, 0f, wallLayer)) return;
        if (Physics2D.OverlapBox((Vector2)wallCheckPos.position + Vector2.up * ledgeCheckUp, new Vector2(0.4f, 0.6f), 0f, wallLayer)) return;
        RaycastHit2D hitDown = Physics2D.Raycast((Vector2)wallCheckPos.position + dir * ledgeCheckForward + Vector2.up * ledgeCheckUp, Vector2.down, ledgeCheckDownDist, groundLayer);
        if (!hitDown) return;

        Vector3 target = new Vector3(hitDown.point.x - (isFacingRight ? ledgePullBack : -ledgePullBack), hitDown.point.y + ledgeClimbYOffset, transform.position.z);
        StartCoroutine(LedgeClimbRoutine(target));
    }

    private IEnumerator LedgeClimbRoutine(Vector3 targetPos)
    {
        isClimbingLedge = true;
        isWallGrabbingActive = isGrabbingWall = false;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
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
        yield return new WaitForSeconds(0.05f);
        isClimbingLedge = false;
    }
    #endregion

    #region Dashing
    public void Dash(InputAction.CallbackContext context)
    {
        if (canDash && context.started) dashBufferTimer = dashBufferTime;
    }

    private IEnumerator DashCoroutine(Vector2 inputDirection)
    {
        isDashing = true;
        rb.gravityScale = defaultGravity;
        dashDirection = inputDirection;
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            rb.MovePosition(rb.position + dashDirection * dashSpeed * Time.fixedDeltaTime);
            StepUp();
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
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

    #region Helper Methods
    private void GroundCheck()
    {
        isGrounded = Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer);
        if (isGrounded)
        {
            if (rb.gravityScale != defaultGravity) rb.gravityScale = defaultGravity;
            if (!wasGroundedLastFrame) currentStamina = maxStamina;
        }
        wasGroundedLastFrame = isGrounded;
    }

    void UpdateAnimations()
    {
        if (isDead || isRespawning) return;
        animator.SetBool("isDashing", isDashing);
        if (isDashing)
        {
            animator.SetBool("isRunning", false);
            animator.SetBool("isWallGrabbing", false);
            animator.SetFloat("yVelocity", 0f);
            return;
        }

        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("yVelocity", rb.linearVelocity.y);
        animator.SetBool("isRunning", isGrounded && !isWallGrabbingActive && !isWallJumping && !isClimbingLedge && (Mathf.Abs(moveInput.x) > 0.01f || Mathf.Abs(rb.linearVelocity.x) > 0.1f));

        if (isGrounded && !isWallGrabbingActive && !isWallJumping && !isClimbingLedge && Mathf.Abs(moveInput.x) <= 0.01f && Mathf.Abs(rb.linearVelocity.x) < 0.05f)
        {
            idleTimer += Time.deltaTime;
            if (!idleLong1Played && idleTimer >= idleLong1Delay) { animator.SetTrigger("IdleLong1"); idleLong1Played = true; }
            if (!idleLong2Played && idleTimer >= idleLong2Delay) { animator.SetTrigger("IdleLong2"); idleLong2Played = true; }
        }
        else { idleTimer = 0f; idleLong1Played = idleLong2Played = false; }

        if (idleLong2Played && animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) { idleTimer = 0f; idleLong1Played = idleLong2Played = false; }

        animator.SetBool("isWallGrabbing", isWallGrabbingActive);
        animator.SetFloat("wallClimbSpeed", Mathf.Abs(moveInput.y));
        if (isWallGrabbingActive) { animator.SetFloat("yVelocity", 0f); animator.SetBool("isRunning", false); }
    }

    void UpdateStaminaUI()
    {
        if (staminaTextDisplay == null) return;
        staminaTextDisplay.text = Mathf.FloorToInt(currentStamina).ToString();
        bool show = canGrabWall && (isWallDetected || isWallGrabbingActive) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && !isDead;
        staminaTextDisplay.gameObject.SetActive(show);
        if (show)
        {
            Vector3 scale = staminaTextDisplay.transform.parent.localScale;
            scale.x = Mathf.Abs(scale.x) * (transform.localScale.x > 0 ? 1 : -1);
            staminaTextDisplay.transform.parent.localScale = scale;
            staminaTextDisplay.transform.parent.localPosition = staminaLabelOffset;
        }
    }

    public void Die(Vector3 respawnPosition)
    {
        if (isDead || isRespawning) return;
        isDead = true;
        isRespawning = true;
        respawnPoint = respawnPosition;
        BlockControl();
        animator.SetLayerWeight(deathLayerIndex, 1f);
        animator.Play("Death", deathLayerIndex, 0f);
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSecondsRealtime(deathAnimationDuration);
        yield return StartCoroutine(FadeRespawnTo(respawnPoint));
        RespawnManager.ResetWorld();
        isDead = isRespawning = false;
        UnblockControl();
    }

    public IEnumerator FadeRespawnTo(Vector3 newPosition)
    {
        if (playerInput != null) playerInput.enabled = false;
        if (blackScreen != null) { blackScreen.SetActive(true); fadeAnimator.Play("BlackScreenIn", -1, 0f); }
        yield return new WaitForSecondsRealtime(1.0f);
        transform.position = newPosition;
        animator.SetLayerWeight(deathLayerIndex, 0f);
        animator.Rebind();
        animator.Play("Idle", 0, 0f);
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSecondsRealtime(0.5f);
        if (blackScreen != null) blackScreen.SetActive(false);
        if (blackScreen2 != null) { blackScreen2.SetActive(true); fadeAnimator2.Play("BlackScreenOut", -1, 0f); }
        yield return new WaitForSecondsRealtime(1.0f);
        if (blackScreen2 != null) blackScreen2.SetActive(false);
        UnblockControl();
    }

    public void BlockControl()
    {
        isControlBlocked = true;
        if (playerInput != null) playerInput.enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
        animator.SetBool("isRunning", false);
    }

    public void UnblockControl()
    {
        isControlBlocked = false;
        if (playerInput != null) playerInput.enabled = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        moveInput = Vector2.zero;
        horizontalMovement = 0f;
    }

    private void ForceIdleAnimation()
    {
        if (idleForced) return;
        idleForced = true;
        animator.SetBool("isRunning", false);
        animator.SetBool("isDashing", false);
        animator.SetBool("isWallGrabbing", false);
        animator.SetBool("isGrounded", true);
        animator.SetFloat("yVelocity", 0f);
        animator.CrossFade("Idle", 0.1f);
        idleTimer = 0f;
        idleLong1Played = idleLong2Played = false;
    }

    public void EnterDialogue()
    {
        isInDialogue = true;
        BlockControl();
        isDashing = isWallGrabbingActive = isGrabbingWall = isWallJumping = isClimbingLedge = false;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        rb.gravityScale = defaultGravity;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        if (isGrounded) ForceIdleAnimation();
    }

    public void ExitDialogue()
    {
        isInDialogue = false;
        idleForced = false;
        UnblockControl();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(wallCheckPos.position, wallCheckSize);
    }
    #endregion
}