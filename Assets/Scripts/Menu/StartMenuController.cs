using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    [SerializeField] GameObject firstSelectedButton;

    void Start()
    {
        SelectFirstButton();
    }

    void OnEnable()
    {
        SelectFirstButton();
    }

    void SelectFirstButton()
    {
        if (EventSystem.current == null) return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

    public void OnStartClick()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void OnExitClick()
    {
        Application.Quit();
    }
}
