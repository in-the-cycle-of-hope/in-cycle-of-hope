using Fungus;
using UnityEngine;

public class DialogTrigger : MonoBehaviour
{
    public Flowchart flowchart;
    public string blockName;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasTriggered) return;
        if (!collision.CompareTag("Player")) return;

        Debug.Log("DialogTrigger fired: " + blockName);

        flowchart.ExecuteBlock(blockName);

        hasTriggered = true;

        GetComponent<Collider2D>().enabled = false;
    }
}
