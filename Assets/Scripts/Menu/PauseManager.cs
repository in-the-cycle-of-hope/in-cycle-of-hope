using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] PlayerMovement playerMovement;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0f;
            playerMovement.isControlBlocked = true;
            playerMovement.moveInput = Vector2.zero;
            playerMovement.playerInput.enabled = false;
            playerMovement.horizontalMovement = 0f;
        }
    }
}
