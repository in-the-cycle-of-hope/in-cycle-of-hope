using UnityEngine;

public class TeleportTrigger : MonoBehaviour
{
    [Header("Teleport target")]
    public Transform teleportPoint;

    private bool isTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isTriggered) return;

        if (collision.CompareTag("Player"))
        {
            isTriggered = true;

            PlayerMovement player = collision.GetComponent<PlayerMovement>();

            if (player != null)
            {
                player.StartCoroutine(
                    player.FadeRespawnTo(teleportPoint.position)
                );
            }
        }
    }
}
