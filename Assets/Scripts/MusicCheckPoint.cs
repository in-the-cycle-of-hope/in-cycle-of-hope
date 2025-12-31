using UnityEngine;

public class MusicCheckPoint : MonoBehaviour
{
    public AudioClip checkpointMusic;

    bool triggered;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;
            AudioManager.Instance.PlayMusic(checkpointMusic);
        }
    }
}
