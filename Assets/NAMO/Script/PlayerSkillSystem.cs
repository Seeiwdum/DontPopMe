using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkillSystem : MonoBehaviour
{
    [System.Serializable]
    public class SkillData
    {
        public string skillName = "Skill";
        public KeyCode keyBinding = KeyCode.Alpha1; // ปุ่ม 1, 2, 3
        public float cooldownTime = 3f;             // เวลา Cooldown (วินาที)
        public Image cooldownOverlayImage;           // Image เงามืดที่ปรับเป็น Radial 360
        [HideInInspector] public float currentCooldown = 0f;
    }

    [Header("Skills Setup")]
    [SerializeField] private SkillData skill1 = new SkillData { skillName = "ตดไขมัน (Burrow)", keyBinding = KeyCode.Alpha1, cooldownTime = 4f };
    [SerializeField] private SkillData skill2 = new SkillData { skillName = "Skill 2", keyBinding = KeyCode.Alpha2, cooldownTime = 5f };
    [SerializeField] private SkillData skill3 = new SkillData { skillName = "Skill 3", keyBinding = KeyCode.Alpha3, cooldownTime = 6f };

    private PlayerBurrowSkill burrowSkillScript;

    private void Awake()
    {
        burrowSkillScript = GetComponent<PlayerBurrowSkill>();
    }

    private void Update()
    {
        // อัปเดตคูลดาวน์และหลอด UI ของทุกสกิล
        UpdateSkillCooldown(skill1, 1);
        UpdateSkillCooldown(skill2, 2);
        UpdateSkillCooldown(skill3, 3);
    }

    private void UpdateSkillCooldown(SkillData skill, int skillNumber)
    {

        // ถ้าอยู่ในช่วง Cooldown ให้คำนวณเวลาและปรับ UI Fill Amount
        if (skill.currentCooldown > 0)
        {
            skill.currentCooldown -= Time.deltaTime;

            if (skill.cooldownOverlayImage != null)
            {
                skill.cooldownOverlayImage.fillAmount = skill.currentCooldown / skill.cooldownTime;
            }
        }
        else
        {
            if (skill.cooldownOverlayImage != null)
            {
                skill.cooldownOverlayImage.fillAmount = 0f; // คูลดาวน์เสร็จ พร้อมใช้
            }

            // ตรวจจับการกดปุ่ม
            if (Input.GetKeyDown(skill.keyBinding))
            {
                UseSkill(skill, skillNumber);
            }
        }
    }

    private void UseSkill(SkillData skill, int skillNumber)
    {
        // เริ่มนับ Cooldown
        skill.currentCooldown = skill.cooldownTime;

        // แจ้งเตือน Tutorial Manager ว่ากดสกิลนี้แล้ว
        if (GameTutorialManager.Instance != null)
        {
            GameTutorialManager.Instance.RegisterSkillPressed(skillNumber);
        }

        // เอฟเฟกต์การทำงานของแต่ละสกิล
        if (skillNumber == 1)
        {
            // เรียกใช้สคริปต์ตดไขมัน (Burrow Skill)
            if (burrowSkillScript != null)
            {
                burrowSkillScript.TriggerBurrowFromSkillSystem();
            }
        }
        else if (skillNumber == 2)
        {
            Debug.Log("ใช้งาน Skill 2!");
        }
        else if (skillNumber == 3)
        {
            Debug.Log("ใช้งาน Skill 3!");
        }
    }
}