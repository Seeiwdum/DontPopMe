using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("UI Reference")]
    [SerializeField] private GameObject gameOverUI; // 🟢 ลาก Panel Game Over มาใส่ตรงนี้

    private PlayerSkillController skillController;
    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        skillController = GetComponent<PlayerSkillController>();

        // ซ่อน UI ไว้ก่อนตอนเริ่มเกม
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        if (skillController != null && (skillController.IsBurrowed || skillController.IsInvincible))
        {
            return;
        }

        currentHealth -= damage;
        Debug.Log($"❤️ เลือดปัจจุบัน: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("💀 Player ตาย!");

        // เปิดหน้า UI Replay/Game Over
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }

        // หยุดเวลาในเกม (optional)
        Time.timeScale = 0f;
    }

    // 🟢 ฟังก์ชันนี้ไว้ผูกกับปุ่ม Replay Button ใน UI (OnClick)
    public void ReplayButton()
    {
        Time.timeScale = 1f; // คืนค่าเวลา
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}