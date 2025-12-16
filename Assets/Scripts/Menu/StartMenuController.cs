using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnStartClick()
    {
        SceneManager.LoadScene("SampleScene");
    }
    public void OnExitClick()
    {
        UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }
}
