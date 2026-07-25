using System.Collections;
using UnityEngine;

public class BubbleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private float minSpawnInterval = 0.6f;
    [SerializeField] private float maxSpawnInterval = 1.5f;
    [SerializeField] private float spawnXMin = -8f;
    [SerializeField] private float spawnXMax = 8f;
    [SerializeField] private float spawnY = 6f;

    private void OnEnable()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (enabled)
        {
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);

            if (bubblePrefab != null)
            {
                Vector3 spawnPos = new Vector3(Random.Range(spawnXMin, spawnXMax), spawnY, 0f);
                Instantiate(bubblePrefab, spawnPos, Quaternion.identity);
            }
        }
    }
}