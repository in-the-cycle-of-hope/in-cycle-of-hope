using UnityEngine;
using UnityEngine.Audio;

[DefaultExecutionOrder(-100)]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Mixers")]
    public AudioMixer mainMixer;
    public AudioMixer fungusMixer;

    [Header("Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music")]
    public AudioClip menuMusic;
    public AudioClip subtitlesMusic;

    [Header("UI Sounds")]
    public AudioClip menuMove;
    public AudioClip menuConfirm;
    public AudioClip crack;

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

    void Start()
    {
        LoadVolume();
    }

    public void LoadVolume()
    {
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        SetMixerVolume("MusicVol", musicVol);
        SetMixerVolume("SFXVol", sfxVol);
    }

    public void SetMixerVolume(string parameterName, float value)
    {
        float dbValue = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20;
        mainMixer.SetFloat(parameterName, dbValue);

        if (parameterName == "SFXVol" && fungusMixer != null)
        {
            fungusMixer.SetFloat(parameterName, dbValue);
        }
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
