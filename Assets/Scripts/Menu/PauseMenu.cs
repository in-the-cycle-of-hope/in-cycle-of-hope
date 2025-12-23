using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] PlayerMovement playerMovement;

    public void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;

        playerMovement.isControlBlocked = false;
        playerMovement.playerInput.enabled = true;

        EventSystem.current.SetSelectedGameObject(null);
    }

    public void Home()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartScene");
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
