using UnityEngine;

public class GradientSkyController : MonoBehaviour
{
    [Header("Renderer")]
    public SpriteRenderer gradientRenderer;

    [Header("Sprites")]
    public Sprite sunriseSprite;
    public Sprite sunsetSprite;
    public Sprite nightSprite;

    [Header("Time settings")]
    public float cycleDuration = 300f;

    [Header("Max alpha (keep small)")]
    [Range(0f, 0.25f)] public float sunriseMaxAlpha = 0.15f;
    [Range(0f, 0.25f)] public float sunsetMaxAlpha = 0.20f;
    [Range(0f, 0.25f)] public float nightAlpha = 0.10f;

    [Header("Colors")]
    public Color sunriseColor = new Color(1f, 0.82f, 0.60f, 1f);
    public Color sunsetColor = new Color(1f, 0.62f, 0.55f, 1f);
    public Color nightColor = new Color(0.45f, 0.55f, 1f, 1f);

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        float phaseDuration = cycleDuration / 4f;
        float tCycle = timer % cycleDuration;

        // 0: Night -> Sunrise (night fades out)
        if (tCycle < phaseDuration)
        {
            float t = tCycle / phaseDuration;
            SetSprite(nightSprite);
            SetColor(nightColor, Mathf.Lerp(nightAlpha, 0f, t));
        }
        // 1: Sunrise (fade in + fade out)
        else if (tCycle < phaseDuration * 2f)
        {
            float t = (tCycle - phaseDuration) / phaseDuration;
            SetSprite(sunriseSprite);

            float alpha;
            if (t < 0.5f)
            {
                // Fade IN
                alpha = Mathf.Lerp(0f, sunriseMaxAlpha, t * 2f);
            }
            else
            {
                // Fade OUT
                alpha = Mathf.Lerp(sunriseMaxAlpha, 0f, (t - 0.5f) * 2f);
            }

            SetColor(sunriseColor, alpha);
        }
        // 2: Day (no gradient)
        else if (tCycle < phaseDuration * 3f)
        {
            gradientRenderer.sprite = null;
        }
        // 3: Sunset (fade in + fade out, then night)
        else
        {
            float t = (tCycle - phaseDuration * 3f) / phaseDuration;

            if (t < 0.7f)
            {
                // Sunset fade in/out
                SetSprite(sunsetSprite);

                float tt = t / 0.7f;
                float alpha;

                if (tt < 0.5f)
                {
                    alpha = Mathf.Lerp(0f, sunsetMaxAlpha, tt * 2f);
                }
                else
                {
                    alpha = Mathf.Lerp(sunsetMaxAlpha, 0f, (tt - 0.5f) * 2f);
                }

                SetColor(sunsetColor, alpha);
            }
            else
            {
                // Night fade in
                float tt = (t - 0.7f) / 0.3f;
                SetSprite(nightSprite);
                SetColor(nightColor, Mathf.Lerp(0f, nightAlpha, tt));
            }
        }
    }

    void SetSprite(Sprite s)
    {
        if (gradientRenderer.sprite != s)
            gradientRenderer.sprite = s;
    }

    void SetColor(Color baseColor, float alpha)
    {
        Color c = baseColor;
        c.a = alpha;
        gradientRenderer.color = c;
    }
}
