using UnityEngine;
using UnityEngine.Audio;

public class AudioInitializer : MonoBehaviour
{
    [Header("Mixers")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private AudioMixer fungusMixer;

    void Start()
    {
        ApplyAllSettings();
    }

    public void ApplyAllSettings()
    {
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        float musicDb = Mathf.Log10(Mathf.Max(musicVol, 0.0001f)) * 20;
        float sfxDb = Mathf.Log10(Mathf.Max(sfxVol, 0.0001f)) * 20;

        mainMixer.SetFloat("MusicVol", musicDb);
        mainMixer.SetFloat("SFXVol", sfxDb);

        if (fungusMixer != null)
        {
            fungusMixer.SetFloat("MusicVol", musicDb);
            fungusMixer.SetFloat("SFXVol", sfxDb);
        }
    }
}
