using UnityEngine;

public class SingleStarController : MonoBehaviour
{
    public SpriteRenderer starRenderer;
    public SkyTimeController skyController;

    [Range(0f, 1f)] public float maxAlpha = 1f;

    void Update()
    {
        if (skyController == null || starRenderer == null) return;

        float currentTime = skyController.timer % skyController.cycleDuration;
        float phaseDuration = skyController.cycleDuration / 4f;

        float alpha = 0f;

        if (currentTime < phaseDuration)
            alpha = Mathf.Lerp(maxAlpha, 0f, currentTime / phaseDuration);
        else if (currentTime >= phaseDuration * 3f)
            alpha = Mathf.Lerp(0f, maxAlpha, (currentTime - phaseDuration * 3f) / phaseDuration);
        else
            alpha = 0f;

        Color c = starRenderer.color;
        c.a = alpha;
        starRenderer.color = c;
    }
}
