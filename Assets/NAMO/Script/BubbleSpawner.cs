using System.Collections;
using UnityEngine;

public class BubbleSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private BoxCollider2D mapBoundsCollider;

    [Header("Super Fast Spawn Settings")]
    [Tooltip("ระยะห่างการสปอว์นเริ่มต้น (วินาที)")]
    [SerializeField] private float startSpawnInterval = 0.15f;
    [Tooltip("ระยะห่างการสปอว์นที่เร็วที่สุด (วินาที)")]
    [SerializeField] private float minSpawnInterval = 0.03f;
    [SerializeField] private float rampUpDuration = 25f;

    [Header("Multi-Spawn & Pattern")]
    [Tooltip("จำนวนฟองสบู่ที่ออกมาพร้อมกันต่อ 1 รอบ")]
    [SerializeField] private int bubblesPerSpawn = 3;

    [SerializeField] private float waveInterval = 6f; // พายุสปอว์นถี่ขึ้นทุก 6 วิ
    [SerializeField] private int waveCount = 20;

    private float timer = 0f;
    private float waveTimer = 0f;
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
        waveTimer += Time.deltaTime;

        // คำนวณความถี่ให้เร็วขึ้นเรื่อยๆ ตามเวลา
        float progress = Mathf.Clamp01(gameTime / rampUpDuration);
        float currentInterval = Mathf.Lerp(startSpawnInterval, minSpawnInterval, progress);

        // สปอว์นถล่มถี่ๆ
        if (timer >= currentInterval)
        {
            timer = 0f;
            // สปอว์นทีละหลายลูกพร้อมกันให้ทั่วแมพ
            for (int i = 0; i < bubblesPerSpawn; i++)
            {
                SpawnBubbleAcrossMap();
            }
        }

        // Event พายุฟองสบู่ระเบิดทั่วแมพ
        if (waveTimer >= waveInterval)
        {
            waveTimer = 0f;
            StartCoroutine(SpawnFullMapShowerWave());
        }
    }

    // สปอว์นฟองสบู่ให้ตกกระจายทั่วแมพ
    private void SpawnBubbleAcrossMap()
    {
        Bounds bounds = mapBoundsCollider.bounds;

        // 1. สุ่มพิกัด X แบบสุ่มเต็มความกว้างของแมพ (กระจายทั่วซ้าย-ขวา)
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        // สุ่มจุดเกิดเหนือขอบบนแมพเล็กน้อย
        Vector2 spawnPos = new Vector2(randomX, bounds.max.y + Random.Range(1f, 3f));

        // 2. ให้ทิศทางพุ่งลงด้านล่าง + เอียงสุ่มเบาๆ เพื่อให้วิถีดูเป็นธรรมชาติ
        Vector2 direction = new Vector2(Random.Range(-0.3f, 0.3f), -1f).normalized;

        // 3. สุ่มโอกาสให้บางลูกพุ่งตรงหา Player เพื่อเพิ่มความกดดัน
        if (Random.value < 0.25f && playerTransform != null)
        {
            // สุ่มเกิดจากขอบแมพฝั่งอื่นๆ ด้วย (ซ้าย/ขวา/ล่าง)
            spawnPos = GetRandomEdgePosition(bounds);
            direction = ((Vector2)playerTransform.position - spawnPos).normalized;
        }

        GameObject obj = Instantiate(bubblePrefab, spawnPos, Quaternion.identity);
        Bubble bubble = obj.GetComponent<Bubble>();

        if (bubble != null)
        {
            // สุ่มประเภทฟองสบู่
            float rand = Random.value;
            if (rand < 0.50f) bubble.type = Bubble.BubbleType.Normal;
            else if (rand < 0.75f) bubble.type = Bubble.BubbleType.Fast;
            else if (rand < 0.90f) bubble.type = Bubble.BubbleType.Spiral;
            else bubble.type = Bubble.BubbleType.Homing;

            float speedBonus = 1f + (gameTime / rampUpDuration) * 0.6f;
            bubble.Init(direction, speedBonus, playerTransform);
        }
    }

    // พายุฟองสบู่ตกลงมาจากฟ้าแบบ Rain Shower เต็มแมพ
    private IEnumerator SpawnFullMapShowerWave()
    {
        if (GameTutorialManager.Instance != null)
        {
            yield return GameTutorialManager.Instance.ShowBanner("An avalanche of facial cleansing foam!!");
        }

        yield return new WaitForSeconds(0.5f);

        Bounds bounds = mapBoundsCollider.bounds;

        // ปล่อยฟองสบู่เรียงแถวกระจายจากซ้ายไปขวาตกลงมาพร้อมกัน
        for (int i = 0; i < waveCount; i++)
        {
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            Vector2 spawnPos = new Vector2(randomX, bounds.max.y + 2f);
            Vector2 dir = new Vector2(0, -1f); // พุ่งลงตรงๆ ด้วยความเร็วสูง

            GameObject obj = Instantiate(bubblePrefab, spawnPos, Quaternion.identity);
            Bubble bubble = obj.GetComponent<Bubble>();
            if (bubble != null)
            {
                bubble.type = Bubble.BubbleType.Fast;
                bubble.Init(dir, 1.5f, playerTransform);
            }

            yield return new WaitForSeconds(0.04f); // ปล่อยรัวรวดเดียว!
        }

        yield return new WaitForSeconds(1.2f);
        if (GameTutorialManager.Instance != null) GameTutorialManager.Instance.HideBanner();
    }

    private Vector2 GetRandomEdgePosition(Bounds bounds)
    {
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