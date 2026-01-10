using UnityEngine;

public class DeathZone : MonoBehaviour
{
    public Transform assignedCheckpoint;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Зіткнення з: " + collision.gameObject.name); // Це з'явиться в консолі?
        if (!collision.gameObject.CompareTag("Player")) return;

        PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
        if (player != null)
        {
            Debug.Log("Викликаю Die()");
            player.Die(assignedCheckpoint.position);
        }
    }
}
