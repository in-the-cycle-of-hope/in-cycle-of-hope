using UnityEngine;

public class DeathZone : MonoBehaviour
{
    public Transform assignedCheckpoint;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // 1. Îòğèìóºìî Rigidbody2D ãğàâöÿ
            Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // ÏĞÀÂÈËÜÍÀ ÍÀÇÂÀ: RigidbodyType2D
                rb.bodyType = RigidbodyType2D.Static;
            }

            if (AudioManager.Instance) AudioManager.Instance.PlaySFX(AudioManager.Instance.death);

            PlayerMovement playerRespawn = collision.gameObject.GetComponent<PlayerMovement>();
            if (playerRespawn != null)
            {
                StartCoroutine(playerRespawn.FadeRespawnTo(assignedCheckpoint.position));
            }
        }
    }
}
