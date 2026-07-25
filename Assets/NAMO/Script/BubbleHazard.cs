using UnityEngine;
public class BubbleHazard : MonoBehaviour
{
    [Header("Bubble Settings")]
    [SerializeField] private float minSpeed = 2f;
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float stunDuration = 1f;
    [SerializeField] private float lifeTime = 10f;

    private float moveSpeed;
    private Vector2 moveDirection = Vector2.down;

    private void Start()
    {
        moveSpeed = Random.Range(minSpeed, maxSpeed);
        float randomScale = Random.Range(0.8f, 1.3f);
        transform.localScale = new Vector3(randomScale, randomScale, 1f);

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }

    public void SetDirection(Vector2 dir)
    {
        moveDirection = dir.normalized;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(stunDuration);
            }

            PopBubble();
        }
    }

    private void PopBubble()
    {
        Destroy(gameObject);
    }
}