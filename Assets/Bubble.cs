using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bubble : MonoBehaviour
{
    public enum BubbleType { Normal, Fast, Homing, Sine, MegaSplit }

    [Header("Type & Base Settings")]
    public BubbleType type = BubbleType.Normal;
    [SerializeField] private float baseSpeed = 10f;

    [Header("Homing & Sine Settings")]
    [SerializeField] private float homingSteerSpeed = 8f;
    [SerializeField] private float sineFrequency = 5f;
    [SerializeField] private float sineMagnitude = 3f;

    [Header("Split Settings")]
    [SerializeField] private GameObject miniBubblePrefab; // Prefab ลูกเล็กสำหรับแตกตัว
    [SerializeField] private float splitDelay = 1.5f;

    private Rigidbody2D rb;
    private Transform playerTransform;
    private Vector2 currentDirection;
    private float currentSpeed;
    private Vector3 initialScale;

    private float spawnTime;
    private Vector2 moveVelocity;
    private bool isSplit = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // เพื่อความเนียน ไม่ทะลุ
        initialScale = transform.localScale;
        spawnTime = Time.time;
    }

    public void Init(Vector2 direction, float speedMultiplier = 1f, Transform player = null, GameObject miniPrefab = null)
    {
        currentDirection = direction.normalized;
        currentSpeed = baseSpeed * speedMultiplier;
        playerTransform = player;
        miniBubblePrefab = miniPrefab;

        switch (type)
        {
            case BubbleType.Fast:
                currentSpeed *= 2.5f;
                transform.localScale = initialScale * 0.7f;
                break;

            case BubbleType.MegaSplit:
                currentSpeed *= 0.6f;
                transform.localScale = initialScale * 2.2f;
                Invoke(nameof(SplitIntoMiniBubbles), splitDelay);
                break;

            case BubbleType.Homing:
                currentSpeed *= 1.1f;
                break;

            case BubbleType.Sine:
                currentSpeed *= 1.3f;
                break;
        }

        Destroy(gameObject, 8f);
    }

    private void FixedUpdate()
    {
        float aliveTime = Time.time - spawnTime;
        Vector2 targetVelocity = currentDirection * currentSpeed;

        // 1. ลอจิกการเคลื่อนที่ตามประเภท
        if (type == BubbleType.Homing && playerTransform != null)
        {
            Vector2 targetDir = ((Vector2)playerTransform.position - rb.position).normalized;
            currentDirection = Vector2.Lerp(currentDirection, targetDir, Time.fixedDeltaTime * homingSteerSpeed).normalized;
            targetVelocity = currentDirection * currentSpeed;
        }
        else if (type == BubbleType.Sine)
        {
            Vector2 perpendicular = new Vector2(-currentDirection.y, currentDirection.x);
            float sine = Mathf.Sin(aliveTime * sineFrequency) * sineMagnitude;
            targetVelocity = (currentDirection * currentSpeed) + (perpendicular * sine);
        }

        // 2. เคลื่อนที่ด้วย Physics MovePosition แบบเนียนระดับ 100 FPS
        Vector2 nextPos = rb.position + targetVelocity * Time.fixedDeltaTime;
        rb.MovePosition(nextPos);
    }

    private void Update()
    {
        // เอฟเฟกต์ยืดหด (Juice Animation) ตามจังหวะ
        float wave = Mathf.Sin((Time.time - spawnTime) * 8f) * 0.08f;
        transform.localScale = new Vector3(initialScale.x + wave, initialScale.y - wave, initialScale.z);
    }

    private void SplitIntoMiniBubbles()
    {
        if (isSplit) return;
        isSplit = true;

        // แตกออกเป็น 8 ลูก พุ่งกระจายเป็นวงกลม 360 องศา
        int count = 8;
        for (int i = 0; i < count; i++)
        {
            float angle = i * (360f / count);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            GameObject mini = Instantiate(miniBubblePrefab != null ? miniBubblePrefab : gameObject, transform.position, Quaternion.identity);
            Bubble b = mini.GetComponent<Bubble>();
            if (b != null)
            {
                b.type = BubbleType.Fast;
                b.Init(dir, 1.2f, playerTransform);
            }
        }

        Destroy(gameObject); // ทำลายลูกใหญ่ทิ้ง
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

            if (type == BubbleType.MegaSplit)
            {
                CancelInvoke(nameof(SplitIntoMiniBubbles));
            }

            Destroy(gameObject);
        }
    }
}