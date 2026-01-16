using System;
using UnityEngine;
using UnityEngine.UI;

public class DialogAvatar : MonoBehaviour
{
    public Image avatarImage;

    public Sprite heroNeutral;
    public Sprite heroUnsure;
    public Sprite heroShocked;
    public Sprite heroContent;
    public Sprite heroSuspisious;
    public Sprite heroAEnd;

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

    public void ShowHeroNeutral()
    {
        avatarImage.sprite = heroNeutral;
        avatarImage.enabled = true;
        SetAlpha(1f);
    }
    public void ShowHeroUnsure()
    {
        avatarImage.sprite = heroUnsure;
        avatarImage.enabled = true;
        SetAlpha(1f);
    }
    internal void ShowHeroShocked()
    {
        avatarImage.sprite = heroShocked;
        avatarImage.enabled = true;
        SetAlpha(1f);
    }

    internal void ShowHeroEnd()
    {
        avatarImage.sprite = heroAEnd;
        avatarImage.enabled = true;
        SetAlpha(1f);
    }

    public void HideAvatar()
    {
        avatarImage.enabled = false;
        SetAlpha(0f);
    }
}
