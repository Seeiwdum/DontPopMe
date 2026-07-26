using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class JuicyButtonFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [Header("Audio Spectrum Settings (เด้งตาม Beat เพลง)")]
    public AudioSource BGM_AudioSource;          // AudioSource ของเพลงหลัก
    public bool enableBeatPulse = true;          // เปิด/ปิด การเด้งตามเพลง
    [Range(0, 63)] public int frequencyIndex = 2;// ย่านความถี่เบส
    public float beatSensitivity = 3f;           // ความไวต่อเพลง (ตั้งค่าน้อยๆ ให้เด้งเบากว่า Title)
    public float maxBeatScale = 0.08f;           // ขีดจำกัดความใหญ่สูงสุดตอนเด้งตามเพลง (เบาๆ เบื้องต้น 8%)

    [Header("Audio Settings (เสียง SFX ปุ่ม)")]
    public AudioSource sfxAudioSource;
    public AudioClip hoverSFX;
    public AudioClip clickSFX;
    [Range(0f, 1f)] public float soundVolume = 1f;

    [Header("Hover Settings (เมาส์จ่อ)")]
    public float hoverScale = 1.15f;
    public float hoverDuration = 0.2f;

    [Header("Click Settings (ตอนกดคลิก)")]
    public float punchScale = 0.3f;
    public float punchDuration = 0.4f;
    public int vibrato = 10;
    public float elasticity = 0.8f;

    private Vector3 originalScale;
    private bool isHovered = false;
    private float[] spectrumData = new float[64];
    private float currentAudioPulse = 0f;

    void Awake()
    {
        if (sfxAudioSource == null)
        {
            sfxAudioSource = GetComponent<AudioSource>();
            if (sfxAudioSource == null)
            {
                sfxAudioSource = gameObject.AddComponent<AudioSource>();
                sfxAudioSource.playOnAwake = false;
            }
        }
    }

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        // คำนวณ Beat เพลงเฉพาะเวลาที่ไม่ได้เอามือไปจ่อหรือกดปุ่มอยู่
        if (enableBeatPulse && !isHovered)
        {
            HandleBeatPulse();
        }
    }

    // วิเคราะห์ Beat เพลงแบบเบาๆ
    void HandleBeatPulse()
    {
        if (BGM_AudioSource == null || !BGM_AudioSource.isPlaying) return;

        BGM_AudioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);

        float targetScale = spectrumData[frequencyIndex] * beatSensitivity;
        targetScale = Mathf.Clamp(targetScale, 0f, maxBeatScale);

        currentAudioPulse = Mathf.Lerp(currentAudioPulse, targetScale, Time.deltaTime * 15f);

        // นำค่า Beat ไปบวกเพิ่มจากขนาดเดิมของปุ่ม
        transform.localScale = originalScale * (1f + currentAudioPulse);
    }

    // 1. เมื่อเอาเมาส์ไปจ่อ
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;

        if (hoverSFX != null && sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(hoverSFX, soundVolume);
        }

        transform.DOKill();
        transform.DOScale(originalScale * hoverScale, hoverDuration)
                 .SetEase(Ease.OutBack)
                 .SetUpdate(true);
    }

    // 2. เมื่อเอาเมาส์ออก
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;

        transform.DOKill();
        transform.DOScale(originalScale, hoverDuration)
                 .SetEase(Ease.OutQuad)
                 .SetUpdate(true);
    }

    // 3. เมื่อกดคลิก
    public void OnPointerDown(PointerEventData eventData)
    {
        if (clickSFX != null && sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(clickSFX, soundVolume);
        }

        transform.DOKill();
        transform.localScale = originalScale;

        transform.DOPunchScale(Vector3.one * punchScale, punchDuration, vibrato, elasticity)
                 .SetUpdate(true)
                 .OnComplete(() =>
                 {
                     if (isHovered)
                     {
                         transform.DOScale(originalScale, 0.15f).SetUpdate(true);
                     }
                 });
    }

    void OnDisable()
    {
        transform.DOKill();
        transform.localScale = originalScale;
        isHovered = false;
    }
}