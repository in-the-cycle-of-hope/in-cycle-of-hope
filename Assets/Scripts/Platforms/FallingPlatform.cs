using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FallingPlatform : MonoBehaviour
{
    [Header("Timing")]
    public float delayBeforeFall = 1.5f;
    public float shakeDuration = 0.5f;
    public float shakeStrength = 0.08f;
    public float fadeDuration = 0.8f;

    [Header("Respawn")]
    public bool respawnPlatform = true;
    public float respawnDelay = 3f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D platformCollider;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private bool isActivated;
    private bool hasFallen;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        platformCollider = GetComponent<Collider2D>();

        startPosition = transform.position;
        startRotation = transform.rotation;
        ResetPhysics();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Перевіряємо, чи це гравець і чи не активована вже платформа
        if (collision.gameObject.CompareTag("Player") && !isActivated && !hasFallen)
        {
            // Перевіряємо, чи гравець зверху (щоб не падала, якщо вдарився головою знизу)
            if (collision.contacts[0].normal.y < -0.5f)
            {
                isActivated = true;
                StartCoroutine(FallRoutine());
            }
        }
    }

    private IEnumerator FallRoutine()
    {
        // 1. Чекаємо затримку
        yield return new WaitForSeconds(delayBeforeFall);

        // 2. Звук
        if (AudioManager.Instance)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.crack);

        // 3. Тряска
        yield return StartCoroutine(Shake());

        // 4. Падіння
        hasFallen = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1.5f; // Трохи швидше падіння

        // 5. Плавне зникнення (одночасно з падінням)
        yield return StartCoroutine(Fade(1f, 0f));

        // 6. Вимикаємо фізику повністю
        platformCollider.enabled = false;
        rb.bodyType = RigidbodyType2D.Static;

        // 7. Респавн
        if (respawnPlatform)
        {
            yield return new WaitForSeconds(respawnDelay);

            // Повертаємо на місце
            transform.position = startPosition;
            transform.rotation = startRotation;
            ResetPhysics();

            // Плавна поява
            yield return StartCoroutine(Fade(0f, 1f));

            // Включаємо колайдер ТІЛЬКИ після того, як вона з'явилася (або на початку появи)
            platformCollider.enabled = true;
            isActivated = false;
            hasFallen = false;
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

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        Color color = spriteRenderer.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            spriteRenderer.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
        // Фіксуємо кінцеве значення
        spriteRenderer.color = new Color(color.r, color.g, color.b, endAlpha);
    }

    private void ResetPhysics()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
}
