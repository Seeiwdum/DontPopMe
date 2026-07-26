using System.Collections;
using UnityEngine;

public class BubbleSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private BoxCollider2D mapBoundsCollider;

    [Header("Spawn Rate & Difficulty")]
    [SerializeField] private float initialSpawnInterval = 1.8f;
    [SerializeField] private float minimumSpawnInterval = 0.4f;
    [SerializeField] private float difficultyRampTime = 40f;

    [Header("Wave Attack")]
    [SerializeField] private float waveInterval = 12f;
    [SerializeField] private int waveBubbleCount = 8;

    private float timer = 0f;
    private float currentSpawnInterval;
    private float gameTime = 0f;
    private float waveTimer = 0f;

    private void Awake()
    {
        // ช่วยค้นหาอัตโนมัติถ้าลืมลากมาใส่ใน Inspector
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        if (mapBoundsCollider == null)
        {
            mapBoundsCollider = GetComponent<BoxCollider2D>();
        }
    }

    private void Start()
    {
        currentSpawnInterval = initialSpawnInterval;
    }

    private void Update()
    {
        // ป้องกัน Error หากยังไม่มี Map Bounds หรือ Prefab
        if (mapBoundsCollider == null || bubblePrefab == null) return;

        gameTime += Time.deltaTime;
        timer += Time.deltaTime;
        waveTimer += Time.deltaTime;

        float progress = Mathf.Clamp01(gameTime / difficultyRampTime);
        currentSpawnInterval = Mathf.Lerp(initialSpawnInterval, minimumSpawnInterval, progress);

        if (timer >= currentSpawnInterval)
        {
            timer = 0f;
            SpawnRandomBubble();
        }

        if (waveTimer >= waveInterval)
        {
            waveTimer = 0f;
            StartCoroutine(SpawnBubbleWave());
        }
    }

    private void SpawnRandomBubble()
    {
        if (mapBoundsCollider == null || bubblePrefab == null) return;

        Vector2 spawnPos = GetRandomSpawnPositionOutsideBounds();

        Vector2 targetPos = (playerTransform != null)
            ? (Vector2)playerTransform.position + Random.insideUnitCircle * 3f
            : (Vector2)mapBoundsCollider.bounds.center;

        Vector2 direction = (targetPos - spawnPos).normalized;

        GameObject obj = Instantiate(bubblePrefab, spawnPos, Quaternion.identity);
        Bubble bubble = obj.GetComponent<Bubble>();

        if (bubble != null)
        {
            float rand = Random.value;
            if (rand < 0.5f) bubble.type = Bubble.BubbleType.Normal;
            else if (rand < 0.75f) bubble.type = Bubble.BubbleType.Fast;
            else if (rand < 0.90f) bubble.type = Bubble.BubbleType.Homing;
            else bubble.type = Bubble.BubbleType.Mega;

            float speedMult = 1f + (gameTime / difficultyRampTime) * 0.5f;
            bubble.Init(direction, speedMult, playerTransform);
        }
    }

    private IEnumerator SpawnBubbleWave()
    {
        if (GameTutorialManager.Instance != null)
        {
            yield return GameTutorialManager.Instance.ShowBanner("⚠️ เตือนภัย: พายุฟองสบู่ล้างหน้ากำลังมา!!");
        }

        yield return new WaitForSeconds(1f);

        Vector2 center = (playerTransform != null) ? (Vector2)playerTransform.position : Vector2.zero;
        float radius = 12f;

        for (int i = 0; i < waveBubbleCount; i++)
        {
            float angle = i * (360f / waveBubbleCount);
            Vector2 spawnPos = center + new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * radius;
            Vector2 dir = (center - spawnPos).normalized;

            GameObject obj = Instantiate(bubblePrefab, spawnPos, Quaternion.identity);
            Bubble bubble = obj.GetComponent<Bubble>();
            if (bubble != null)
            {
                bubble.type = Bubble.BubbleType.Fast;
                bubble.Init(dir, 1.2f, playerTransform);
            }
        }

        yield return new WaitForSeconds(2f);
        if (GameTutorialManager.Instance != null) GameTutorialManager.Instance.HideBanner();
    }

    private Vector2 GetRandomSpawnPositionOutsideBounds()
    {
        Bounds bounds = mapBoundsCollider.bounds;
        float padding = 2f;

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