using Fungus;
using System.Collections;
using UnityEngine;

public class PlayerIntro : MonoBehaviour
{
    [Header("Movement")]
    public Transform startPoint;
    public Transform endPoint;
    public float moveDuration = 3f;
    public float animationEndDelay = 1f;

    [Header("References")]
    public Animator animator;

    private bool isPlaying;

    public void PlayIntro()
    {
        if (isPlaying) return;
        StartCoroutine(IntroRoutine());
    }

    private IEnumerator IntroRoutine()
    {
        isPlaying = true;

        transform.position = startPoint.position;

        // Граємо інтро-анімацію
        animator.Play("Intro", 0, 0f);

        // Чекаємо, поки кліп дограє
        float clipLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(clipLength);

        // Перемикаємося на Idle
        animator.Play("Idle", 0, 0f);

        // Дозволяємо гравцю контроль / запускаємо діалог
        // Тут виклик Fungus або будь-яка логіка

        isPlaying = false;
    }

}
