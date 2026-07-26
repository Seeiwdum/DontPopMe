using System.Collections;
using UnityEngine;

public class PlayerSkillController : MonoBehaviour
{
    [Header("Skill Settings")]
    [SerializeField] private float maxSkillCharge = 100f;     // หลอดเกจชาร์จเต็ม
    [SerializeField] private float chargePerSecond = 20f;     // ความเร็วในการชาร์จต่อวินาที
    [SerializeField] private float stealthDuration = 3f;      // ระยะเวลาซ่อนตัวใต้ผิวหน้า (3 วินาที)
    [SerializeField] private float shieldDuration = 1f;       // ระยะเวลาโล่ป้องกันหลังโผล่ขึ้นมา (1 วินาที)
    private float currentCharge = 0f;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private string burrowAnimTrigger = "BurrowDown";
    [SerializeField] private float animWaitTime = 0.3f;

    [Header("Target Indicator Settings")]
    [SerializeField] private GameObject warpIndicatorPrefab;
    private GameObject currentIndicator;

    [Header("Components")]
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private Collider2D playerCollider;

    // State Statuses
    private bool isBurrowed = false;
    private bool isInvincible = false;
    private Vector2 warpTargetPosition;

    // Getters สำหรับสคริปต์อื่นมาอ่านค่า
    public bool IsBurrowed => isBurrowed;
    public bool IsInvincible => isInvincible;

    private void Start()
    {
        currentCharge = maxSkillCharge;

        if (animator == null) animator = GetComponent<Animator>();
        if (playerSprite == null) playerSprite = GetComponent<SpriteRenderer>();
        if (playerCollider == null) playerCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        // 1. ชาร์จเกจเมื่อไม่ได้มุดอยู่
        if (!isBurrowed && currentCharge < maxSkillCharge)
        {
            currentCharge += chargePerSecond * Time.deltaTime;
            currentCharge = Mathf.Min(currentCharge, maxSkillCharge);
        }

        // 2. กดใช้สกิล
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Alpha1) || Input.GetMouseButtonDown(1))
        {
            if (currentCharge >= maxSkillCharge && !isBurrowed)
            {
                StartCoroutine(ExecuteFatBurrowSkill());
            }
        }

        // 3. เลื่อน Indicator ตามเมาส์ขณะมุด
        if (isBurrowed && currentIndicator != null)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;
            currentIndicator.transform.position = mouseWorldPos;
            warpTargetPosition = mouseWorldPos;
        }
    }

    private IEnumerator ExecuteFatBurrowSkill()
    {
        isBurrowed = true;
        currentCharge = 0f;

        // สั่ง Animator เล่นท่ามุด
        if (animator != null && !string.IsNullOrEmpty(burrowAnimTrigger))
        {
            animator.SetTrigger(burrowAnimTrigger);
        }

        yield return new WaitForSeconds(animWaitTime);

        // ซ่อน Sprite และปิด Collider
        if (playerSprite != null) playerSprite.enabled = false;
        if (playerCollider != null) playerCollider.enabled = false;

        // สร้าง Indicator
        if (warpIndicatorPrefab != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            currentIndicator = Instantiate(warpIndicatorPrefab, mousePos, Quaternion.identity);
        }

        warpTargetPosition = transform.position;
        yield return new WaitForSeconds(stealthDuration);

        // ทำลาย Indicator
        if (currentIndicator != null)
        {
            Destroy(currentIndicator);
        }

        // ย้ายตำแหน่ง Player
        transform.position = warpTargetPosition;

        // แสดง Sprite กลับมา
        if (playerSprite != null) playerSprite.enabled = true;

        if (animator != null && !string.IsNullOrEmpty(burrowAnimTrigger))
        {
            animator.SetTrigger(burrowAnimTrigger);
        }

        yield return new WaitForSeconds(animWaitTime);

        if (playerCollider != null) playerCollider.enabled = true;
        isBurrowed = false;

        // เปิดโล่อก
        StartCoroutine(ActivateShieldRoutine());
    }

    private IEnumerator ActivateShieldRoutine()
    {
        isInvincible = true;

        if (playerSprite != null) playerSprite.color = new Color(0.3f, 0.8f, 1f, 0.7f);

        yield return new WaitForSeconds(shieldDuration);

        if (playerSprite != null) playerSprite.color = Color.white;
        isInvincible = false;
    }

    public float GetSkillChargeRatio()
    {
        return currentCharge / maxSkillCharge;
    }
}