using UnityEngine;
using TMPro;
public class FontChanger : MonoBehaviour
{
    public TMP_FontAsset newFont;

    [ContextMenu("Change All Fonts")]
    public void ChangeAllFonts()
    {
        // Знаходимо всі текстові об'єкти на сцені
        TextMeshProUGUI[] allText = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();

        foreach (TextMeshProUGUI text in allText)
        {
            text.font = newFont;
            // Оновлюємо відображення
            text.SetAllDirty();
        }

        Debug.Log($"Змінено шрифт для {allText.Length} об'єктів!");
    }
}
