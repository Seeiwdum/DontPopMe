using UnityEngine;
using TMPro;
using DG.Tweening;

public class MusicReactiveUI : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource audioSource;
    [Range(0, 63)] public int frequencySampleIndex = 2; // ย่านความถี่เบส (ปกติอยู่ช่วง 0-4)
    public float audioSensitivity = 15f;                // ความไวต่อจังหวะเพลง
    public float maxBeatScale = 0.5f;                  // ขีดจำกัดการสั่นขนาดใหญ่สุด

    [Header("Evade Settings (Mouse Escape)")]
    public float detectionRadius = 150f;               // ระยะที่เมาส์เข้าใกล้แล้วจะเริ่มหลบ (พิกเซล UI)
    public float escapeDistance = 100f;                // ระยะที่จะถอยหนีออกมา
    public float moveSpeed = 8f;                        // ความเร็วในการตอบสนองหลบ/กลับที่เดิม

    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Vector2 targetEvadeOffset;
    private Vector2 currentEvadeOffset;
    
    private float[] spectrumData = new float[64];
    private float currentAudioScale = 0f;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Start()
    {
        // บันทึกตำแหน่งเดิมของ UI เอาไว้
        originalPosition = rectTransform.anchoredPosition;
    }

    void Update()
    {
        HandleAudioBeat();
        HandleMouseEvade();
        ApplyCombinedTransform();
    }

    // 1. วิเคราะห์เสียงเพลง Real-time เพื่อหาจังหวะสั่น
    void HandleAudioBeat()
    {
        if (audioSource == null || !audioSource.isPlaying) return;

        // ดึงข้อมูล Spectrum ความถี่เสียง
        audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);

        // คำนวณขนาดการสั่นจากความถี่ที่เลือก
        float targetScale = spectrumData[frequencySampleIndex] * audioSensitivity;
        targetScale = Mathf.Clamp(targetScale, 0f, maxBeatScale);

        // ค่อยๆ ปรับขนาดเข้าหาจังหวะเพลงให้นุ่มนวล
        currentAudioScale = Mathf.Lerp(currentAudioScale, targetScale, Time.deltaTime * 15f);
    }

    // 2. คำนวณทิศทางขยับหนีเมื่อเมาส์เข้าใกล้
    void HandleMouseEvade()
    {
        // แปลงตำแหน่งเมาส์บนหน้าจอให้เปรียบเทียบกับ UI ได้
        Vector2 mousePosition = Input.mousePosition;
        Vector2 uiScreenPosition = RectTransformUtility.WorldToScreenPoint(null, rectTransform.position);

        // วัดระยะห่างระหว่างเมาส์กับ UI
        float distance = Vector2.Distance(mousePosition, uiScreenPosition);

        if (distance < detectionRadius)
        {
            // หาคำนวณทิศทางตรงกันข้ามกับเมาส์ (ถอยหนี)
            Vector2 dirFromMouse = (uiScreenPosition - mousePosition).normalized;
            
            // ป้องกันกรณีเมาส์อยู่ตรงกลางเป๊ะๆ
            if (dirFromMouse == Vector2.zero) dirFromMouse = Vector2.up;

            // ตั้งเป้าหมายระยะที่จะหลบ
            targetEvadeOffset = dirFromMouse * escapeDistance;
        }
        else
        {
            // ถ้าเมาส์อยู่นอกระยะ ให้กลับมาที่เดิม (Offset เป็น 0)
            targetEvadeOffset = Vector2.zero;
        }

        // ค่อยๆ เคลื่อนที่ไปหา Offset เป้าหมายอย่างสมูท
        currentEvadeOffset = Vector2.Lerp(currentEvadeOffset, targetEvadeOffset, Time.deltaTime * moveSpeed);
    }

    // 3. รวมการสั่นตามเพลง + การหลบเมาส์ นำมาใช้งานกับ UI
    void ApplyCombinedTransform()
    {
        // ปรับตำแหน่งตามการหลบเมาส์
        rectTransform.anchoredPosition = originalPosition + currentEvadeOffset;

        // ปรับขนาด (Scale) สั่นขยายขึ้นลงตามจังหวะเพลง
        float scaleFactor = 1f + currentAudioScale;
        rectTransform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
    }

    // แสดงวงกลมระยะตรวจจับเมาส์ในหน้า Scene ให้เห็นชัดๆ เวลาปรับค่า
    private void OnDrawGizmosSelected()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(rectTransform.position, detectionRadius);
    }
}