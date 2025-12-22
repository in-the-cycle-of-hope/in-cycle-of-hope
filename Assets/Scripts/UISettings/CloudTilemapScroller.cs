using UnityEngine;

public class CloudTilemapScroller : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 0.5f;
    public Vector2 direction = Vector2.right;

    [Header("Loop settings")]
    public float tilemapWidth = 20f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        transform.position += (Vector3)(direction.normalized * speed * Time.deltaTime);

        if (direction.x > 0 && transform.position.x >= startPosition.x + tilemapWidth)
        {
            transform.position = startPosition;
        }
        else if (direction.x < 0 && transform.position.x <= startPosition.x - tilemapWidth)
        {
            transform.position = startPosition;
        }
    }
}
