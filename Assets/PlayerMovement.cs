using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    bool isFacingRight = true;

    [Header("Movement")]
    public float moveSpeed = 5f;
    float horizontalMovement;

    [Header("Jumping")]
    public float jumpPower = 10f;
    private bool isGrounded;

    [Header("GroundCheck")]
    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask groundLayer;

    [Header("Gravity")]
    public float baseGravity = 2f;
    public float maxFallSpeed = 18f;
    public float fallSpeedMultiplier = 2f;

    [Header("Dash")]
    private Vector2 dashStartPos;
    private float dashDistance = 3f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.1f;
    public float dashCooldown = 0.5f;

    [Header("WallCheck")]
    public Transform wallCheckPos;
    public Vector2 wallCheckSize = new Vector2(0.5f, 1f);
    public LayerMask wallLayer;

    [Header("WallMovement")]
    public float wallSlideSpeed = 2f;
    bool isWallSliding;

    private bool canDash = true;
    private bool isDashing = false;
    private float dashTimeLeft;
    private Vector2 dashDirection;
    private float dashCooldownTimer;
    private Vector2 lastDashInput = Vector2.zero;

    // Wall Jump
    bool isWallJumping;
    float wallJumpDirection;
    float wallJumpTime = 0.5f;
    float wallJumpTimer;
    public Vector2 wallJumpPower = new Vector2(5f, 10f);

    void Update()
    {
        GroundCheck();
        ProcessWallSlide();
        ProcessWallJump();
        ProcessGravity();

        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        if (isDashing)
        {
            float dashProgress = 1f - (dashTimeLeft / dashDuration);
            Vector2 targetPos = dashStartPos + dashDirection * dashDistance;
            Vector2 newPos = Vector2.Lerp(dashStartPos, targetPos, dashProgress);
            rb.MovePosition(newPos);

            dashTimeLeft -= Time.deltaTime;
            if (dashTimeLeft <= 0f)
            {
                isDashing = false;
                rb.gravityScale = baseGravity;
            }
            return;
        }

        if (!isWallJumping)
        {
            rb.linearVelocity = new Vector2(horizontalMovement * moveSpeed, rb.linearVelocity.y);
        }

        Flip();
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (isGrounded && context.performed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
        }
        else if (context.canceled && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }

        // Wall Jump
        if (context.performed && wallJumpTimer > 0f)
        {
            isWallJumping = true;
            rb.linearVelocity = new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y);
            wallJumpTimer = 0;

            // Примусовий Flip
            if ((wallJumpDirection > 0 && !isFacingRight) || (wallJumpDirection < 0 && isFacingRight))
            {
                FlipImmediate();
            }

            Invoke(nameof(CancelWallJump), wallJumpTime);
        }
    }

    private void CancelWallJump()
    {
        isWallJumping = false;
    }

    private void GroundCheck()
    {
        isGrounded = Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer);
        if (isGrounded)
        {
            canDash = true;
        }
    }

    private bool WallCheck()
    {
        return Physics2D.OverlapBox(wallCheckPos.position, wallCheckSize, 0, wallLayer);
    }

    private void ProcessWallSlide()
    {
        if (!isGrounded && WallCheck() && horizontalMovement != 0)
        {
            isWallSliding = true;
            if (rb.linearVelocity.y < -wallSlideSpeed)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void ProcessWallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpDirection = -Mathf.Sign(transform.localScale.x);
            wallJumpTimer = wallJumpTime;
            CancelInvoke(nameof(CancelWallJump));
        }
        else if (wallJumpTimer > 0f)
        {
            wallJumpTimer -= Time.deltaTime;
        }
    }

    private void ProcessGravity()
    {
        if (!isDashing)
        {
            if (rb.linearVelocity.y < 0 && !isWallSliding)
            {
                rb.gravityScale = baseGravity * fallSpeedMultiplier;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -maxFallSpeed));
            }
            else
            {
                rb.gravityScale = baseGravity;
            }
        }
    }

    private void Flip()
    {
        if (horizontalMovement > 0 && !isFacingRight)
        {
            FlipImmediate();
        }
        else if (horizontalMovement < 0 && isFacingRight)
        {
            FlipImmediate();
        }
    }

    private void FlipImmediate()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public void DashDirection(InputAction.CallbackContext context)
    {
        lastDashInput = context.ReadValue<Vector2>();
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed && canDash && dashCooldownTimer <= 0f)
        {
            Vector2 inputDir = lastDashInput.normalized;

            if (inputDir == Vector2.zero)
                inputDir = isFacingRight ? Vector2.right : Vector2.left;

            dashDirection = inputDir.normalized;
            dashStartPos = rb.position; // стартова позиція дашу

            isDashing = true;
            canDash = false;
            dashTimeLeft = dashDuration;
            dashCooldownTimer = dashCooldown;

            // Обрізати гравітацію і поточну швидкість
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(wallCheckPos.position, wallCheckSize);
    }
}
