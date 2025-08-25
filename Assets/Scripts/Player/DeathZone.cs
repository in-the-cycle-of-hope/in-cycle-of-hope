using UnityEngine;

public class DeathZone : MonoBehaviour
{
    public Transform assignedCheckpoint;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMovement playerRespawn = collision.gameObject.GetComponent<PlayerMovement>();
            if (playerRespawn != null)
            {
                StartCoroutine(playerRespawn.FadeRespawnTo(assignedCheckpoint.position));
            }
        }
    }
}
