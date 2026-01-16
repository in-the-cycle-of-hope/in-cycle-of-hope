using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music")]
    public AudioClip menuMusic;
    public AudioClip subtitlesMusic;

    [Header("Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("UI Sounds")]
    public AudioClip menuMove;
    public AudioClip menuConfirm;
    public AudioClip crack;
    public AudioClip dragging;

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
        if (clip == null || musicSource.clip == clip) return;
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }
    public void StopMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
            musicSource.clip = null;
        }
    }
    public void StopAllSounds()
    {
        musicSource.Stop();
        musicSource.clip = null;
        musicSource.volume = 1f;

        sfxSource.Stop();
    }
}
