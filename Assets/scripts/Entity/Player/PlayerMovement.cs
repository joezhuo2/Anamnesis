using UnityEngine;
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]

[RequireComponent(typeof(PlayerUpgradeManager))]
public class PlayerMovement : MonoBehaviour
{
    public GameObject dashCooldownUI;

    [HideInInspector] public Vector2 moveInput;
    private static readonly int SpeedHash = Animator.StringToHash("speed");
    [HideInInspector] public PlayerStats p;
    private Rigidbody2D rb;
    private Animator animator;
    public static readonly float baseAnimSpeed = 1f;
    [HideInInspector] public float lastDashTime;
    private float dashTravelled;
    private Vector2 dashDir;
    [HideInInspector] public static int playerDir = 1; // 1 => facing right, -1 => facing left
    private void Awake() => animator = GetComponent<Animator>();
    private PlayerUpgradeManager pum;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        p = GetComponent<EntityStatManager>()?.s as PlayerStats;
        pum = GetComponent<PlayerUpgradeManager>();

        p.canDash = true;
        p.isDashing = false;

        lastDashTime = -Mathf.Infinity;

        if (dashCooldownUI != null && dashCooldownUI.TryGetComponent<PlayerDashCooldownUI>(out var pdcui))
            pdcui.Setup(this, p);
    }
    private void FixedUpdate()
    {
        if (!p.isAlive || !p.canMove)
        {
            rb.linearVelocity = Vector2.zero;
            animator.speed = baseAnimSpeed;
            return;
        }

        if (p.isDashing) rb.linearVelocity = dashDir * (p.FinalSpd * p.dashSpeedMult);
        else rb.linearVelocity = Vector2.ClampMagnitude(moveInput, 1f) * p.FinalSpd;

        float inputMag = moveInput.magnitude;

        if (!p.isDashing && ((moveInput.x > 0 && transform.localScale.x < 0) || (moveInput.x < 0 && transform.localScale.x > 0)))
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            playerDir *= -1;
        }

        if (inputMag > 0.1) animator.speed = Mathf.Max(inputMag * baseAnimSpeed, 0.01f);
        animator.SetFloat(SpeedHash, inputMag);
    }
    public void TryStartDash()
    {
        if (p.isDashing || !p.canDash) return;
        if (Time.time < lastDashTime + p.dashCooldown) return;
        if (p.currentStamina < p.dashStaminaCost) return;

        if (pum!= null) pum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnStartDash);

        p.currentStamina -= p.dashStaminaCost;
        lastDashTime = Time.time;
        dashTravelled = 0f;

        dashDir = moveInput.magnitude > 0.01f ? moveInput.normalized : new Vector2(playerDir, 0f);
        if (dashDir == Vector2.zero) dashDir = new Vector2(playerDir, 0f);

        p.isDashing = true;

        float dashSpeed = p.FinalSpd * p.dashSpeedMult;
        float dashDuration = dashSpeed > 0 ? p.dashDistance / dashSpeed : 0.2f;

        if (p.dashShouldApplyIFrame && gameObject.TryGetComponent<EntityHealth>(out var eh))
            eh.StartCoroutine(eh.TriggerIFrames(dashDuration + 0.2f));

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