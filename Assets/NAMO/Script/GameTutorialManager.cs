using System.Collections;
using UnityEngine;
using TMPro;

public class GameTutorialManager : MonoBehaviour
{
    public static GameTutorialManager Instance { get; private set; }

    // ใน GameTutorialManager.cs
    [Header("Full Map Settings")]
    [SerializeField] private BoxCollider2D fullMapBoundsCollider; // ลาก FullMapBounds มาวางตรงนี้!
    [SerializeField] private float overviewCameraSize = 25f;
    [SerializeField] private float cameraHoldDuration = 2f;
    [SerializeField] private float cameraTransitionDuration = 1.5f;

    [Header("UI References")]
    [SerializeField] private CanvasGroup bannerCanvasGroup;
    [SerializeField] private TextMeshProUGUI bannerText;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Tutorial Zone Settings")]
    [SerializeField] private GameObject invisibleWalls;
    [SerializeField] private SpriteRenderer tutorialFloorSprite;
    [SerializeField] private BubbleSpawner bubbleSpawner;

    [Header("Full Map Camera Settings (ตั้งค่าระยะกล้องแมพใหญ่)")]
    [SerializeField] private Vector3 fullMapCenter = Vector3.zero; // จุดศูนย์กลางหน้าคน
    [Header("Full Map Bounds (ขอบแมพหน้าคนทั้งหมด)")]
    [SerializeField] private float fullMapMinX = -40f;
    [SerializeField] private float fullMapMaxX = 40f;
    [SerializeField] private float fullMapMinY = -25f;
    [SerializeField] private float fullMapMaxY = 25f;

    public enum QuestState { Movement, TrySkills, ZoneUnlocking, FreeRoam, WashingFaceEvent, MainGame }
    public QuestState CurrentState { get; private set; } = QuestState.Movement;

    private bool pressed1, pressed2, pressed3;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (bubbleSpawner != null) bubbleSpawner.enabled = false;
        StartCoroutine(TutorialSequence());
    }

    private IEnumerator TutorialSequence()
    {
        // -------------------------------------------------------------
        // 1. WASD Movement Quest
        // -------------------------------------------------------------
        CurrentState = QuestState.Movement;
        yield return ShowBanner("Press W, A, S, D to move");
        
        bool w = false, a = false, s = false, d = false;
        while (!(w && a && s && d))
        {
            if (Input.GetKeyDown(KeyCode.W)) w = true;
            if (Input.GetKeyDown(KeyCode.A)) a = true;
            if (Input.GetKeyDown(KeyCode.S)) s = true;
            if (Input.GetKeyDown(KeyCode.D)) d = true;
            yield return null;
        }

        // -------------------------------------------------------------
        // 2. Skill Quest (1, 2, 3)
        // -------------------------------------------------------------
        CurrentState = QuestState.TrySkills;
        yield return ShowBanner("Try your skill [1], [2], [3] ");
        
        while (!(pressed1 && pressed2 && pressed3))
        {
            yield return null;
        }

        // -------------------------------------------------------------
        // 3. Unlock Zone & Overview Camera Effect
        // -------------------------------------------------------------
        CurrentState = QuestState.ZoneUnlocking;
        yield return ShowBanner("Done! Unlock new path...");

        // ปิดกำแพงล่องหน
        if (invisibleWalls != null) invisibleWalls.SetActive(false);

        // สั่งกล้องให้แพนไปดูแมพหน้าคน
        if (CameraFollow2D.Instance != null)
        {
            if (fullMapBoundsCollider != null)
            {
                // กรณีมี BoxCollider2D แมพใหญ่
                CameraFollow2D.Instance.TriggerFaceMapOverview(
                    fullMapBoundsCollider,
                    overviewCameraSize,
                    cameraHoldDuration,
                    cameraTransitionDuration
                );
            }
            else
            {
                // กรณีไม่มี Collider แมพใหญ่ สั่งปลดล็อคขอบเขตกล้องทันที
                CameraFollow2D.Instance.DisableBounds();
            }
        }

        // C. ค่อยๆ Fade รูป WASD บนพื้นให้หายไป
        if (tutorialFloorSprite != null)
        {
            float timer = 0f;
            Color initialColor = tutorialFloorSprite.color;
            while (timer < 3f)
            {
                timer += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, timer / 3f);
                tutorialFloorSprite.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
                yield return null;
            }
            tutorialFloorSprite.gameObject.SetActive(false);
        }

        // -------------------------------------------------------------
        // 4. Free Roam (วิ่งเล่นบนแมพใหญ่)
        // -------------------------------------------------------------
        CurrentState = QuestState.FreeRoam;
        yield return ShowBanner("Survey the area");
        yield return new WaitForSeconds(6f);

        // -------------------------------------------------------------
        // 5. Event Washing Face
        // -------------------------------------------------------------
        CurrentState = QuestState.WashingFaceEvent;
        yield return ShowBanner("She is about to wash her face!! dodge the bubbles!");
        yield return new WaitForSeconds(2.5f);

        // -------------------------------------------------------------
        // 6. Main Game
        // -------------------------------------------------------------
        CurrentState = QuestState.MainGame;
        HideBanner();
        if (bubbleSpawner != null) bubbleSpawner.enabled = true;
    }

    public void RegisterSkillPressed(int skillIndex)
    {
        if (CurrentState != QuestState.TrySkills) return;

        if (skillIndex == 1) pressed1 = true;
        if (skillIndex == 2) pressed2 = true;
        if (skillIndex == 3) pressed3 = true;
    }

    public IEnumerator ShowBanner(string text)
    {
        bannerText.text = text;
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            bannerCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        bannerCanvasGroup.alpha = 1f;
    }

    public void HideBanner()
    {
        StartCoroutine(FadeOutBannerRoutine());
    }

    private IEnumerator FadeOutBannerRoutine()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            bannerCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        bannerCanvasGroup.alpha = 0f;
    }
}