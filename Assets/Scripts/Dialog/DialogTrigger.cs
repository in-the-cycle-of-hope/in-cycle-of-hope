using Fungus;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogTrigger : MonoBehaviour
{
    public Flowchart flowchart;
    public string blockName;

    [Header("Intro")]
    public bool isIntroTrigger = false;

    [Header("Settings")]
    public bool isOneTimeOnly = true;

    [Header("Save (Only for One Time dialogues)")]
    public string uniqueId;

    private bool hasTriggered = false;
    private Collider2D col;

    private string PrefKey => $"DialogTriggered_{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}_{uniqueId}";

    private void Awake() => col = GetComponent<Collider2D>();

    private void Start()
    {
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
