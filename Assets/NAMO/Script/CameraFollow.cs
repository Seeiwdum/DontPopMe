using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target & Smoothness")]
    [SerializeField] private Transform target; // ลาก Player มาใส่
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);

    [Header("Camera Map Bounds (ขอบเขตแมพ)")]
    [SerializeField] private float minX = -10f; // ขอบซ้ายสุด
    [SerializeField] private float maxX = 10f;  // ขอบขวาสุด
    [SerializeField] private float minY = -5f;  // ขอบล่างสุด
    [SerializeField] private float maxY = 5f;   // ขอบบนสุด

    private void LateUpdate()
    {
        if (target == null) return;

        // คำนวณตำแหน่งที่กล้องอยากจะไปตาม Player
        Vector3 desiredPosition = target.position + offset;

        // ล็อคพิกัด X และ Y ไม่ให้เกินขอบเขต min/max ที่ตั้งไว้
        float clampedX = Mathf.Clamp(desiredPosition.x, minX, maxX);
        float clampedY = Mathf.Clamp(desiredPosition.y, minY, maxY);

        Vector3 clampedPosition = new Vector3(clampedX, clampedY, desiredPosition.z);

        // เลื่อนกล้องไปตำแหน่งใหม่แบบนุ่มนวล (Smooth)
        transform.position = Vector3.Lerp(transform.position, clampedPosition, Time.deltaTime * smoothSpeed);
    }

    // วาดกรอบสี่เหลี่ยมในหน้า Scene เพื่อให้ปรับขอบเขตได้ง่ายขึ้น
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, 0f);
        Vector3 size = new Vector3(maxX - minX, maxY - minY, 1f);
        Gizmos.DrawWireCube(center, size);
    }
}