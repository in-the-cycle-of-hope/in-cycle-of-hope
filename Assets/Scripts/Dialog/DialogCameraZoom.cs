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

    [Header("Settings")]
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
        var activeCam = CinemachineCore.GetVirtualCamera(0);

        if (activeCam == null)
        {
            yield break;
        }

        // --- ≈“¿œ 1: «¿“”’¿ÕÕﬂ ---
        if (blackScreenObj1 != null)
        {
            blackScreenObj1.SetActive(true);
            blackScreen1.Play("BlackScreenIn", -1, 0f);
        }

        yield return new WaitForSecondsRealtime(1f);

        if (activeCam is CinemachineCamera vcam)
        {
            vcam.Lens.OrthographicSize = targetSize;
        }

        yield return new WaitForSecondsRealtime(0.15f);

        if (blackScreenObj1 != null)
            blackScreenObj1.SetActive(false);

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
