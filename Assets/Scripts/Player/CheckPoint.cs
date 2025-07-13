using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    private PlayerMovement playerRespawn;

    void Start()
    {
        playerRespawn = GameObject.Find("Player").GetComponent<PlayerMovement>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.name == "Player")
        {
            playerRespawn.respawnPoint = transform.position;
        }
    }
}
