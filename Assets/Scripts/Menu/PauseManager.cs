using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] GameObject firstSelectedButton;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Pause();
        }
    }

    void Pause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;

        playerMovement.isControlBlocked = true;
        playerMovement.moveInput = Vector2.zero;
        playerMovement.playerInput.enabled = false;
        playerMovement.horizontalMovement = 0f;

        // ⭐ ГОЛОВНЕ
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }
}
