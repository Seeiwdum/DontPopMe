using UnityEngine;
using UnityEngine.SceneManagement; // จำเป็นสำหรับการเปลี่ยน Scene

public class SceneLoader : MonoBehaviour
{
    [Header("Target Scene")]
    public string gameplaySceneName = "Game"; // ตั้งชื่อ Scene Target ตรงนี้

    // ฟังก์ชันสำหรับเรียกเปลี่ยน Scene (นำไปผูกกับ OnClick ของปุ่ม Play)
    public void LoadGameplayScene()
    {
        // โหลด Scene ที่ระบุชื่อไว้
        SceneManager.LoadScene(gameplaySceneName);
    }
}