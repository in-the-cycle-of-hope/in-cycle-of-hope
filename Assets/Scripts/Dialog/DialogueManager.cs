using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    private DialogAvatar avatar;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        avatar = FindObjectOfType<DialogAvatar>(true);
    }

    public void ShowHeroHappy()
    {
        if (avatar == null)
            avatar = FindObjectOfType<DialogAvatar>(true);

        avatar?.ShowHeroHappy();
    }

    public void ShowHeroAngry()
    {
        if (avatar == null)
            avatar = FindObjectOfType<DialogAvatar>(true);

        avatar?.ShowHeroAngry();
    }

    public void HideAvatar()
    {
        if (avatar == null)
            avatar = FindObjectOfType<DialogAvatar>(true);

        avatar?.HideAvatar();
    }
}
