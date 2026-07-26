using UnityEngine;

public class Bubble : MonoBehaviour
{
    public enum BubbleType { Normal, Fast, Homing, Mega }

    [Header("Bubble Settings")]
    public BubbleType type = BubbleType.Normal;
    [SerializeField] private float baseSpeed = 4f;
    [SerializeField] private float rotateSpeed = 100f; // เลี้ยวตาม Player
    [SerializeField] private float lifetime = 10f;

    private Vector2 moveDirection;
    private Transform playerTransform;
    private float currentSpeed;
    private float homingTimer = 3f; // ฟองติดตามจะตาม Player แค่ 3 วินาทีแรก

    public void Init(Vector2 direction, float speedMultiplier = 1f, Transform player = null)
    {
        moveDirection = direction.normalized;
        currentSpeed = baseSpeed * speedMultiplier;
        playerTransform = player;

        // ปรับขนาด/ความเร็วตามประเภทฟอง
        switch (type)
        {
            case BubbleType.Fast:
                currentSpeed *= 1.8f;
                transform.localScale = Vector3.one * 0.7f;
                break;
            case BubbleType.Mega:
                currentSpeed *= 0.6f;
                transform.localScale = Vector3.one * 2.2f;
                break;
            case BubbleType.Homing:
                currentSpeed *= 1.1f;
                break;
        }

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // ถ้าเป็นฟองโฮมมิ่ง จะค่อยๆ เลี้ยวหัวเข้าหา Player
        if (type == BubbleType.Homing && playerTransform != null && homingTimer > 0)
        {
            homingTimer -= Time.deltaTime;
            Vector2 targetDir = (playerTransform.position - transform.position).normalized;
            moveDirection = Vector2.Lerp(moveDirection, targetDir, Time.deltaTime * 2.5f).normalized;
        }

        // เคลื่อนที่ไปตามทิศทาง
        transform.Translate(moveDirection * currentSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // เรียกใช้ฟังก์ชันโดนชนจาก Player (หรือ GameOver)
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
            }
            Destroy(gameObject); // ชนสิวแล้วฟองแตก
        }
    }
}