using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void OnEnable()
    {
        StartCoroutine(SyncSlidersRoutine());
    }
    private IEnumerator SyncSlidersRoutine()
    {
        yield return new WaitForEndOfFrame();

        float mVol = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float sVol = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        musicSlider.onValueChanged.RemoveAllListeners();
        sfxSlider.onValueChanged.RemoveAllListeners();

        musicSlider.value = mVol;
        sfxSlider.value = sVol;

        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }
    public void SetMusicVolume(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMixerVolume("MusicVol", value);

        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMixerVolume("SFXVol", value);

        PlayerPrefs.SetFloat("SFXVolume", value);
    }
}
