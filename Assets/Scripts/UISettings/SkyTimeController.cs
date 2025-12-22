using UnityEngine;
using UnityEngine.Tilemaps;

public class SkyTimeController : MonoBehaviour
{
    [Header("Renderers")]
    public SpriteRenderer spriteRenderer;

    [Header("Time settings")]
    public float cycleDuration = 300f;

    [Header("Colors")]
    public Color nightColor = new Color(0.15f, 0.15f, 0.25f);
    public Color sunriseColor = new Color(0.8f, 0.5f, 0.4f);
    public Color dayColor = Color.white;
    public Color sunsetColor = new Color(0.8f, 0.45f, 0.35f);

    float timer;

    void Update()
    {
        timer += Time.deltaTime;

        float phaseDuration = cycleDuration / 4f;
        float phaseTime = timer % cycleDuration;

        Color targetColor;

        if (phaseTime < phaseDuration)
        {
            float t = phaseTime / phaseDuration;
            targetColor = Color.Lerp(nightColor, sunriseColor, t);
        }
        else if (phaseTime < phaseDuration * 2f)
        {
            float t = (phaseTime - phaseDuration) / phaseDuration;
            targetColor = Color.Lerp(sunriseColor, dayColor, t);
        }
        else if (phaseTime < phaseDuration * 3f)
        {
            float t = (phaseTime - phaseDuration * 2f) / phaseDuration;
            targetColor = Color.Lerp(dayColor, sunsetColor, t);
        }
        else
        {
            float t = (phaseTime - phaseDuration * 3f) / phaseDuration;
            targetColor = Color.Lerp(sunsetColor, nightColor, t);
        }

        ApplyColor(targetColor);
    }

    void ApplyColor(Color c)
    {
        if (spriteRenderer != null)
            spriteRenderer.color = c;
    }
}
