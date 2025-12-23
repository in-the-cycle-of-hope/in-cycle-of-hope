using UnityEngine;
using UnityEngine.EventSystems;

public class IgnoreMouseHover : MonoBehaviour
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(null);
    }
}
