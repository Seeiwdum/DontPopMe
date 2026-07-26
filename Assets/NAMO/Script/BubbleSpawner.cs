using System.Collections;
using UnityEngine;

public class BubbleSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private BoxCollider2D mapBoundsCollider;

    [Header("Extreme Spawn Rates")]
    [SerializeField] private float startSpawnInterval = 0.1f;  // ปล่อยรัวตั้งแต่เริ่ม (ทุก 0.1s)
    [SerializeField] private float minSpawnInterval = 0.02f;   // รัวสุดๆ ทุก 0.02s!
    [SerializeField] private float rampUpDuration = 20f;
    [SerializeField] private int bubblesPerBurst = 3;          // ออกมาพร้อมกันทีละ 3 ลูก

    [Header("Event Bullet Hell")]
    [SerializeField] private float eventCooldown = 7f;         // พายุเข้าบ่อยขึ้นทุก 7 วิ

    private float timer = 0f;
    private float eventTimer = 0f;
    private float gameTime = 0f;

    private void Awake()
    {
        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }
    }

    private void Update()
    {
        if (mapBoundsCollider == null || bubblePrefab == null) return;

        gameTime += Time.deltaTime;
        timer += Time.deltaTime;
        eventTimer += Time.deltaTime;

        float progress = Mathf.Clamp01(gameTime / rampUpDuration);
        float currentInterval = Mathf.Lerp(startSpawnInterval, minSpawnInterval, progress);

        if (timer >= currentInterval)
        {
            timer = 0f;
            for (int i = 0; i < bubblesPerBurst; i++)
            {
                SpawnHardcoreBubble();
            }
        }

        if (eventTimer >= eventCooldown)
        {
            eventTimer = 0f;
            // สุ่ม Event ความโหด
            if (Random.value < 0.5f)
                StartCoroutine(SpiralStormPattern());
            else
                StartCoroutine(CrossfireRainPattern());
        }
    }

    private void SpawnHardcoreBubble()
    {
        Bounds bounds = mapBoundsCollider.bounds;
        Vector2 spawnPos = GetRandomEdgePosition(bounds);

        // ดักทางหน้า Player สุ่มเบี่ยงทิศเล็กน้อย
        Vector2 targetPos = (playerTransform != null)
            ? (Vector2)playerTransform.position + (Vector2)Random.insideUnitSphere * 2f
            : (Vector2)bounds.center;

        Vector2 direction = (targetPos - spawnPos).normalized;

        GameObject obj = Instantiate(bubblePrefab, spawnPos, Quaternion.identity);
        Bubble bubble = obj.GetComponent<Bubble>();

        if (bubble != null)
        {
            float rand = Random.value;
            if (rand < 0.35f) bubble.type = Bubble.BubbleType.Normal;
            else if (rand < 0.60f) bubble.type = Bubble.BubbleType.Fast;
            else if (rand < 0.80f) bubble.type = Bubble.BubbleType.Sine;
            else if (rand < 0.92f) bubble.type = Bubble.BubbleType.Homing;
            else bubble.type = Bubble.BubbleType.MegaSplit;

            float speedBonus = 1f + (gameTime / rampUpDuration) * 0.8f;
            bubble.Init(direction, speedBonus, playerTransform, bubblePrefab);
        }
    }

    // Pattern 1: พายุควงสว่าน บีบเข้าหาศูนย์กลาง (Spiral Storm)
    private IEnumerator SpiralStormPattern()
    {
        if (GameTutorialManager.Instance != null)
            yield return GameTutorialManager.Instance.ShowBanner("⚠️ เตือนภัย: พายุควงสว่านถล่มผิว!!");

        yield return new WaitForSeconds(0.4f);

        Vector2 center = (playerTransform != null) ? (Vector2)playerTransform.position : Vector2.zero;
        float radius = 12f;
        int count = 24;

        for (int i = 0; i < count; i++)
        {
            float angle = i * (360f / count);
            Vector2 spawnPos = center + new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * radius;
            Vector2 dir = (center - spawnPos).normalized;

            GameObject obj = Instantiate(bubblePrefab, spawnPos, Quaternion.identity);
            Bubble bubble = obj.GetComponent<Bubble>();
            if (bubble != null)
            {
                bubble.type = Bubble.BubbleType.Fast;
                bubble.Init(dir, 1.4f, playerTransform);
            }
            yield return new WaitForSeconds(0.03f); // ปล่อยไล่ระดับเป็นสาย
        }

        yield return new WaitForSeconds(1f);
        if (GameTutorialManager.Instance != null) GameTutorialManager.Instance.HideBanner();
    }

    // Pattern 2: ห่าฝนไขว้ข้ามทิศทาง (Crossfire Rain)
    private IEnumerator CrossfireRainPattern()
    {
        if (GameTutorialManager.Instance != null)
            yield return GameTutorialManager.Instance.ShowBanner("⚠️ เตือนภัย: ห่าฝนกรดฟองสบู่!!");

        yield return new WaitForSeconds(0.4f);

        Bounds bounds = mapBoundsCollider.bounds;
        int lines = 15;

        for (int i = 0; i < lines; i++)
        {
            // ยิงจากขอบบนพุ่งลง และ ยิงจากขอบซ้ายพุ่งขวา (กากบาท)
            Vector2 topPos = new Vector2(Random.Range(bounds.min.x, bounds.max.x), bounds.max.y + 2f);
            Vector2 leftPos = new Vector2(bounds.min.x - 2f, Random.Range(bounds.min.y, bounds.max.y));

            GameObject b1 = Instantiate(bubblePrefab, topPos, Quaternion.identity);
            b1.GetComponent<Bubble>()?.Init(Vector2.down, 1.5f, playerTransform);

            GameObject b2 = Instantiate(bubblePrefab, leftPos, Quaternion.identity);
            b2.GetComponent<Bubble>()?.Init(Vector2.right, 1.5f, playerTransform);

            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(1f);
        if (GameTutorialManager.Instance != null) GameTutorialManager.Instance.HideBanner();
    }

    private Vector2 GetRandomEdgePosition(Bounds bounds)
    {
        float padding = 2.5f;
        int edge = Random.Range(0, 4);
        switch (edge)
        {
            case 0: return new Vector2(Random.Range(bounds.min.x, bounds.max.x), bounds.max.y + padding);
            case 1: return new Vector2(Random.Range(bounds.min.x, bounds.max.x), bounds.min.y - padding);
            case 2: return new Vector2(bounds.min.x - padding, Random.Range(bounds.min.y, bounds.max.y));
            default: return new Vector2(bounds.max.x + padding, Random.Range(bounds.min.y, bounds.max.y));
        }
    }
}