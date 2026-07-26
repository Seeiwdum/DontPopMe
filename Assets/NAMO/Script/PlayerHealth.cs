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
    [SerializeField] private Image healthBarFill;       // รูป UI หลอดเลือด (ต้องเป็น Image Type: Filled)
    [SerializeField] private Image damageFlashImage;    // UI แผ่นสีแดงเต็มจอสำหรับกระพริบ
    [SerializeField] private GameObject gameOverUI;     // Panel หน้าจอ Game Over

    [Header("Damage Effect Settings")]
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private Color flashColor = new Color(1f, 0f, 0f, 0.4f);

    private SpriteRenderer playerSprite;
    private bool isDead = false;
    private bool isInvulnerable = false;

    private void Start()
    {
        currentHealth = maxHealth;
        playerSprite = GetComponent<SpriteRenderer>();

        if (gameOverUI != null) gameOverUI.SetActive(false);

        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        // ถ้าตายแล้ว หรืออยู่ในช่วงอมตะ ให้ข้าม
        if (isDead || isInvulnerable) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        Debug.Log($"<color=red>[Damage!]</color> Player โดนความเสียหาย! เลือดเหลือ: {currentHealth}/{maxHealth}");

        // อัปเดตหลอด UI
        UpdateHealthUI();

        // เล่นเอฟเฟกต์กระพริบแดง
        StartCoroutine(DamageFlashRoutine());

        // เช็กการตาย
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthBarFill != null)
        {
            // คำนวณอัตราส่วน 0.0 - 1.0 เพื่อปรับความยาวหลอดเลือด
            float healthRatio = (float)currentHealth / maxHealth;
            healthBarFill.fillAmount = healthRatio;
            Debug.Log($"[UI Update] Fill Amount ปรับเป็น: {healthRatio}");
        }
        else
        {
            Debug.LogWarning("⚠️ ยังไม่ได้ลากรูป Health Bar Fill มาใส่ใน Inspector ของ PlayerHealth!");
        }
    }

    private IEnumerator DamageFlashRoutine()
    {
        isInvulnerable = true;

        if (damageFlashImage != null) damageFlashImage.color = flashColor;
        if (playerSprite != null) playerSprite.color = Color.red;

        yield return new WaitForSeconds(flashDuration);

        if (damageFlashImage != null) damageFlashImage.color = Color.clear;
        if (playerSprite != null) playerSprite.color = Color.white;

        yield return new WaitForSeconds(0.3f); // ช่วงเวลาอมตะหลังโดนชน
        isInvulnerable = false;
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("💀 Player ตายแล้ว!");

        // ซ่อนตัวละคร
        gameObject.SetActive(false);

        // ขึ้นหน้าจอ Game Over
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
    }

    public void ReplayGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}