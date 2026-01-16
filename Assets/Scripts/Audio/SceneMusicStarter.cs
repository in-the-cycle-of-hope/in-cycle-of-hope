using UnityEngine;

public class SceneMusicStarter : MonoBehaviour
{
    public AudioClip sceneMusic;
    void Start()
    {
        AudioManager.Instance.PlayMusic(sceneMusic);
    }
}
