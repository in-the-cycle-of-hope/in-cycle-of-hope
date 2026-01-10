using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DelayedButton : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI titlesText;
    [SerializeField] private Button startButton;
    [SerializeField] private CanvasGroup startButtonGroup;

    [Header("Titles Settings")]
    [SerializeField, TextArea] private string fullText;
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float buttonAppearDelay = 1.0f;
    [SerializeField] private float buttonFadeDuration = 0.8f;

    [Header("Audio")]
    [SerializeField] private AudioClip typingSound;

    [Header("Fading Systems")]
    [SerializeField] private Animator fadeAnimatorIn;   // з чорного
    [SerializeField] private Animator fadeAnimatorOut;  // в чорний
    [SerializeField] private GameObject blackScreenIn;
    [SerializeField] private GameObject blackScreenOut;

    [Header("Scene")]
    [SerializeField] private string startGameScene = "StartScene";

    private bool isBusy = false;

    private void Start()
    {
        Time.timeScale = 1f;

        // Початковий стан UI
        titlesText.text = "";
        startButtonGroup.alpha = 0f;
        startButtonGroup.interactable = false;
        startButtonGroup.blocksRaycasts = false;
        startButton.interactable = false;

        startButton.onClick.AddListener(OnStartClicked);

        StartCoroutine(FadeInRoutine());
        StartCoroutine(FullSceneRoutine());
    }

    // =========================
    // FADE IN (з чорного)
    // =========================
    private IEnumerator FadeInRoutine()
    {
        if (blackScreenIn != null && fadeAnimatorIn != null)
        {
            blackScreenIn.SetActive(true);
            fadeAnimatorIn.SetTrigger("BlackScreen");
            yield return new WaitForSecondsRealtime(1f);
            blackScreenIn.SetActive(false);
        }
    }

    // =========================
    // MAIN FLOW
    // =========================
    private IEnumerator FullSceneRoutine()
    {
        // 1. Друк титрів
        yield return StartCoroutine(TypeTextRoutine());

        // 2. Затримка перед кнопкою
        yield return new WaitForSecondsRealtime(buttonAppearDelay);

        // 3. Fade кнопки
        float t = 0f;
        while (t < buttonFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            startButtonGroup.alpha = Mathf.Lerp(0f, 1f, t / buttonFadeDuration);
            yield return null;
        }

        // 4. Активація кнопки
        startButtonGroup.alpha = 1f;
        startButtonGroup.interactable = true;
        startButtonGroup.blocksRaycasts = true;
        startButton.interactable = true;
        isBusy = false;

        // 5. Фокус для Enter
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            yield return new WaitForEndOfFrame();
            EventSystem.current.SetSelectedGameObject(startButton.gameObject);
        }

        // 6. Звук появи
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.menuMove);
    }

    // =========================
    // TYPEWRITER EFFECT
    // =========================
    private IEnumerator TypeTextRoutine()
    {
        titlesText.text = "";

        foreach (char letter in fullText)
        {
            titlesText.text += letter;

            if (letter != ' ' && typingSound != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(typingSound);

            yield return new WaitForSecondsRealtime(typingSpeed);
        }
    }

    // =========================
    // START NEW GAME
    // =========================
    private void OnStartClicked()
    {
        if (isBusy) return;
        isBusy = true;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.menuConfirm);

        // 🔥 НОВА ГРА = чистий сейв
        SaveSystem.ClearSave();

        StartCoroutine(FadeOutAndLoad());
    }

    // =========================
    // FADE OUT + LOAD
    // =========================
    private IEnumerator FadeOutAndLoad()
    {
        if (blackScreenOut != null && fadeAnimatorOut != null)
        {
            blackScreenOut.SetActive(true);
            fadeAnimatorOut.SetTrigger("BlackScreen");
            yield return new WaitForSecondsRealtime(1f);
        }

        SceneManager.LoadScene(startGameScene);
    }
}
