using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement: MonoBehaviour
{
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

    private void Start()
    {
        remainingDashes = maxDashes;
    }

    void Update()
    {
        GroundCheck();

        // Processing buffers

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

        if (!isDashing)
        {
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
            Flip();
        }

        // Refill dash if grounded
        if (isGrounded && remainingDashes < maxDashes && !isDashing && !dashOnCooldown)
        {
            remainingDashes = maxDashes;
        }
    }

    // Окремий метод для стрибка
    private void DoJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
    }

    // Методи для прийому інпуту
    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        horizontalMovement = moveInput.x;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            jumpBufferTimer = jumpBufferTime;
        }
    }

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
        dashOnCooldown = true;
        remainingDashes--;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        Vector2 dashDir = inputDirection == Vector2.zero
            ? (isFacingRight ? Vector2.right : Vector2.left)
            : inputDirection.normalized;

        rb.linearVelocity = Vector2.zero;

        float dashTimer = 0f;
        while (dashTimer < dashDuration)
        {
            rb.MovePosition(rb.position + dashDir * dashSpeed * Time.fixedDeltaTime);
            dashTimer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        rb.gravityScale = originalGravity;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        dashOnCooldown = false;
    }

    private void GroundCheck()
    {
        isGrounded = Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer);
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
    }
}