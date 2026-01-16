using UnityEngine;

public class MusicCheckPoint : MonoBehaviour
{
    public AudioClip checkpointMusic;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayMusic(checkpointMusic);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopMusic();
            }
        }
    }
}
