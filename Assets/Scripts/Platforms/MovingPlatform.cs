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

    private void Start()
    {
        nextPosition = pointA.position;
    }

    private void Update()
    {
        if (!CameraManager.IsActiveCamera(requiredCamera))
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            nextPosition,
            moveSpeed * Time.deltaTime
        );

        if (transform.position == nextPosition)
        {
            nextPosition = (nextPosition == pointA.position)
                ? pointB.position
                : pointA.position;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.parent = transform;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.parent = null;
        }
    }
}
