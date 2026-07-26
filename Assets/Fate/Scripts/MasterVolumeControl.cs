using UnityEngine;
using UnityEngine.UI;

public class MasterVolumeControl : MonoBehaviour
{
    private Slider masterSlider;

    void Awake()
    {
        // ดึง Component Slider มาจาก Object นี้
        masterSlider = GetComponent<Slider>();
    }

    void Start()
    {
        if (masterSlider != null)
        {
            // ตั้งค่า Min และ Max ของ Slider ให้เป็น 0 ถึง 1 (0% - 100%)
            masterSlider.minValue = 0f;
            masterSlider.maxValue = 1f;

            // โหลดค่าเสียงเดิมที่เคยเซฟไว้ (ถ้าไม่มี ให้เริ่มที่ความดังสูงสุด 1f)
            float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            
            // ตั้งค่าตั้งต้นให้ Slider และระดับเสียง
            masterSlider.value = savedVolume;
            AudioListener.volume = savedVolume;

            // ผูกคำสั่งเมื่อมีการเลื่อน Slider ให้เรียกใช้ฟังก์ชัน OnVolumeChanged
            masterSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
    }

    // ฟังก์ชันปรับความดังเสียงทั้งหมดในเกมเมื่อเลื่อน Slider
    public void OnVolumeChanged(float value)
    {
        // ปรับความดังเสียงหลักทั้งหมดของเกม (0 = เงียบสนิท, 1 = ดังสุด)
        AudioListener.volume = value;

        // บันทึกค่าไว้ในเครื่องผู้เล่น
        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();
    }

    void OnDestroy()
    {
        // ลบ Listener ออกเมื่อ Object ถูกลบเพื่อป้องกัน Memory Leak
        if (masterSlider != null)
        {
            masterSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        }
    }
}