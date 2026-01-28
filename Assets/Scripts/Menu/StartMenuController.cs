using Fungus;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Navigation")]
    [SerializeField] private GameObject firstSelectedButton;
    [SerializeField] private GameObject firstSettingsButton;
    public Button continueButton;
    private GameObject lastSelected;

    [Header("Fading Systems")]
    public Animator fadeAnimatorStart;
    public Animator fadeAnimatorExit;
    public GameObject blackScreenStart;
    public GameObject blackScreenExit;

    void Start()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusic(AudioManager.Instance.menuMusic);

        Time.timeScale = 1f;

        if (blackScreenStart != null)
        {
            blackScreenStart.SetActive(true);
            StartCoroutine(FadeInRoutine());
        }

        continueButton.interactable = SaveSystem.CanContinue();

        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);

        StartCoroutine(SelectObject(firstSelectedButton));
    }

    public void OnStartClick()
    {
        PlayConfirm();
        SaveSystem.ClearSave();
        StartCoroutine(SwitchSceneWithFade("SampleScene"));
    }

    private IEnumerator SwitchSceneWithFade(string sceneName)
    {
        blackScreenExit.SetActive(true);
        fadeAnimatorExit.SetTrigger("BlackScreen");

        yield return new WaitForSecondsRealtime(1f);

        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeInRoutine()
    {
        yield return new WaitForEndOfFrame();

        if (fadeAnimatorStart != null)
        {
            fadeAnimatorStart.Play("BlackScreenOut", -1, 0f);
        }

        yield return new WaitForSecondsRealtime(1.0f);
        blackScreenStart.SetActive(false);
    }

    public void OpenSettings()
    {
        PlayConfirm();
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        StartCoroutine(SelectObject(firstSettingsButton));
    }

    public void CloseSettings()
    {
        PlayConfirm();
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        StartCoroutine(SelectObject(firstSelectedButton));
    }
    public void ContinueGame()
    {
        PlayConfirm();
        StartCoroutine(SwitchSceneWithFade(SaveSystem.LoadScene()));
    }
    public void OnExitClick()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.menuConfirm);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    void Update()
    {
        GameObject current = EventSystem.current.currentSelectedGameObject;
        if (current == null && lastSelected != null) EventSystem.current.SetSelectedGameObject(lastSelected);
        else if (current != lastSelected && current != null)
        {
            PlayMove();
            lastSelected = current;
        }
    }
    private void PlayConfirm() { if (AudioManager.Instance) AudioManager.Instance.PlaySFX(AudioManager.Instance.menuConfirm); }
    private void PlayMove() { if (AudioManager.Instance) AudioManager.Instance.PlaySFX(AudioManager.Instance.menuMove); }
    private IEnumerator SelectObject(GameObject target)
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(target);
    }
}
