using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Camera Zoom Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform pimpleTarget; // Object สิวบนหน้าจอ
    [SerializeField] private float zoomOrthographicSize = 2.5f; // ขนาดซูมของกล้อง 2D
    [SerializeField] private float zoomSpeed = 5f;

    [Header("Timeline Settings")]
    [SerializeField] private PlayableDirector playTimeline;

    [Header("Fade & Scene Transition")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private string gameplaySceneName = "GameplayScene"; // ชื่อ Scene ที่จะเปลี่ยนไป

    private Vector3 defaultCamPosition;
    private float defaultCamSize;

    private bool isHovering = false;
    private bool isPlayClicked = false;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null)
        {
            defaultCamPosition = mainCamera.transform.position;
            defaultCamSize = mainCamera.orthographicSize;
        }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    private void Update()
    {
        if (isPlayClicked || mainCamera == null || pimpleTarget == null) return;

        // คำนวณตำแหน่งและขนาดกล้อง
        Vector3 targetPosition = isHovering
            ? new Vector3(pimpleTarget.position.x, pimpleTarget.position.y, defaultCamPosition.z)
            : defaultCamPosition;

        float targetSize = isHovering ? zoomOrthographicSize : defaultCamSize;

        // เลื่อนกล้องและซูมแบบ Smooth
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, Time.deltaTime * zoomSpeed);
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetSize, Time.deltaTime * zoomSpeed);
    }

    // เมื่อเอาเมาส์มาชี้ที่ปุ่ม
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isPlayClicked) return;
        Debug.Log("เมาส์ชี้ปุ่ม Play แล้ว! (Hover In)");
        isHovering = true;
    }

    // เมื่อเอาเมาส์ออกจากปุ่ม
    public void OnPointerExit(PointerEventData eventData)
    {
        if (isPlayClicked) return;
        Debug.Log("เอาเมาส์ออกจากปุ่ม Play แล้ว! (Hover Out)");
        isHovering = false;
    }

    // ฟังก์ชันเมื่อกดปุ่ม Play
    public void OnPlayButtonClicked()
    {
        if (isPlayClicked) return;
        Debug.Log("กดปุ่ม Play แล้ว!");
        isPlayClicked = true;

        StartCoroutine(PlaySequenceAndStartGame());
    }

    private IEnumerator PlaySequenceAndStartGame()
    {
        if (playTimeline != null)
        {
            Debug.Log("กำลังเล่น Timeline...");
            playTimeline.Play();
            yield return new WaitForSeconds((float)playTimeline.duration);
        }

        if (fadeCanvasGroup != null)
        {
            Debug.Log("กำลังทำ Fade Out...");
            fadeCanvasGroup.blocksRaycasts = true;
            float timer = 0f;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Clamp01(timer / fadeDuration);
                yield return null;
            }

            fadeCanvasGroup.alpha = 1f;
        }

        Debug.Log("กำลังโหลดเข้า Scene: " + gameplaySceneName);
        SceneManager.LoadScene(gameplaySceneName);
    }
}