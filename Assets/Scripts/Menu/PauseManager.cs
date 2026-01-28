using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject settingsPanel;
    [SerializeField] GameObject controlsImage;
    [SerializeField] GameObject warningPanel;

    private float lastOpenedTime;

    [Header("Navigation")]
    [SerializeField] GameObject firstPauseButton;
    [SerializeField] GameObject firstSettingsButton;
    [SerializeField] GameObject firstWarningButton;
    private GameObject lastSelected;
    private GameObject buttonBeforeWindow;

    [Header("Player Reference")]
    [SerializeField] PlayerMovement playerMovement;

    [Header("Fading")]
    [SerializeField] GameObject blackScreenStart;
    [SerializeField] Animator fadeAnimatorStart;
    [SerializeField] GameObject blackScreenExit;
    [SerializeField] Animator fadeAnimatorExit;

    void Start()
    {
        Time.timeScale = 1f;

        if (blackScreenStart != null)
        {
            blackScreenStart.SetActive(true);
            StartCoroutine(FadeInRoutine());
        }
    }

    void Update()
    {
        GameObject current = EventSystem.current.currentSelectedGameObject;
        if (current != lastSelected && current != null)
        {
            PlayMove();
            lastSelected = current;
        }

        if (controlsImage != null && controlsImage.activeSelf)
        {
            if (Time.unscaledTime - lastOpenedTime < 0.2f) return;

            if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
            {
                HideControls();
            }
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (settingsPanel.activeSelf) CloseSettings();
            else if (!pauseMenu.activeSelf) OpenMenu();
            else ResumeGame();
        }
    }

    public void OpenMenu()
    {
        PlayConfirmSound();
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;

        playerMovement.BlockControl();
        playerMovement.playerInput.SwitchCurrentActionMap("UI");

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(SelectObject(firstPauseButton));
    }

    public void ResumeGame()
    {
        PlayConfirmSound();
        pauseMenu.SetActive(false);
        settingsPanel.SetActive(false);
        if (controlsImage != null) controlsImage.SetActive(false);

        Time.timeScale = 1f;
        playerMovement.UnblockControl();
        playerMovement.playerInput.SwitchCurrentActionMap("Player");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        EventSystem.current.SetSelectedGameObject(null);
    }
    private IEnumerator FadeOutRoutine(string sceneName)
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
        PlayConfirmSound();

        buttonBeforeWindow = EventSystem.current.currentSelectedGameObject;

        pauseMenu.SetActive(false);
        settingsPanel.SetActive(true);
        StartCoroutine(SelectObject(firstSettingsButton));
    }
    public void OpenWarning()
    {
        PlayConfirmSound();

        buttonBeforeWindow = EventSystem.current.currentSelectedGameObject;

        pauseMenu.SetActive(false);
        warningPanel.SetActive(true);
        StartCoroutine(SelectObject(firstWarningButton));
    }

    public void CloseSettings()
    {
        PlayConfirmSound();
        settingsPanel.SetActive(false);
        pauseMenu.SetActive(true);

        if (buttonBeforeWindow != null)
        {
            StartCoroutine(SelectObject(buttonBeforeWindow));
        }
        else
        {
            StartCoroutine(SelectObject(firstPauseButton));
        }
    }
    public void CloseWarning()
    {
        PlayConfirmSound();
        warningPanel.SetActive(false);
        pauseMenu.SetActive(true);

        if (buttonBeforeWindow != null)
        {
            StartCoroutine(SelectObject(buttonBeforeWindow));
        }
        else
        {
            StartCoroutine(SelectObject(firstPauseButton));
        }
    }
    public void ShowControls()
    {
        if (controlsImage == null || controlsImage.activeSelf) return;

        PlayConfirm();

        buttonBeforeWindow = EventSystem.current.currentSelectedGameObject;

        controlsImage.SetActive(true);
        controlsImage.transform.SetAsLastSibling();

        lastOpenedTime = Time.unscaledTime;
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void HideControls()
    {
        if (controlsImage == null) return;

        PlayConfirm();
        controlsImage.SetActive(false);

        if (buttonBeforeWindow != null)
        {
            StartCoroutine(SelectObject(buttonBeforeWindow));
        }
        else
        {
            StartCoroutine(SelectObject(firstPauseButton));
        }
    }

    public void Home()
    {
        PlayConfirm();
        Time.timeScale = 1f;
        StartCoroutine(FadeOutRoutine("StartScene"));
    }
    public void Subtitles()
    {
        Time.timeScale = 1f; 
        StartCoroutine(FadeOutRoutine("Credits"));
    }

    private void PlayConfirmSound()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.menuConfirm);
    }

    private IEnumerator SelectObject(GameObject target)
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(target);
    }
    private void PlayConfirm() { if (AudioManager.Instance) AudioManager.Instance.PlaySFX(AudioManager.Instance.menuConfirm); }
    private void PlayMove() { if (AudioManager.Instance) AudioManager.Instance.PlaySFX(AudioManager.Instance.menuMove); }
}
