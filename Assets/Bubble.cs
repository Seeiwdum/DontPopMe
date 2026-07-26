using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bubble : MonoBehaviour
{
    public enum BubbleType { Normal, Fast, Homing, Mega, Spiral }

    [Header("Type & Speed")]
    public BubbleType type = BubbleType.Normal;
    [SerializeField] private float baseSpeed = 8f;         // เพิ่มความเร็วตั้งต้นขึ้น
    [SerializeField] private float homingSteerSpeed = 5f;  // ความไวในการเลี้ยวตาม Player

    [Header("Visual Juice (Smooth Wobble)")]
    [SerializeField] private float wobbleSpeed = 6f;      // ความเร็วในการกระดุกกระดิก
    [SerializeField] private float wobbleAmount = 0.08f;   // ระยะการยืดหดของฟองสบู่

    private Rigidbody2D rb;
    private Transform playerTransform;
    private Vector2 currentDirection;
    private float currentSpeed;
    private Vector3 initialScale;
    private float randomOffset;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // เพื่อความเนียน ไม่ทะลุ
        initialScale = transform.localScale;
        randomOffset = Random.Range(0f, 100f);
    }

    public void Init(Vector2 direction, float speedMultiplier = 1f, Transform player = null)
    {
        currentDirection = direction.normalized;
        currentSpeed = baseSpeed * speedMultiplier;
        playerTransform = player;

        // ปรับคุณสมบัติตามประเภทฟองสบู่
        switch (type)
        {
            case BubbleType.Fast:
                currentSpeed *= 2.2f;
                transform.localScale = initialScale * 0.65f;
                break;
            case BubbleType.Mega:
                currentSpeed *= 0.7f;
                transform.localScale = initialScale * 2.5f;
                break;
            case BubbleType.Homing:
                currentSpeed *= 1.2f;
                break;
            case BubbleType.Spiral:
                currentSpeed *= 1.4f;
                break;
        }

        // กำหนดความเร็วฟิสิกส์ทันที (เนียนตากว่า Translate)
        rb.linearVelocity = currentDirection * currentSpeed;

        // ทำลายตัวเองหลังผ่านไป 8 วินาที
        Destroy(gameObject, 8f);
    }

    private void FixedUpdate()
    {
        // ระบบเลี้ยวติดตามแบบ Smooth
        if (type == BubbleType.Homing && playerTransform != null)
        {
            Vector2 targetDir = ((Vector2)playerTransform.position - rb.position).normalized;
            currentDirection = Vector2.Lerp(currentDirection, targetDir, Time.fixedDeltaTime * homingSteerSpeed).normalized;
            rb.linearVelocity = currentDirection * currentSpeed;
        }
        else if (type == BubbleType.Spiral)
        {
            // ยิงเบี่ยงโค้งเล็กน้อยเป็นวงคลื่น
            float sineAngle = Mathf.Sin(Time.time * 5f + randomOffset) * 1.5f;
            Vector2 spiralDir = Quaternion.Euler(0, 0, sineAngle * 30f) * currentDirection;
            rb.linearVelocity = spiralDir * currentSpeed;
        }
    }

    private void Update()
    {
        // เอฟเฟกต์ฟองสบู่กระพริบ/ยืดหด (Wobble Effect) เพิ่มความสมจริง
        float wave = Mathf.Sin(Time.time * wobbleSpeed + randomOffset) * wobbleAmount;
        transform.localScale = new Vector3(
            initialScale.x + wave,
            initialScale.y - wave,
            initialScale.z
        );
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
            }
            Destroy(gameObject);
        }
    }
}