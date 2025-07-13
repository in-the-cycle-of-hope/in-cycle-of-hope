using UnityEngine;
using UnityEngine.Events;
public class TriggerZone : MonoBehaviour
{
    public Unity.Cinemachine.CinemachineCamera cameraToActivate;
    public string playerTag = "Player";
    public float switchDelay = 0.05f; // Затримка перед перемиканням

    private bool isSwitching = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(playerTag) && !isSwitching)
        {
            StartCoroutine(SwitchCameraDelayed());
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag(playerTag))
        {
            if (!CameraManager.IsActiveCamera(cameraToActivate))
            {
                CameraManager.SwitchCamera(cameraToActivate);
            }
        }
    }

    private System.Collections.IEnumerator SwitchCameraDelayed()
    {
        isSwitching = true;
        yield return new WaitForSeconds(switchDelay);

        CameraManager.SwitchCamera(cameraToActivate);

        // Трохи часу, щоб уникнути повторних викликів під час перетину зон
        yield return new WaitForSeconds(0.2f);
        isSwitching = false;
    }
}
