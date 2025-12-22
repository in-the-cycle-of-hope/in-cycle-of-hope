using UnityEngine;

public class SunFollowCamera : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;

    [Header("Horizontal range (screen space)")]
    public float xRange = 6f;

    [Header("Vertical movement")]
    public float minHeight = -3f;
    public float maxHeight = 4f;

    [Header("Time")]
    public float cycleDuration = 300f;

    private float timer;

    void LateUpdate()
    {
        timer += Time.deltaTime;

        float t = (timer % cycleDuration) / cycleDuration;

        // X Ч ф≥ксована позиц≥€ в≥дносно камери
        float xOffset = 0f;

        // Y Ч плавно вгору ≥ вниз
        float yOffset;

        if (t < 0.5f)
        {
            // п≥дйом
            yOffset = Mathf.Lerp(minHeight, maxHeight, t / 0.5f);
        }
        else
        {
            // спуск
            yOffset = Mathf.Lerp(maxHeight, minHeight, (t - 0.5f) / 0.5f);
        }

        transform.position = new Vector3(
            cameraTransform.position.x + xOffset,
            cameraTransform.position.y + yOffset,
            transform.position.z
        );
    }
}
