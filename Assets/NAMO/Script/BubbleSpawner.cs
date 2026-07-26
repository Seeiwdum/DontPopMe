using System.Collections;
using UnityEngine;

public class BubbleSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private BoxCollider2D mapBoundsCollider;

    [Header("Spawn Settings")]
    [SerializeField] private float startSpawnInterval = 0.5f;
    [SerializeField] private float minSpawnInterval = 0.1f;
    [SerializeField] private float rampUpDuration = 30f;
    [SerializeField] private int bubblesPerBurst = 2;

    private float timer = 0f;
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

        float progress = Mathf.Clamp01(gameTime / rampUpDuration);
        float currentInterval = Mathf.Lerp(startSpawnInterval, minSpawnInterval, progress);

        if (timer >= currentInterval)
        {
            timer = 0f;
            for (int i = 0; i < bubblesPerBurst; i++)
            {
                SpawnBubble();
            }
        }
    }

    private void SpawnBubble()
    {
        Bounds bounds = mapBoundsCollider.bounds;
        Vector2 spawnPos = GetRandomEdgePosition(bounds);

        Vector2 targetPos = (playerTransform != null)
            ? (Vector2)playerTransform.position
            : (Vector2)bounds.center;

        Vector2 direction = (targetPos - spawnPos).normalized;

        GameObject obj = Instantiate(bubblePrefab, spawnPos, Quaternion.identity);
        Bubble bubble = obj.GetComponent<Bubble>();

        if (bubble != null)
        {
            bubble.Init(direction, 1f, playerTransform);
        }
    }

    private Vector2 GetRandomEdgePosition(Bounds bounds)
    {
        float padding = 1.5f;
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