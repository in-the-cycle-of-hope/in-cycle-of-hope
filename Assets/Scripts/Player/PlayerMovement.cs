using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement: MonoBehaviour
{
    #region Initialization
    public Rigidbody2D rb;
    bool isFacingRight = true;

    [Header("Movement")]
    public float moveSpeed = 5f;
    float horizontalMovement;
    Vector2 moveInput;

    [Header("Jumping")]
    public float jumpPower = 10f;
    private bool isGrounded;

    // buffer jump and coyote time
    private float jumpBufferTime = 0.15f;
    private float jumpBufferTimer = 0f;

    private float coyoteTime = 0.15f;
    private float coyoteTimer = 0f;

    [Header("GroundCheck")]
    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask groundLayer;

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

    // Step-up mechanic
    [Header("Player step-up")]
    [SerializeField] Transform stepRayUpper;
    [SerializeField] Transform stepRayLower;
    [SerializeField] float stepHeight = 0.08f;
    [SerializeField] float stepSmooth = 0.04f;
    [SerializeField] LayerMask Ground;
    #endregion

    private void Start()
    {
        remainingDashes = maxDashes;
    }

    void Update()
    {
        GroundCheck();

        if (jumpBufferTimer > 0)
            jumpBufferTimer -= Time.deltaTime;

        if (dashBufferTimer > 0)
            dashBufferTimer -= Time.deltaTime;

        if (isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        if (jumpBufferTimer > 0 && (isGrounded || coyoteTimer > 0) && !isDashing)
        {
            DoJump();
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }

        if (dashBufferTimer > 0 && !isDashing && !dashOnCooldown && remainingDashes > 0)
        {
            Vector2 dashInput = moveInput;
            if (dashInput == Vector2.zero)
                dashInput = isFacingRight ? Vector2.right : Vector2.left;

            StartCoroutine(DashCoroutine(dashInput.normalized));
            dashBufferTimer = 0f;
        }

        Flip();

        if (isGrounded && remainingDashes < maxDashes && !isDashing)
        {
            remainingDashes = maxDashes;
            dashOnCooldown = false;
        }
    }
    private void FixedUpdate()
    {
        if (!isDashing)
        {
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
            Flip();
            StepUp();
        }
    }

    #region Movement
    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        horizontalMovement = moveInput.x;
    }
    void StepUp()
    {
        Vector2 dir = Vector2.right * transform.localScale.x;

        RaycastHit2D hitLower = Physics2D.Raycast(stepRayLower.position, dir, 0.1f, groundLayer);
        Debug.DrawRay(stepRayLower.position, dir * 0.1f, Color.red);

        if (hitLower.collider != null)
        {
            RaycastHit2D hitUpper = Physics2D.Raycast(stepRayUpper.position, dir, 0.2f, groundLayer);
            Debug.DrawRay(stepRayUpper.position, dir * 0.2f, Color.green);

            if (hitUpper.collider == null)
            {
                float obstacleHeight = hitLower.point.y - rb.position.y;

                if (obstacleHeight <= stepHeight + 0.01f)
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
        }
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
        rb.gravityScale = 0f;

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

        if (!isGrounded && remainingDashes <= 0)
        {
            dashOnCooldown = true;
            yield return new WaitForSeconds(dashCooldown);
            dashOnCooldown = false;
            remainingDashes = maxDashes;
        }
    }
    #endregion

    private void GroundCheck()
    {
        isGrounded = Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer);
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
    }
}