using System.Collections;
using UnityEngine;

public class PlayerBurrowSkill : MonoBehaviour
{
    [Header("Burrow Settings")]
    [SerializeField] private float maxBurrowDuration = 5f;
    [SerializeField] private float shieldDuration = 1f;
    [SerializeField] private GameObject teleportMarkerPrefab;
    [SerializeField] private GameObject shieldVisual;

    [Header("Energy System")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float chargeRatePerSecond = 15f;
    private float currentEnergy = 0f;

    private bool isBurrowed = false;
    private bool hasShield = false;

    private Collider2D playerCollider;
    private SpriteRenderer playerSprite;
    private Animator animator;
    private GameObject currentMarker;
    private Camera mainCam;

    private void Awake()
    {
        playerCollider = GetComponent<Collider2D>();
        playerSprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        mainCam = Camera.main;
    }

    private void Update()
    {
        // สะสมหลอดไขมันเฉพาะตอนเข้า Main Game แล้วเท่านั้น
        if (GameTutorialManager.Instance != null && GameTutorialManager.Instance.CurrentState == GameTutorialManager.QuestState.MainGame)
        {
            if (!isBurrowed && currentEnergy < maxEnergy)
            {
                currentEnergy += chargeRatePerSecond * Time.deltaTime;
            }
        }

        // กดใช้สกิล
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Space))
        {
            // ช่วง Tutorial หรือเกจเต็มแล้ว ให้ใช้ได้
            bool isTutorial = GameTutorialManager.Instance != null && GameTutorialManager.Instance.CurrentState == GameTutorialManager.QuestState.TrySkills;

            if ((currentEnergy >= maxEnergy || isTutorial) && !isBurrowed)
            {
                StartCoroutine(BurrowRoutine());
            }
        }

        // ระหว่างมุด ให้ Marker เล็งตามตำแหน่ง Cursor เมาส์
        if (isBurrowed && currentMarker != null)
        {
            Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            currentMarker.transform.position = mousePos;
        }
    }

    private IEnumerator BurrowRoutine()
    {
        isBurrowed = true;
        currentEnergy = 0f; // รีเซ็ตเกจ

        // เล่น Animation มุดลงดิน (ถ้ามี)
        if (animator != null) animator.SetTrigger("BurrowDown");

        // ปิด Collision & ค่อยๆ จางตัวละครลง
        if (playerCollider != null) playerCollider.enabled = false;
        yield return StartCoroutine(FadePlayer(1f, 0.2f, 0.3f));

        // สร้าง Target Marker ติดตามเมาส์
        if (teleportMarkerPrefab != null)
        {
            currentMarker = Instantiate(teleportMarkerPrefab);
        }

        float timer = 0f;
        bool teleportedByClick = false;

        // วนลูปสแกนคำสั่งคลิกวาร์ป หรือรอจนครบ 5 วินาที
        while (timer < maxBurrowDuration)
        {
            timer += Time.deltaTime;

            if (Input.GetMouseButtonDown(0)) // Left Click โผล่ขึ้นมาทันที
            {
                teleportedByClick = true;
                break;
            }
            yield return null;
        }

        // ย้ายตำแหน่งถ้ากดคลิกวาร์ป
        if (currentMarker != null)
        {
            transform.position = currentMarker.transform.position;
            Destroy(currentMarker);
        }

        // โผล่ขึ้นมาจากผิว
        if (animator != null) animator.SetTrigger("BurrowUp");
        yield return StartCoroutine(FadePlayer(0.2f, 1f, 0.3f));

        if (playerCollider != null) playerCollider.enabled = true;
        isBurrowed = false;

        // เปิดใช้งาน Shield ป้องกัน 1 วินาที
        StartCoroutine(ShieldRoutine());
    }

    private IEnumerator ShieldRoutine()
    {
        hasShield = true;
        if (shieldVisual != null) shieldVisual.SetActive(true);

        yield return new WaitForSeconds(shieldDuration);

        hasShield = false;
        if (shieldVisual != null) shieldVisual.SetActive(false);
    }

    private IEnumerator FadePlayer(float startAlpha, float endAlpha, float duration)
    {
        float t = 0f;
        Color c = playerSprite.color;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, endAlpha, t / duration);
            playerSprite.color = c;
            yield return null;
        }
    }

    public bool IsProtected() => isBurrowed || hasShield;

    public void TriggerBurrowFromSkillSystem()
    {
        if (!isBurrowed)
        {
            StartCoroutine(BurrowRoutine());
        }
    }
}