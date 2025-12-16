using UnityEngine;
using UnityEngine.UI;

public class DialogAvatar : MonoBehaviour
{
    public Image avatarImage;

    public Sprite heroHappy;
    public Sprite heroAngry;

    private void Awake()
    {
        SetAlpha(0f);
    }

    void SetAlpha(float a)
    {
        Color c = avatarImage.color;
        c.a = a;
        avatarImage.color = c;
    }

    public void ShowHeroHappy()
    {
        avatarImage.sprite = heroHappy;
        avatarImage.enabled = true;
        SetAlpha(1f);
    }

    public void ShowHeroAngry()
    {
        avatarImage.sprite = heroAngry;
        avatarImage.enabled = true;
        SetAlpha(1f);
    }

    public void HideAvatar()
    {
        avatarImage.enabled = false;
        SetAlpha(0f);
    }
}
