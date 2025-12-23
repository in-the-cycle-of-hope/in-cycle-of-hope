using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FallingPlatform : MonoBehaviour
{
    [Header("Timing")]
    public float delayBeforeFall = 2f;
    public float shakeDuration = 0.5f;
    public float shakeStrength = 0.08f;

    [Header("Sound")]
    public AudioClip crackSound;

    [Header("Camera Shake")]
    public CinemachineImpulseSource impulseSource;

    [Header("Respawn")]
    public bool respawnPlatform = true;
    public float respawnDelay = 5f;

    private Rigidbody2D rb;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private Coroutine fallCoroutine;
    private bool playerOnPlatform;
    private bool hasFallen;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        startPosition = transform.position;
        startRotation = transform.rotation;

        ResetPhysics();
    }

    // -------------------- COLLISION --------------------

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasFallen)
            return;

        if (collision.gameObject.CompareTag("Player"))
        {
            playerOnPlatform = true;

            if (fallCoroutine == null)
                fallCoroutine = StartCoroutine(FallRoutine());
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerOnPlatform = false;

            // ❗ гравець зійшов — скасовуємо падіння
            if (!hasFallen && fallCoroutine != null)
            {
                StopCoroutine(fallCoroutine);
                fallCoroutine = null;
            }
        }
    }

    // -------------------- FALL LOGIC --------------------

    private IEnumerator FallRoutine()
    {
        // чекаємо, ПОКИ гравець реально стоїть
        float timer = 0f;

        while (timer < delayBeforeFall)
        {
            if (!playerOnPlatform)
            {
                fallCoroutine = null;
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // 🔊 звук
        if (crackSound != null)
            AudioSource.PlayClipAtPoint(crackSound, transform.position);

        // 📷 камера-трус
        if (impulseSource != null)
            impulseSource.GenerateImpulse();

        // 😬 shake
        yield return StartCoroutine(Shake());

        // 💥 падіння
        hasFallen = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;

        if (respawnPlatform)
        {
            yield return new WaitForSeconds(respawnDelay);
            ResetPlatform();
        }
    }

    private IEnumerator Shake()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float offsetX = Random.Range(-shakeStrength, shakeStrength);
            transform.position = startPosition + new Vector3(offsetX, 0f, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = startPosition;
    }

    // -------------------- RESET --------------------

    private void ResetPlatform()
    {
        StopAllCoroutines();

        transform.position = startPosition;
        transform.rotation = startRotation;

        ResetPhysics();

        hasFallen = false;
        playerOnPlatform = false;
        fallCoroutine = null;
    }

    private void ResetPhysics()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        // 🔒 ЗАБОРОНА ОБЕРТАННЯ (ВАЖЛИВО ДЛЯ ПІКСЕЛЬ-АРТУ)
        rb.freezeRotation = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
}
