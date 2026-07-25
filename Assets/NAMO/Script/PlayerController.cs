using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float movementSmoothing = 0.05f;
    private Vector2 moveInput;
    private Vector2 smoothDampVelocity;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    [Header("Status")]
    private bool isStunned = false;
    private bool hasShield = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (isStunned) return;

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;

        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (isStunned)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 targetVelocity = moveInput * moveSpeed;
        Vector2 smoothedVelocity = Vector2.SmoothDamp(rb.linearVelocity, targetVelocity, ref smoothDampVelocity, movementSmoothing);
        rb.MovePosition(rb.position + smoothedVelocity * Time.fixedDeltaTime);
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        if (moveInput.sqrMagnitude > 0.01f)
        {
            animator.SetFloat("MoveX", moveInput.x);
            animator.SetFloat("MoveY", moveInput.y);
            animator.SetFloat("Speed", moveInput.sqrMagnitude);
        }
        else
        {
            animator.SetFloat("MoveX", 0f);
            animator.SetFloat("MoveY", 0f);
            animator.SetFloat("Speed", 0f);
        }
    }

    public void TakeDamage(float stunDuration)
    {
        if (hasShield || isStunned) return;
        StartCoroutine(StunRoutine(stunDuration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        if (spriteRenderer != null) spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(0.15f);

        if (spriteRenderer != null) spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(duration - 0.15f);

        isStunned = false;
    }
}