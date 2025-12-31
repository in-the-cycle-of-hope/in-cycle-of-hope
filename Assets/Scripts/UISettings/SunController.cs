using UnityEngine;

public class SunController : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer sunRenderer;

    [Header("Vertical movement (local space)")]
    public float minHeight = -3f;
    public float maxHeight = 4f;

    [Header("Time")]
    public float cycleDuration = 300f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        float t = (timer % cycleDuration) / cycleDuration;

        UpdatePosition(t);
        UpdateVisibility(t);
    }

    void UpdatePosition(float t)
    {
        float y;

        if (t < 0.5f)
            y = Mathf.Lerp(minHeight, maxHeight, t / 0.5f);
        else
            y = Mathf.Lerp(maxHeight, minHeight, (t - 0.5f) / 0.5f);

        transform.localPosition = new Vector3(
            transform.localPosition.x,
            y,
            transform.localPosition.z
        );
    }

    void UpdateVisibility(float t)
    {
        if (sunRenderer == null) return;

        float alpha = 0f;

        if (t >= 0.15f && t < 0.25f)
            alpha = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.15f, 0.25f, t));
        else if (t >= 0.25f && t <= 0.65f)
            alpha = 1f;
        else if (t > 0.65f && t <= 0.75f)
            alpha = Mathf.SmoothStep(1f, 0f, Mathf.InverseLerp(0.65f, 0.75f, t));

        Color c = sunRenderer.color;
        c.a = alpha;
        sunRenderer.color = c;
    }
}
