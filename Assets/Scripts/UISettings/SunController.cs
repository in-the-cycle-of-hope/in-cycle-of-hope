using UnityEngine;

public class SunController : MonoBehaviour
{
    [Header("References")]
    public Transform sun;

    [Header("Positions")]
    public Transform sunrisePoint;
    public Transform sunsetPoint;
    public float maxHeight = 5f;

    [Header("Time")]
    public float cycleDuration = 300f;

    [Header("Fade")]
    public SpriteRenderer sunRenderer;

    float timer;

    void Update()
    {
        timer += Time.deltaTime;

        float normalizedTime = (timer % cycleDuration) / cycleDuration;

        UpdateSunPosition(normalizedTime);
        UpdateSunVisibility(normalizedTime);
    }

    void UpdateSunPosition(float t)
    {
        // Горизонтальний рух (від сходу до заходу)
        Vector3 horizontalPos = Vector3.Lerp(
            sunrisePoint.position,
            sunsetPoint.position,
            t
        );

        // Вертикальний рух по синусоїді
        float height = Mathf.Sin(t * Mathf.PI) * maxHeight;

        sun.position = new Vector3(
            horizontalPos.x,
            sunrisePoint.position.y + height,
            sun.position.z
        );
    }

    void UpdateSunVisibility(float t)
    {
        if (sunRenderer == null) return;

        float alpha = 0f;

        // СВІТАНОК (плавна поява)
        if (t >= 0.15f && t < 0.25f)
        {
            float localT = Mathf.InverseLerp(0.15f, 0.25f, t);
            alpha = Mathf.SmoothStep(0f, 1f, localT);
        }
        // ДЕНЬ (повністю видно)
        else if (t >= 0.25f && t <= 0.65f)
        {
            alpha = 1f;
        }
        // ЗАХІД (плавне зникання)
        else if (t > 0.65f && t <= 0.75f)
        {
            float localT = Mathf.InverseLerp(0.65f, 0.75f, t);
            alpha = Mathf.SmoothStep(1f, 0f, localT);
        }
        // НІЧ
        else
        {
            alpha = 0f;
        }

        Color c = sunRenderer.color;
        c.a = alpha;
        sunRenderer.color = c;
    }

}
