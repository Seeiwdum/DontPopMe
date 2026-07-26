using UnityEngine;

public class Bubble : MonoBehaviour
{
    public enum BubbleType { Normal, Fast, Homing, Sine }
    public BubbleType type = BubbleType.Normal;

    [SerializeField] private float moveSpeed = 5f;
    private Vector2 moveDirection;
    private Transform targetTransform;

    public void Init(Vector2 dir, float speedMultiplier = 1f, Transform target = null)
    {
        moveDirection = dir;
        moveSpeed *= speedMultiplier;
        targetTransform = target;

        Destroy(gameObject, 8f);
    }

    private void Update()
    {
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth player = collision.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(1);
            }
            Destroy(gameObject);
        }
    }
}