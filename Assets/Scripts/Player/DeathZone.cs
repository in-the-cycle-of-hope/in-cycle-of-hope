using UnityEngine;

public class DeathZone : MonoBehaviour
{
    public Transform assignedCheckpoint;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
        if (player != null)
        {
            player.Die(assignedCheckpoint.position);
        }
    }
}
