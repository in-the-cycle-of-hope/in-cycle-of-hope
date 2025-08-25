using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject kyControl;
    [SerializeField] PlayerMovement playerMovement;
    public void Pause()
    {
        kyControl.SetActive(!kyControl.activeSelf);
        Time.timeScale = 0f;
        playerMovement.isControlBlocked = true;
        playerMovement.moveInput = Vector2.zero;
        playerMovement.playerInput.enabled = false;
    }
    public void Home()
    {
        SceneManager.LoadScene("StartScene");
        Time.timeScale = 1f;
    }
    public void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f;
    }
}
