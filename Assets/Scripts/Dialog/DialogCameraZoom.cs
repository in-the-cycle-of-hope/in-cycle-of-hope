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
        blackScreenObj1.SetActive(true);
        blackScreen1.SetTrigger("BlackScreen1");
        yield return new WaitForSeconds(0.3f);

        cam.Lens.OrthographicSize = targetSize;

        blackScreenObj2.SetActive(true);
        blackScreen2.SetTrigger("BlackScreen2");
        yield return new WaitForSeconds(0.3f);

        blackScreenObj1.SetActive(false);
        blackScreenObj2.SetActive(false);
    }
}
