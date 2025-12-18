using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightLightController : MonoBehaviour
{
    [Header("Light")]
    public Light2D globalLight;

    [Header("Time settings")]
    public float cycleDuration = 300f;

    [Header("Light Intensity")]
    public float nightIntensity = 0.25f;
    public float sunriseIntensity = 0.6f;
    public float dayIntensity = 1.1f;
    public float sunsetIntensity = 0.5f;

    float timer;

    void Update()
    {
        timer += Time.deltaTime;

        float phaseDuration = cycleDuration / 4f;
        float phaseTime = timer % cycleDuration;

        float targetIntensity;

        if (phaseTime < phaseDuration)
        {
            float t = phaseTime / phaseDuration;
            targetIntensity = Mathf.Lerp(nightIntensity, sunriseIntensity, t);
        }
        else if (phaseTime < phaseDuration * 2f)
        {
            float t = (phaseTime - phaseDuration) / phaseDuration;
            targetIntensity = Mathf.Lerp(sunriseIntensity, dayIntensity, t);
        }
        else if (phaseTime < phaseDuration * 3f)
        {
            float t = (phaseTime - phaseDuration * 2f) / phaseDuration;
            targetIntensity = Mathf.Lerp(dayIntensity, sunsetIntensity, t);
        }
        else
        {
            float t = (phaseTime - phaseDuration * 3f) / phaseDuration;
            targetIntensity = Mathf.Lerp(sunsetIntensity, nightIntensity, t);
        }

        globalLight.intensity = targetIntensity;
    }
}
