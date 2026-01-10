using UnityEngine;

public class PlayerAudioEvents : MonoBehaviour
{
    [Header("Steps")]
    public AudioClip[] steps;

    [Header("Actions")]
    public AudioClip jump;
    public AudioClip dash;
    public AudioClip climb;
    public AudioClip death;

    public void PlayStep()
    {
        if (steps == null || steps.Length == 0) return;

        var clip = steps[Random.Range(0, steps.Length)];
        AudioManager.Instance.PlaySFX(clip);
    }

    public void PlayJump()
    {
        AudioManager.Instance.PlaySFX(jump);
    }

    public void PlayDash()
    {
        AudioManager.Instance.PlaySFX(dash);
    }

    public void PlayClimb()
    {
        AudioManager.Instance.PlaySFX(climb);
    }

    public void PlayDeath()
    {
        AudioManager.Instance.PlaySFX(death);
    }
}
