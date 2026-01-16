using System.Collections;
using UnityEngine;

public class PlayerOutro : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform endPoint;
    public float moveDuration = 3f;
    public float animationEndDelay = 1f;

    [Header("Sync with Fungus")]
    [Tooltip("Через скільки секунд після початку бігу надіслати сигнал у Fungus?")]
    public float timeToTriggerFungus = 1.5f;
    public string fungusMessage = "StartCredits";

    [Header("References")]
    public Animator animator;
    private bool isPlaying;

    public void PlayOutro()
    {
        if (isPlaying) return;
        StartCoroutine(OutroRoutine());
    }

    private IEnumerator OutroRoutine()
    {
        isPlaying = true;
        PlayerMovement mov = GetComponent<PlayerMovement>();

        mov.isIntroPlaying = true;
        mov.isControlBlocked = true;

        animator.SetBool("isDashing", false);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        animator.SetBool("isRunning", true);
        animator.SetBool("isGrounded", true);

        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(endPoint.position.x, transform.position.y, transform.position.z);

        bool messageSent = false;

        while (elapsed < moveDuration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / moveDuration);

            elapsed += Time.deltaTime;

            if (!messageSent && elapsed >= timeToTriggerFungus)
            {
                messageSent = true;
                Fungus.Flowchart.BroadcastFungusMessage(fungusMessage);
            }

            yield return null;
        }

        transform.position = targetPos;
        animator.SetBool("isRunning", false);

        yield return new WaitForSeconds(animationEndDelay);

        isPlaying = false;
    }
}
