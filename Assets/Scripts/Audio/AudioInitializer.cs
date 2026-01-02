using UnityEngine;
using UnityEngine.Audio;

public class AudioInitializer : MonoBehaviour
{
    [Header("Mixers")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private AudioMixer fungusMixer;

    void Start()
    {
        // Викликаємо метод завантаження
        ApplyAllSettings();
    }

    public void ApplyAllSettings()
    {
        // Отримуємо збережені значення (0.75f - стандарт, якщо ще нічого не збережено)
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        // Конвертуємо у Децибели
        float musicDb = Mathf.Log10(Mathf.Max(musicVol, 0.0001f)) * 20;
        float sfxDb = Mathf.Log10(Mathf.Max(sfxVol, 0.0001f)) * 20;

        // Застосовуємо до основного мікшера
        mainMixer.SetFloat("MusicVol", musicDb);
        mainMixer.SetFloat("SFXVol", sfxDb);

        // Застосовуємо до мікшера Fungus
        if (fungusMixer != null)
        {
            // Якщо у фунгус-мікшері параметри називаються так само:
            fungusMixer.SetFloat("MusicVol", musicDb);
            fungusMixer.SetFloat("SFXVol", sfxDb);
        }

        Debug.Log("Звук ініціалізовано: Music " + musicVol + ", SFX " + sfxVol);
    }
}
