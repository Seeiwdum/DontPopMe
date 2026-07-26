using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class JuicyButtonFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [Header("Audio Settings (เสียง SFX)")]
    public AudioSource audioSource;       // ตัวเล่นเสียง (ถ้าไม่ใส่ สคริปต์จะหาให้อัตโนมัติ)
    public AudioClip hoverSFX;            // เสียงตอนเอาเมาส์ไปจ่อ (จะใส่หรือไม่ใส่ก็ได้)
    public AudioClip clickSFX;            // เสียงตอนกดคลิก (ใส่ไฟล์เสียง SFX ตรงนี้)
    [Range(0f, 1f)] public float soundVolume = 1f; // ความดังของเสียง SFX

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

    void Awake()
    {
        // ถ้าไม่ได้ลาก AudioSource มาใส่ จะพยายามดึง AudioSource จากในเครื่องหรือสร้างให้อัตโนมัติ
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }
    }

    void Start()
    {
        originalScale = transform.localScale;
    }

    // 1. เมื่อเอาเมาส์ไปจ่อ
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;

        // เล่นเสียง Hover (ถ้ามีใส่ไว้)
        if (hoverSFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(hoverSFX, soundVolume);
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
        // เล่นเสียง Click SFX
        if (clickSFX != null && audioSource != null)
        {
            // PlayOneShot ช่วยให้เสียงเล่นซ้อนกันได้นุ่มนวล แม้จะกดคลิกย้ำๆ ไวๆ
            audioSource.PlayOneShot(clickSFX, soundVolume);
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