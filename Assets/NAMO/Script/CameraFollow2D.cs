using System.Collections;
using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public static CameraFollow2D Instance { get; private set; }

    [Header("Target & Follow Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);

    [Header("Map Boundary")]
    [SerializeField] private BoxCollider2D currentMapCollider;
    [SerializeField] private bool useBounds = false; // เริ่มต้นตั้งเป็น false ไว้ก่อนได้ถ้าไม่ใช้ขอบโซนแรก

    private Camera cam;
    private float defaultOrthographicSize;
    private bool isTransitioning = false;

    private float mapMinX, mapMaxX, mapMinY, mapMaxY;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;

        defaultOrthographicSize = cam.orthographicSize;
    }

    private void Start()
    {
        if (currentMapCollider != null)
        {
            UpdateBoundsFromCollider(currentMapCollider);
        }

        if (target != null)
        {
            Vector3 targetPos = target.position + offset;
            if (useBounds && currentMapCollider != null)
            {
                targetPos = ClampPositionToBounds(targetPos, cam.orthographicSize);
            }
            transform.position = targetPos;
        }
    }

    private void LateUpdate()
    {
        // ถ้าอยู่ในช่วงเปลี่ยนผ่านซูมกล้อง หรือไม่มี Target ให้ข้าม
        if (isTransitioning || target == null || cam == null) return;

        Vector3 desiredPosition = target.position + offset;

        // ถ้าใช้ Bounds ค่อย Clamp
        if (useBounds)
        {
            desiredPosition = ClampPositionToBounds(desiredPosition, cam.orthographicSize);
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);
    }

    private Vector3 ClampPositionToBounds(Vector3 rawPosition, float cameraSize)
    {
        float camVertExtent = cameraSize;
        float camHorizExtent = cameraSize * cam.aspect;

        float minAllowedX = mapMinX + camHorizExtent;
        float maxAllowedX = mapMaxX - camHorizExtent;
        float minAllowedY = mapMinY + camVertExtent;
        float maxAllowedY = mapMaxY - camVertExtent;

        float clampedX = (minAllowedX > maxAllowedX) ? (mapMinX + mapMaxX) / 2f : Mathf.Clamp(rawPosition.x, minAllowedX, maxAllowedX);
        float clampedY = (minAllowedY > maxAllowedY) ? (mapMinY + mapMaxY) / 2f : Mathf.Clamp(rawPosition.y, minAllowedY, maxAllowedY);

        return new Vector3(clampedX, clampedY, rawPosition.z);
    }

    public void UpdateBoundsFromCollider(BoxCollider2D collider)
    {
        if (collider != null)
        {
            currentMapCollider = collider;
            Bounds bounds = collider.bounds;
            mapMinX = bounds.min.x;
            mapMaxX = bounds.max.x;
            mapMinY = bounds.min.y;
            mapMaxY = bounds.max.y;
            useBounds = true;
        }
        else
        {
            useBounds = false; // ถ้าไม่มี Collider ให้ปิดการล็อคขอบกล้อง
        }
    }

    // ฟังก์ชันสั่งปิดการจำกัดขอบเขตกล้อง (ให้กล้องตาม Player ได้อิสระทั้งแมพ)
    public void DisableBounds()
    {
        useBounds = false;
    }

    public void TriggerFaceMapOverview(BoxCollider2D fullMapCollider, float overviewCameraSize, float holdDuration, float transitionDuration)
    {
        StartCoroutine(FaceMapOverviewRoutine(fullMapCollider, overviewCameraSize, holdDuration, transitionDuration));
    }

    private IEnumerator FaceMapOverviewRoutine(BoxCollider2D fullMapCollider, float overviewSize, float holdTime, float transitionTime)
    {
        isTransitioning = true;

        // 1. อัปเดตขอบเขตกล้องเป็นแมพใหญ่ทันที
        if (fullMapCollider != null)
        {
            UpdateBoundsFromCollider(fullMapCollider);
        }
        else
        {
            useBounds = false;
        }

        Vector3 startPos = transform.position;
        Vector3 targetOverviewPos = (fullMapCollider != null)
            ? new Vector3(fullMapCollider.bounds.center.x, fullMapCollider.bounds.center.y, offset.z)
            : new Vector3(0, 0, offset.z);

        float startSize = cam.orthographicSize;

        // 2. แพนกล้องไปตรงกลาง + ซูมออก
        float timer = 0f;
        while (timer < transitionTime)
        {
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, timer / transitionTime);

            transform.position = Vector3.Lerp(startPos, targetOverviewPos, t);
            cam.orthographicSize = Mathf.Lerp(startSize, overviewSize, t);
            yield return null;
        }

        transform.position = targetOverviewPos;
        cam.orthographicSize = overviewSize;

        // 3. ค้างภาพไว้
        yield return new WaitForSeconds(holdTime);

        // 4. แพนกล้องกลับมาหา Player
        timer = 0f;
        while (timer < transitionTime)
        {
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, timer / transitionTime);

            float currentCamVert = Mathf.Lerp(overviewSize, defaultOrthographicSize, t);
            Vector3 rawPlayerPos = target.position + offset;

            // ถ้ามี Bounds ใช้ Clamp ถ้าไม่มีเลื่อนหา Player ตรงๆ
            Vector3 destination = useBounds ? ClampPositionToBounds(rawPlayerPos, currentCamVert) : rawPlayerPos;

            transform.position = Vector3.Lerp(targetOverviewPos, destination, t);
            cam.orthographicSize = Mathf.Lerp(overviewSize, defaultOrthographicSize, t);
            yield return null;
        }

        cam.orthographicSize = defaultOrthographicSize;

        // **สำคัญมาก**: คืนค่าให้กล้องติดตาม Player ตามปกติหลังซูมเสร็จ
        isTransitioning = false;
    }
}