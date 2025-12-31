using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("Mixers")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private AudioMixer fungusMixer; // Посилання на другий мікшер

    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    void Start()
    {
        float mVol = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float sVol = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        musicSlider.value = mVol;
        sfxSlider.value = sVol;

        SetMusicVolume(mVol);
        SetSFXVolume(sVol);
    }

    public void SetMusicVolume(float value)
    {
        float dbValue = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20;

        mainMixer.SetFloat("MusicVol", dbValue);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        float dbValue = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20;

        // 1. Оновлюємо основний мікшер
        mainMixer.SetFloat("SFXVol", dbValue);

        // 2. Оновлюємо мікшер Fungus
        // Переконайтеся, що назва "SFXVol" (або "MyExposedParam 1") збігається з Exposed Parameter у Fungus Mixer
        if (fungusMixer != null)
        {
            fungusMixer.SetFloat("SFXVol", dbValue);
        }

        PlayerPrefs.SetFloat("SFXVolume", value);
    }
}
