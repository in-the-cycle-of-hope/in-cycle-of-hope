using Fungus;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogTrigger : MonoBehaviour
{
    public Flowchart flowchart;
    public string blockName;

    [Header("Save")]
    [Tooltip("Унікальний ключ. Якщо пусто — згенерується автоматично.")]
    public string uniqueId;

    private bool hasTriggered = false;
    private Collider2D col;

    private string PrefKey
    {
        get
        {
            // Якщо ти не хочеш вручну задавати uniqueId — він згенерується стабільно
            // (важливо: НЕ змінюй ім'я сцени/об'єкта після релізу, інакше ключ зміниться)
            string id = string.IsNullOrEmpty(uniqueId)
                ? $"{gameObject.name}_{blockName}_{transform.position.x:F2}_{transform.position.y:F2}"
                : uniqueId;

            return $"DialogTriggered_{SceneManager.GetActiveScene().name}_{id}";
        }
    }

    private void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    private void Start()
    {
        // Якщо цей діалог уже показували — вимикаємо тригер
        hasTriggered = PlayerPrefs.GetInt(PrefKey, 0) == 1;
        if (hasTriggered && col != null) col.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasTriggered) return;
        if (!collision.CompareTag("Player")) return;

        if (flowchart == null || string.IsNullOrEmpty(blockName)) return;

        Debug.Log("DialogTrigger fired: " + blockName);

        flowchart.ExecuteBlock(blockName);

        hasTriggered = true;
        PlayerPrefs.SetInt(PrefKey, 1);
        PlayerPrefs.Save();

        if (col != null) col.enabled = false;
    }
}
