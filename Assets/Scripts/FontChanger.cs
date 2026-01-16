using UnityEngine;
using TMPro;
public class FontChanger : MonoBehaviour
{
    public TMP_FontAsset newFont;

    [ContextMenu("Change All Fonts")]
    public void ChangeAllFonts()
    {
        TextMeshProUGUI[] allText = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();

        foreach (TextMeshProUGUI text in allText)
        {
            text.font = newFont;
            text.SetAllDirty();
        }
    }
}
