using UnityEngine;
using UnityEngine.Tilemaps;

public class StarsController : MonoBehaviour
{
    [Header("References")]
    public Tilemap starsTilemap;
    public SkyTimeController skyController;

    [Header("Settings")]
    [Range(0f, 1f)] public float maxAlpha = 0.8f;

    void Update()
    {
        if (skyController == null || starsTilemap == null) return;

        // Беремо час зі скрипта неба
        float currentTime = skyController.timer % skyController.cycleDuration;
        float phaseDuration = skyController.cycleDuration / 4f;

        float alpha = 0f;

        // 1 фаза: Ніч -> Світанок (Зірки зникають)
        if (currentTime < phaseDuration)
        {
            float t = currentTime / phaseDuration;
            alpha = Mathf.Lerp(maxAlpha, 0f, t);
        }
        // 2 фаза: Світанок -> День (Зірок немає)
        else if (currentTime < phaseDuration * 2f)
        {
            alpha = 0f;
        }
        // 3 фаза: День -> Захід (Зірок немає)
        else if (currentTime < phaseDuration * 3f)
        {
            alpha = 0f;
        }
        // 4 фаза: Захід -> Ніч (Зірки з'являються)
        else
        {
            float t = (currentTime - phaseDuration * 3f) / phaseDuration;
            alpha = Mathf.Lerp(0f, maxAlpha, t);
        }

        // Застосовуємо прозорість
        Color c = starsTilemap.color;
        c.a = alpha;
        starsTilemap.color = c;
    }
}
