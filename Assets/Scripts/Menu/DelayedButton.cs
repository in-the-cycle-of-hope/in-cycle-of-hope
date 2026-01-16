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

    [Header("IAudio Sequence")]
    [SerializeField] private AudioClip[] introSounds;
    [SerializeField] private float pauseBetweenIntroSounds = 1.2f;
    [SerializeField] private AudioClip typingSound;

    [Header("Fading Systems")]
    [SerializeField] private Animator fadeAnimatorIn;
    [SerializeField] private Animator fadeAnimatorOut;
    [SerializeField] private GameObject blackScreenIn;
    [SerializeField] private GameObject blackScreenOut;

    [Header("Scene")]
    [SerializeField] private string startGameScene = "StartScene";

    private bool isBusy = false;

    private void Start()
    {
        Time.timeScale = 1f;
        titlesText.text = "";
        startButtonGroup.alpha = 0f;
        startButtonGroup.interactable = false;
        startButtonGroup.blocksRaycasts = false;
        startButton.interactable = false;

        startButton.onClick.AddListener(OnStartClicked);

        StartCoroutine(FadeInRoutine());
        StartCoroutine(SequenceWithSoundsRoutine());
    }

    private IEnumerator SequenceWithSoundsRoutine()
    {
        for (int i = 0; i < introSounds.Length; i++)
        {
            if (introSounds[i] != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(introSounds[i]);
            }
            yield return new WaitForSecondsRealtime(2f);

            if (i == 2)
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayMusic(AudioManager.Instance.subtitlesMusic);

                yield return new WaitForSecondsRealtime(4.0f);
                yield return StartCoroutine(FullSceneRoutine());
                yield break;
            }

            yield return new WaitForSecondsRealtime(pauseBetweenIntroSounds);
        }
    }

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

    private IEnumerator FullSceneRoutine()
    {
        yield return StartCoroutine(TypeTextRoutine());
        yield return new WaitForSecondsRealtime(buttonAppearDelay);

        float t = 0f;
        while (t < buttonFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            startButtonGroup.alpha = Mathf.Lerp(0f, 1f, t / buttonFadeDuration);
            yield return null;
        }

        startButtonGroup.alpha = 1f;
        startButtonGroup.interactable = true;
        startButtonGroup.blocksRaycasts = true;
        startButton.interactable = true;
        isBusy = false;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            yield return new WaitForEndOfFrame();
            EventSystem.current.SetSelectedGameObject(startButton.gameObject);
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.menuMove);
    }

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

    private void OnStartClicked()
    {
        if (isBusy) return;
        isBusy = true;

        StopAllCoroutines();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.menuConfirm);
            AudioManager.Instance.StopAllSounds();
        }

        SaveSystem.ClearSave();
        StartCoroutine(FadeOutAndLoad());
    }

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
