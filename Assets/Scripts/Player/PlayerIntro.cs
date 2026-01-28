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
        PlayerMovement mov = GetComponent<PlayerMovement>();

        mov.isIntroPlaying = true;
        mov.isControlBlocked = true;

        animator.Play("StartAnim", 0, 0f);
        yield return null;

        float moveTime = 1.5f;
        float elapsed = 0f;
        while (elapsed < moveTime)
        {
            transform.position = Vector3.Lerp(startPoint.position, endPoint.position, elapsed / moveTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(2f);

        mov.isIntroPlaying = false;
        mov.isInDialogue = true;

        isPlaying = false;
    }
}
