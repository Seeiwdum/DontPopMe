using UnityEngine;
using DG.Tweening;

public class SettingsWindowFX : MonoBehaviour
{
    [Header("Animation Settings")]
    public float openDuration = 0.6f;        // ระยะเวลาในการเด้งขึ้นมา
    public float closeDuration = 0.4f;       // ระยะเวลาในการสไลด์เก็บลงไป
    public float overshootAmount = 1.3f;     // แรงในการเด้งเลยจุดกึ่งกลาง (ยิ่งเยอะยิ่งเลยไกล)
    public float offscreenOffsetY = -1000f;   // ระยะซ่อนตัวด้านล่างหน้าจอ (แนวตั้ง)

    private RectTransform rectTransform;
    private Vector2 centerPosition;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // บันทึกตำแหน่งกึ่งกลางหน้าจอที่ตั้งไว้ในหน้า Scene
        centerPosition = rectTransform.anchoredPosition;
    }

    // ฟังก์ชันสำหรับเปิดหน้าต่าง Settings (เรียกใช้ตอนกดปุ่ม Option)
    public void OpenSettings()
    {
        // 1. เปิดการทำงานของ GameObject หน้าต่าง Settings
        gameObject.SetActive(true);

        // 2. เคลียร์ Animation เก่าออกก่อน
        rectTransform.DOKill();

        // 3. วาร์ปหน้าต่างไปแอบไว้ด้านล่างหน้าจอก่อนเริ่มแสดงผล
        rectTransform.anchoredPosition = new Vector2(centerPosition.x, centerPosition.y + offscreenOffsetY);

        // 4. ใช้ DOTween เล่น Animation เด้งขึ้นมาจากด้านล่างขึ้นมากึ่งกลางจอ
        // Ease.OutBack จะทำการพุ่งเลยเป้าหมายไปเล็กน้อย แล้วค่อยย้อนกลับมาหยุดตรงกลางให้อัตโนมัติ!
        rectTransform.DOAnchorPos(centerPosition, openDuration)
                     .SetEase(Ease.OutBack, overshootAmount)
                     .SetUpdate(true); // ให้ Animation เล่นได้แม้เกมจะ Pause อยู่
    }

    // แถม: ฟังก์ชันสำหรับปิดหน้าต่าง Settings (สำหรับนำไปแปะที่ปุ่ม Close หรือปุ่ม X)
    public void CloseSettings()
    {
        rectTransform.DOKill();

        // สไลด์กลับลงไปแอบด้านล่างหน้าจอ
        Vector2 targetPosition = new Vector2(centerPosition.x, centerPosition.y + offscreenOffsetY);

        rectTransform.DOAnchorPos(targetPosition, closeDuration)
                     .SetEase(Ease.InBack) // ยุบตัวลงนุ่มนวลก่อนสไลด์ลง
                     .SetUpdate(true)
                     .OnComplete(() =>
                     {
                         // พอสไลด์ลงพ้นหน้าจอแล้ว ค่อยปิด GameObject ใน Inspector
                         gameObject.SetActive(false);
                     });
    }
}