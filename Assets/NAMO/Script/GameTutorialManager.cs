using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameTutorialManager : MonoBehaviour
{
    public static GameTutorialManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private CanvasGroup bannerCanvasGroup;
    [SerializeField] private TextMeshProUGUI bannerText;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Tutorial Zone Settings")]
    [SerializeField] private GameObject invisibleWalls;
    [SerializeField] private SpriteRenderer tutorialFloorSprite; // Sprite WASD บนพื้น
    [SerializeField] private BubbleSpawner bubbleSpawner;

    // Quest Tracking States
    public enum QuestState { Movement, TrySkills, ZoneUnlocking, FreeRoam, WashingFaceEvent, MainGame }
    public QuestState CurrentState { get; private set; } = QuestState.Movement;

    // Track Inputs
    private bool pressedW, pressedA, pressedS, pressedD;
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
        // Step 1: เควส WASD
        yield return ShowBanner("Press W, A, S, D to walk");
        yield return new WaitUntil(() => pressedW && pressedA && pressedS && pressedD);

        // Step 2: เควส กด Skill 1, 2, 3
        yield return ShowBanner("Try out ur skill [1], [2], [3]");
        yield return new WaitUntil(() => pressed1 && pressed2 && pressed3);

        // Step 3: ปลดล็อคโซนฝึกสอน
        CurrentState = QuestState.ZoneUnlocking;
        yield return ShowBanner("Done! Let's Hob in to skin...");

        if (invisibleWalls != null) invisibleWalls.SetActive(false); // เอา Invisible Wall ออก

        // Fade Out พื้นฝึกสอนภายใน 5 วินาที
        if (tutorialFloorSprite != null)
        {
            float timer = 0f;
            Color initialColor = tutorialFloorSprite.color;
            while (timer < 5f)
            {
                timer += Time.deltaTime;
                tutorialFloorSprite.color = new Color(initialColor.r, initialColor.g, initialColor.b, Mathf.Lerp(1f, 0f, timer / 5f));
                yield return null;
            }
            tutorialFloorSprite.gameObject.SetActive(false);
        }

        // Step 4: ปล่อยเดินเล่นแปปนึง
        CurrentState = QuestState.FreeRoam;
        yield return ShowBanner("You");
        yield return new WaitForSeconds(4f);

        // Step 5: แจ้งเตือน Event ล้างหน้า!
        CurrentState = QuestState.WashingFaceEvent;
        yield return ShowBanner("⚠️ เขากำลังจะล้างหน้าแล้ว!! หลบฟองสบู่เร็ว!");
        yield return new WaitForSeconds(2.5f);

        // Step 6: เข้าสู่ Main Game และเริ่มสปอนฟองสบู่
        CurrentState = QuestState.MainGame;
        HideBanner();
        if (bubbleSpawner != null) bubbleSpawner.enabled = true;
    }

    private void Update()
    {
        if (CurrentState == QuestState.Movement)
        {
            if (Input.GetKeyDown(KeyCode.W)) pressedW = true;
            if (Input.GetKeyDown(KeyCode.A)) pressedA = true;
            if (Input.GetKeyDown(KeyCode.S)) pressedS = true;
            if (Input.GetKeyDown(KeyCode.D)) pressedD = true;
        }
        else if (CurrentState == QuestState.TrySkills)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) pressed1 = true;
            if (Input.GetKeyDown(KeyCode.Alpha2)) pressed2 = true;
            if (Input.GetKeyDown(KeyCode.Alpha3)) pressed3 = true;
        }
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