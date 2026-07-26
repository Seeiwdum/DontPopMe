using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SurvivalTimelineManager : MonoBehaviour
{
    [Header("Survival Settings")]
    [Tooltip("ระยะเวลาที่ต้องเอาชีวิตรอดเพื่อชนะ (วินาที)")]
    [SerializeField] private float survivalGoalTime = 60f;
    private float currentSurvivalTime = 0f;
    private bool isGameActive = true;

    [Header("UI References")]
    [SerializeField] private Image timelineProgressBar;  // UI Image หลอดความคืบหน้า (Image Type: Filled)
    [SerializeField] private TextMeshProUGUI timerText;   // ข้อความนับเวลา เช่น "00:45"
    [SerializeField] private GameObject victoryPanel;     // Panel หน้าจอชนะ

    [Header("Pimple Evolution Visuals")]
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color blackSpotColor = new Color(0.2f, 0.1f, 0.1f, 1f); // สีรอยดำเมื่อชนะ

    private void Start()
    {
        currentSurvivalTime = 0f;
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    private void Update()
    {
        if (!isGameActive) return;

        currentSurvivalTime += Time.deltaTime;
        float progress = Mathf.Clamp01(currentSurvivalTime / survivalGoalTime);

        // อัปเดต UI Timeline Bar & Text
        if (timelineProgressBar != null)
        {
            timelineProgressBar.fillAmount = progress;
        }

        if (timerText != null)
        {
            float remainingTime = Mathf.Max(0, survivalGoalTime - currentSurvivalTime);
            int seconds = Mathf.CeilToInt(remainingTime);
            timerText.text = $"เวลาที่เหลือ: {seconds}s";
        }

        // ค่อยๆ เปลี่ยนสีตัวละครให้เข้มขึ้นตามระดับความสุกของสิว
        if (playerSprite != null)
        {
            playerSprite.color = Color.Lerp(normalColor, blackSpotColor, progress * 0.5f);
        }

        // เช็กเงื่อนไขชนะ!
        if (currentSurvivalTime >= survivalGoalTime)
        {
            TriggerVictory();
        }
    }

    private void TriggerVictory()
    {
        isGameActive = false;
        Debug.Log("🎉 ชนะแล้ว! สิวกลายเป็นรอยดำฝังลึกสำเร็จ!");

        // เปลี่ยนตัวละครสิวเป็นรอยดำ 100%
        if (playerSprite != null) playerSprite.color = blackSpotColor;

        // ปิดการทำงานของ Spawner ฟองสบู่
        BubbleSpawner spawner = FindFirstObjectByType<BubbleSpawner>();
        if (spawner != null) spawner.enabled = false;

        // แสดงหน้าจอ Victory
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
    }
}