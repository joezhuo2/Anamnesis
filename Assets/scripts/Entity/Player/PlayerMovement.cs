using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [HideInInspector] public PlayerStats p;
    public Rigidbody2D rb;
    public Vector2 moveInput;
    public Animator animator;
    public static readonly float baseAnimSpeed = 2.5f;
    private static float lastDashTime;
    private static float dashTravelled;
    public static int playerDir = 1; // 1 => facing right, -1 => facing left
    private void Awake() => animator = GetComponent<Animator>();
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        p = GetComponent<EntityStatManager>()?.s as PlayerStats;
    }
    private void FixedUpdate()
    {
        if (!p.isAlive || !p.canMove)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float inputMag = moveInput.magnitude;
        float rawSpeed = p.moveSpeed * (1f + (p.moveSpeedPct * 0.01f));
        float currentSpeed = p.isDashing ? rawSpeed * p.dashSpeedMult : rawSpeed;

        rb.linearVelocity = moveInput * currentSpeed;

        if ((moveInput.x > 0 && transform.localScale.x < 0) || (moveInput.x < 0 && transform.localScale.x > 0))
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            playerDir *= -1;
        }

        if (inputMag > 0.1) animator.speed = Mathf.Max(inputMag * baseAnimSpeed, 0.01f);
    }
    public void TryStartDash()
    {
        if (p.isDashing || !p.canDash) return;
        if (Time.time < lastDashTime + p.dashCooldown) return;
        if (p.currentStamina < p.dashStaminaCost) return;

        p.currentStamina -= p.dashStaminaCost;
        lastDashTime = Time.time;
        dashTravelled = 0f;

        p.isDashing = true;

        float rawSpeed = p.moveSpeed * (1f + (p.moveSpeedPct * 0.01f));
        float dashSpeed = rawSpeed * p.dashSpeedMult;
        float dashDuration = dashSpeed > 0 ? p.dashDistance / dashSpeed : 0.2f;

        if (p.dashShouldApplyIFrame && gameObject.TryGetComponent<EntityHealth>(out var eh)) 
            eh.StartCoroutine(eh.TriggerIFrames(dashDuration));

        StartCoroutine(DashRoutine(dashSpeed));
    }
    private System.Collections.IEnumerator DashRoutine(float dashSpeed)
    {
        while (dashTravelled < p.dashDistance)
        {
            dashTravelled += dashSpeed * Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        EndDash();
    }
    public void EndDash()
    {
        p.isDashing = false;
        dashTravelled = 0f;
    }
}