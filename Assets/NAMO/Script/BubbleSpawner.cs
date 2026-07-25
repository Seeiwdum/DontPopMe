using System.Collections;
using UnityEngine;

public class BubbleSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private float spawnInterval = 1.2f; // ความถี่ในการปล่อยฟองสบู่ (วินาที)
    [SerializeField] private bool isSpawning = true;

    [Header("Spawn Area Boundary")]
    [SerializeField] private float spawnXMin = -8f;
    [SerializeField] private float spawnXMax = 8f;
    [SerializeField] private float spawnYPosition = 6f; // ตำแหน่งเกิดเหนือขอบจอ

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (bubblePrefab != null)
            {
                float randomX = Random.Range(spawnXMin, spawnXMax);
                Vector3 spawnPos = new Vector3(randomX, spawnYPosition, 0f);

                GameObject bubble = Instantiate(bubblePrefab, spawnPos, Quaternion.identity);

                BubbleHazard bubbleScript = bubble.GetComponent<BubbleHazard>();
                if (bubbleScript != null)
                {
                    bubbleScript.SetDirection(Vector2.down);
                }
            }
        }
    }

    // วาดเส้นขอบเขตในหน้า Scene เพื่อให้มองเห็นจุดสปอน
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(spawnXMin, spawnYPosition, 0), new Vector3(spawnXMax, spawnYPosition, 0));
    }
}