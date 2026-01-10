using Fungus;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogTrigger : MonoBehaviour
{
    public Flowchart flowchart;
    public string blockName;

    [Header("Settings")]
    public bool isOneTimeOnly = true; // Додаємо цей перемикач

    [Header("Save (Only for One Time dialogues)")]
    public string uniqueId;

    private bool hasTriggered = false;
    private Collider2D col;

    private string PrefKey => $"DialogTriggered_{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}_{uniqueId}";

    private void Awake() => col = GetComponent<Collider2D>();

    private void Start()
    {
        // Якщо діалог одноразовий — перевіряємо, чи він вже був
        if (isOneTimeOnly)
        {
            hasTriggered = PlayerPrefs.GetInt(PrefKey, 0) == 1;
            if (hasTriggered && col != null) col.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isOneTimeOnly && hasTriggered) return;
        if (!collision.CompareTag("Player")) return;

        if (flowchart == null || string.IsNullOrEmpty(blockName)) return;

        // 🔽 НОВЕ: повідомляємо гравця, що почався діалог
        PlayerMovement player = collision.GetComponent<PlayerMovement>();
        if (player != null)
        {
            player.EnterDialogue();
        }

        flowchart.ExecuteBlock(blockName);

        if (isOneTimeOnly)
        {
            hasTriggered = true;
            PlayerPrefs.SetInt(PrefKey, 1);
            PlayerPrefs.Save();
            if (col != null) col.enabled = false;
        }
    }
}
