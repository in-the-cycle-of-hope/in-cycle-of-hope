using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightToggleCycle : MonoBehaviour
{
    public float timeOn = 150f;
    public float timeOff = 150f;

    private Light2D light2D;

    void Start()
    {
        light2D = GetComponent<Light2D>();

        if (light2D != null)
        {
            StartCoroutine(LightCycle());
        }
        else
        {
            Debug.LogError("Light2D не знайдено на цьому об'єкті!");
        }
    }

    IEnumerator LightCycle()
    {
        while (true)
        {
            // Світло увімкнене
            light2D.enabled = true;
            yield return new WaitForSeconds(timeOn);

            // Світло вимкнене
            light2D.enabled = false;
            yield return new WaitForSeconds(timeOff);
        }
    }
}
