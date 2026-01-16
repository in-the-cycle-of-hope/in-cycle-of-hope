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
    public Animator fadeAnimator1;
    public Animator fadeAnimator2;
    public GameObject blackScreen1;
    public GameObject blackScreen2;

    void Start()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusic(AudioManager.Instance.menuMusic);

        Time.timeScale = 1f;

        StartCoroutine(FadeInRoutine());

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
        blackScreen1.SetActive(true);
        fadeAnimator1.SetTrigger("BlackScreen");

        yield return new WaitForSecondsRealtime(1f);

        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeInRoutine()
    {
        if (blackScreen2 != null)
        {
            blackScreen2.SetActive(true);
            fadeAnimator2.SetTrigger("BlackScreen");
            yield return new WaitForSecondsRealtime(1f);
            blackScreen2.SetActive(false);
        }
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
