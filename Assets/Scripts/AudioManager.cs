using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music")]
    public AudioClip menuMusic;
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("UI Sounds")]
    public AudioClip menuMove;
    public AudioClip menuConfirm;
    public AudioClip move;
    public AudioClip jump;
    public AudioClip dash;
    public AudioClip climb;
    public AudioClip death;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource.clip == clip) return;
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
}
