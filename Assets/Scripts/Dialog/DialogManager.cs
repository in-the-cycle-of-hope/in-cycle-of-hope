using UnityEngine;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance;
    private DialogAvatar _avatar;

    private DialogAvatar Avatar
    {
        get
        {
            if (_avatar == null)
            {
                _avatar = Object.FindAnyObjectByType<DialogAvatar>(FindObjectsInactive.Include);
            }
            return _avatar;
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    public void ShowHeroNeutral() => Avatar?.ShowHeroNeutral();
    public void ShowHeroUnsure() => Avatar?.ShowHeroUnsure();
    public void ShowHeroShocked() => Avatar?.ShowHeroShocked();
    public void ShowHeroEnd() => Avatar?.ShowHeroEnd();
    public void HideAvatar() => Avatar?.HideAvatar();
}
