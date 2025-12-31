using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveCheckpoint : MonoBehaviour
{
    public int checkpointIndex;
    private static int lastShownCheckpointIndex = -1;

    private void OnEnable()
    {
        // Скидається при завантаженні сцени
        if (checkpointIndex == 0)
            lastShownCheckpointIndex = -1;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        SaveSystem.Save(
            other.transform.position,
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
            checkpointIndex
        );

        if (checkpointIndex > 0 && checkpointIndex > lastShownCheckpointIndex)
        {
            lastShownCheckpointIndex = checkpointIndex;
            SaveNotificationUI.Instance.Show();
        }
    }
}
