using UnityEngine;

public class BubbleHazard : MonoBehaviour
{
    [Header("Movement & Wind Drift")]
    [SerializeField] private float minSpeed = 2f;
    [SerializeField] private float maxSpeed = 4f;
    [SerializeField] private float waveFrequency = 3f;  // ความถี่การพริ้วซ้ายขวา
    [SerializeField] private float waveAmplitude = 0.8f; // ความกว้างของการสะบัด
    [SerializeField] private float lifeTime = 10f;

    [Header("Effects")]
    [SerializeField] private GameObject popParticlePrefab; // Effect ฟองสบู่แตก

    private float moveSpeed;
    private float startX;
    private float timeAlive;

    private void Start()
    {
        moveSpeed = Random.Range(minSpeed, maxSpeed);
        startX = transform.position.x;

        float randomScale = Random.Range(0.8f, 1.3f);
        transform.localScale = new Vector3(randomScale, randomScale, 1f);

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        timeAlive += Time.deltaTime;

        // เคลื่อนที่ลงล่าง + โบกสะบัดแบบ Sin Wave (Smooth Wind Drift)
        float xOffset = Mathf.Sin(timeAlive * waveFrequency) * waveAmplitude;
        Vector3 newPos = transform.position + Vector3.down * moveSpeed * Time.deltaTime;
        newPos.x = startX + xOffset;

        transform.position = newPos;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerBurrowSkill skill = collision.GetComponent<PlayerBurrowSkill>();

            // ถ้าไม่ได้รับเกราะ Shield หรือมุดดินอยู่ ให้รับ Damage
            if (skill == null || !skill.IsProtected())
            {
                PlayerController player = collision.GetComponent<PlayerController>();
                if (player != null) player.TakeDamage(1f);
            }

            PopBubble();
        }
    }

    public void PopBubble()
    {
        if (popParticlePrefab != null)
        {
            Instantiate(popParticlePrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}