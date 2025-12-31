using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class DialogCameraZoom : MonoBehaviour
{
    [Header("Fade Animators")]
    public Animator blackScreen1;
    public Animator blackScreen2;
    public GameObject blackScreenObj1;
    public GameObject blackScreenObj2;

    [Header("Camera")]
    public CinemachineCamera cam;
    public float dialogSize = 6f;
    public float normalSize = 11.25f;

    public void EnterDialog()
    {
        StartCoroutine(DialogSequence(dialogSize));
    }

    public void ExitDialog()
    {
        StartCoroutine(DialogSequence(normalSize));
    }

    private IEnumerator DialogSequence(float targetSize)
    {
        // --- ЕТАП 1: ЗАТУХАННЯ (в чорний) ---
        if (blackScreenObj1 != null)
        {
            blackScreenObj1.SetActive(true);
            blackScreen1.Play("BlackScreenIn", -1, 0f);
        }

        yield return new WaitForSecondsRealtime(1f);

        // --- ЕТАП 2: ЗМІНА КАМЕРИ ---
        cam.Lens.OrthographicSize = targetSize;

        // Маленька пауза, щоб камера "осіла"
        yield return new WaitForSecondsRealtime(0.15f);

        if (blackScreenObj1 != null)
            blackScreenObj1.SetActive(false);

        // --- ЕТАП 3: ПРОЯСНЕННЯ (з чорного) ---
        if (blackScreenObj2 != null)
        {
            blackScreenObj2.SetActive(true);
            blackScreen2.Play("BlackScreenOut", -1, 0f);
        }

        yield return new WaitForSecondsRealtime(1f);

        if (blackScreenObj2 != null)
            blackScreenObj2.SetActive(false);
    }
}
