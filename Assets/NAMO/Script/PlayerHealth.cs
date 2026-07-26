using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("UI References")]
    [SerializeField] private Image healthBarFill;       // Image ของหลอดเลือด (ตั้งเป็น Image Type: Filled)
    [SerializeField] private Image damageFlashImage;    // Image สีแดงเต็มหน้าจอ หรือตัว UI สำหรับกระพริบแดง
    [SerializeField] private GameObject gameOverUI;     // Panel หน้าจอ Game Over

    [Header("Damage Effect Settings")]
    [SerializeField] private float flashDuration = 0.2f; // ระยะเวลากระพริบสีแดง
    [SerializeField] private Color flashColor = new Color(1f, 0f, 0f, 0.4f); // สีแดงโปร่งแสง

    private SpriteRenderer playerSprite;
    private bool isDead = false;
    private bool isInvulnerable = false; // กันโดนชนรวดเดียวตาย

    private void Start()
    {
        currentHealth = maxHealth;
        playerSprite = GetComponent<SpriteRenderer>();

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }

        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        // ถ้าตายแล้ว หรืออยู่ในช่วงอมตะ/ใช้อยู่ใต้มุดดิน ให้ข้าม
        if (isDead || isInvulnerable) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        // อัปเดต UI หลอดเลือด
        UpdateHealthUI();

        // เล่นเอฟเฟกต์กระพริบสีแดง
        StartCoroutine(DamageFlashRoutine());

        // เช็กเงื่อนไขการตาย
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthBarFill != null)
        {
            // คำนวณอัตราส่วนเลือด (0.0 - 1.0) ทำให้หลอดเลือดยาว/สั้นลง
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    private IEnumerator DamageFlashRoutine()
    {
        isInvulnerable = true;

        // 1. กระพริบสีแดงที่ UI (ถ้าใส่ damageFlashImage ไว้)
        if (damageFlashImage != null)
        {
            damageFlashImage.color = flashColor;
        }

        // 2. กระพริบตัวละคร Player เป็นสีแดง
        if (playerSprite != null)
        {
            playerSprite.color = Color.red;
        }

        yield return new WaitForSeconds(flashDuration);

        // คืนค่าสีเดิม
        if (damageFlashImage != null)
        {
            damageFlashImage.color = Color.clear;
        }

        if (playerSprite != null)
        {
            playerSprite.color = Color.white;
        }

        yield return new WaitForSeconds(0.3f); // เวลา อมตะหลังโดนชนแป๊บนึง
        isInvulnerable = false;
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Player ตายแล้ว!");

        // หยุดการเคลื่อนที่หรือซ่อนตัวละคร
        gameObject.SetActive(false);

        // แสดงหน้าจอ Game Over
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
    }

    // ฟังก์ชันสำหรับกดปุ่ม Replay ใน UI
    public void ReplayGame()
    {
        Time.timeScale = 1f; // รีเซ็ตเวลาในเกมให้ปกติกรณีมีการ Pause
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name); // โหลด Scene ปัจจุบันใหม่
    }
}