using Unity.Cinemachine;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float moveSpeed = 2f;

    [Header("Camera dependency")]
    public CinemachineCamera requiredCamera;

    private Vector3 nextPosition;
    private Vector3 startPosition;

    private void Awake()
    {
        startPosition = pointA.position;
    }

    private void Start()
    {
        ResetPlatform();
    }

    private void Update()
    {
        if (!CameraManager.IsActiveCamera(requiredCamera)) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            nextPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, nextPosition) < 0.01f)
        {
            nextPosition = (nextPosition == pointA.position)
                ? pointB.position
                : pointA.position;
        }
    }

    public void ResetPlatform()
    {
        transform.position = startPosition;
        nextPosition = pointB.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}
