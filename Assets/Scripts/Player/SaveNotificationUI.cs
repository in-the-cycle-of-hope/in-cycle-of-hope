using System.Collections;
using UnityEngine;

public class SaveNotificationUI : MonoBehaviour
{
    public static SaveNotificationUI Instance;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeInTime = 0.25f;
    [SerializeField] private float stayTime = 1.2f;
    [SerializeField] private float fadeOutTime = 0.35f;
    Coroutine currentRoutine;

    private bool alreadyShown;

    public void ShowOnce()
    {
        if (alreadyShown) return;
        alreadyShown = true;
        Show();
    }

    void Awake()
    {
        Debug.Log("SaveNotificationUI Awake");
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        canvasGroup.alpha = 0f;
    }

    public void Show()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        // Fade In
        float t = 0f;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeInTime);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        yield return new WaitForSeconds(stayTime);

        // Fade Out
        t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeOutTime);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        currentRoutine = null;
    }
}
