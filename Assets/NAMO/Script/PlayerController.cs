using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Animation Settings")]
    private Animator animator;
    private Vector2 lastMoveDirection = Vector2.down; // เก็บค่าทิศทางล่าสุดไว้ใช้ตอน Idle

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float movementSmoothing = 0.05f;
    private Vector2 moveInput;
    private Vector2 smoothDampVelocity;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); // ดึง Component Animator มาใช้งาน
    }

    private void Update()
    {
        // 1. รับค่า Input
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;

        // 2. อัปเดต Parameter ส่งให้ Blend Tree
        UpdateAnimationParameters();
    }

    private void UpdateAnimationParameters()
    {
        if (animator == null) return;

        // ถ้ามีการกดปุ่มเดิน (กำลังเคลื่อนที่)
        if (moveInput.sqrMagnitude > 0.01f)
        {
            lastMoveDirection = moveInput;

            // ส่งค่าพิกัดทิศทางไปที่ Blend Tree เพื่อเล่นท่าเดิน (เช่น 1, 0 หรือ 0, -1)
            animator.SetFloat("MoveX", moveInput.x);
            animator.SetFloat("MoveY", moveInput.y);
            animator.SetFloat("Speed", moveInput.sqrMagnitude);
        }
        else
        {
            // เมื่อหยุดเดิน ปรับพิกัด MoveX และ MoveY กลับเป็น 0, 0 
            // เพื่อให้พิกัดบน Blend Tree ตกลงมาที่ตรงกลาง (จุด Pos X: 0, Pos Y: 0) ซึ่งเป็นท่า Idle
            animator.SetFloat("MoveX", 0f);
            animator.SetFloat("MoveY", 0f);
            animator.SetFloat("Speed", 0f);
        }
    }

    private void FixedUpdate()
    {
        Vector2 targetVelocity = moveInput * moveSpeed;
        Vector2 smoothedVelocity = Vector2.SmoothDamp(rb.linearVelocity, targetVelocity, ref smoothDampVelocity, movementSmoothing);

        rb.MovePosition(rb.position + smoothedVelocity * Time.fixedDeltaTime);
    }
}