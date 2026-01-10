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
    [SerializeField] GameObject blackScreenStart; // Екран появи (Fade In)
    [SerializeField] Animator fadeAnimatorStart;
    [SerializeField] GameObject blackScreenExit; // Вкажіть blackScreen1 (затухання)
    [SerializeField] Animator fadeAnimatorExit;

    void Start()
    {
        Time.timeScale = 1f;
        if (blackScreenStart != null) StartCoroutine(FadeFromBlack());
    }

    void Update()
    {
        // 1. Звуки перемикання (залишаємо як було)
        GameObject current = EventSystem.current.currentSelectedGameObject;
        if (current != lastSelected && current != null)
        {
            PlayMove();
            lastSelected = current;
        }

        // 2. Логіка картинки управління
        if (controlsImage != null && controlsImage.activeSelf)
        {
            // ДОДАЄМО ПЕРЕВІРКУ ЧАСУ:
            // Якщо картинка відкрилася менше ніж 0.2 сек тому - ігноруємо закриття
            if (Time.unscaledTime - lastOpenedTime < 0.2f) return;

            if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
            {
                HideControls();
            }
            return;
        }

        // 3. Логіка виклику паузи (залишаємо як було)
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

    public void OpenSettings()
    {
        PlayConfirmSound();

        // Запам'ятовуємо кнопку "Settings"
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

        // Повертаємо фокус туди, де він був
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

        // Повертаємо фокус туди, де він був
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

        // Запам'ятовуємо, на якій кнопці ми стояли (це буде кнопка Controls)
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

        // Повертаємо фокус на кнопку, яка відкрила вікно
        if (buttonBeforeWindow != null)
        {
            StartCoroutine(SelectObject(buttonBeforeWindow));
        }
        else
        {
            // Якщо раптом щось пішло не так — повертаємо на першу
            StartCoroutine(SelectObject(firstPauseButton));
        }
    }

    public void Home()
    {
        PlayConfirm();
        Time.timeScale = 1f; // Обов'язково повертаємо час для анімацій
        StartCoroutine(HomeRoutine());
    }

    private IEnumerator HomeRoutine()
    {
        blackScreenExit.SetActive(true);
        fadeAnimatorExit.Play("FadeOut", -1, 0f); // Назва вашої анімації згасання
        yield return new WaitForSecondsRealtime(1f);
        SceneManager.LoadScene("StartScene");
    }
    public void Subtitles()
    {
        Debug.Log("Завантажую сцену титрів...");
        PlayConfirm();
        Time.timeScale = 1f; // Обов'язково повертаємо час для анімацій
        StartCoroutine(SubtitlesRoutine());
    }

    private IEnumerator SubtitlesRoutine()
    {
        Debug.Log("Починаю затухання...");
        blackScreenExit.SetActive(true);
        fadeAnimatorExit.Play("FadeOut", -1, 0f);

        // Використовуйте WaitForSeconds, якщо Time.timeScale = 1
        yield return new WaitForSecondsRealtime(1.1f);

        Debug.Log("Спроба завантажити сцену Subtitles зараз!");
        SceneManager.LoadScene("Subtitles");
    }

    // Допоміжний метод для звуку натискання
    private void PlayConfirmSound()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.menuConfirm);
    }

    private IEnumerator FadeFromBlack()
    {
        if (blackScreenStart != null)
        {
            blackScreenStart.SetActive(true);

            // Отримуємо CanvasGroup (додайте цей компонент на чорний екран в Unity!)
            CanvasGroup cg = blackScreenStart.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f; // Примусово робимо ЧОРНИМ в перший же мікросекунду

            yield return new WaitForEndOfFrame(); // Чекаємо, поки все ініціалізується

            fadeAnimatorStart.Play("BlackScreenOut", -1, 0f);

            yield return new WaitForSecondsRealtime(1.0f);
            blackScreenStart.SetActive(false);
        }
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
