using System.Collections;
using TMPro;
using UnityEngine;
public enum TutorialStep
{
    Move,
    Jump,
    Dash,
    WallGrab,
    Done
}
public class TutorialManager : MonoBehaviour
{
    public PlayerMovement player;
    public TMP_Text tutorialText;

    private TutorialStep step = TutorialStep.Move;

    [Header("UI Fade Settings")]
    public CanvasGroup tutorialGroup;
    public float showDelay = 3f;
    public float fadeDuration = 0.5f;

    private bool isStepActive = false;

    void Start()
    {
        ApplyStep();
    }

    void Update()
    {
        if (!isStepActive) return;
        switch (step)
        {
            case TutorialStep.Move:
                if (Mathf.Abs(player.horizontalMovement) > 0.1f)
                    NextStep();
                break;

            case TutorialStep.Jump:
                if (!player.isGrounded)
                    NextStep();
                break;

            case TutorialStep.Dash:
                if (player.isDashing)
                    NextStep();
                break;

            case TutorialStep.WallGrab:
                if (player.isGrabbingWall && Mathf.Abs(player.moveInput.y) > 0.2f)
                    NextStep();
                break;
            case TutorialStep.Done:
                tutorialText.text = "";
                player.canMove = true;
                player.canJump = true;
                player.canDash = true;
                player.canGrabWall = true;
                break;
        }
    }

    void NextStep()
    {
        step++;
        ApplyStep();
    }

    void ApplyStep()
    {
        StopAllCoroutines();
        StartCoroutine(ShowTutorialWithDelay());
    }
    IEnumerator ShowTutorialWithDelay()
    {
        isStepActive = false;

        yield return StartCoroutine(Fade(0f));
        yield return new WaitForSeconds(showDelay);

        ApplyAbilitiesAndText();

        yield return StartCoroutine(Fade(1f));

        isStepActive = true;
    }

    void ApplyAbilitiesAndText()
    {
        // unlock усе спочатку
        player.canMove = true;
        player.canJump = true;
        player.canDash = true;
        player.canGrabWall = true;

        switch (step)
        {
            case TutorialStep.Move:
                tutorialText.text = "Натисніть A / D щоб рухатись";
                player.canJump = false;
                player.canDash = false;
                player.canGrabWall = false;
                break;

            case TutorialStep.Jump:
                tutorialText.text = "Натисніть SPACE щоб стрибнути";
                player.canDash = false;
                player.canGrabWall = false;
                break;

            case TutorialStep.Dash:
                tutorialText.text = "Натисніть SHIFT щоб зробити дешь";
                player.canGrabWall = false;
                break;

            case TutorialStep.WallGrab:
                tutorialText.text = "Затисніть кнопку захвату та рухайтесь вгору / вниз";
                break;

            case TutorialStep.Done:
                tutorialText.text = "";
                break;
        }
    }
    IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = tutorialGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            tutorialGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }

        tutorialGroup.alpha = targetAlpha;
    }
}
