using Unity.Mathematics;
using UnityEngine;
[RequireComponent(typeof(Animator))]

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMovement : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("speed");
    [HideInInspector] public EnemyStats es;
    private Rigidbody2D rb;
    private Animator a;
    private bool wasMoving = false;

    [Header("Targeting")]
    private readonly float targetCheckInterval = 0.25f;
    private float nextTargetCheckTime = 0f;

    [Header("Cached")]
    private Transform cTransform;
    private Vector3 cScale;

    private void Start()
    {
        es = GetComponent<EntityStatManager>()?.s as EnemyStats;
        a = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        cTransform = transform;
        cScale = cTransform.localScale;

        UpdateTargeting();
    }
    private void Update()
    {
        if (!es.isAlive) return;

        UpdateTargeting();
        MoveToTarget();
    }
    private void SetTarget(GameObject target) => es.target = target;
    private void MoveToTarget()
    {
        if (es.target == null || !es.canMove)
        {
            SetAnimator(false);
            return;
        }

        Vector2 dist = es.target.transform.position - cTransform.position;
        float distMag = dist.magnitude;

        if (distMag > es.detectionRange)
        {
            es.target = null;
            rb.linearVelocity = Vector2.zero;
            SetAnimator(false);
            return;
        }

        Vector2 dir = dist.normalized;
        dir = Mathf.Abs(dir.x) > Mathf.Abs(dir.y)
            ? new Vector2(Mathf.Sign(dir.x), 0)
            : new Vector2(0, Mathf.Sign(dir.y));

        float finalSpeed = es.moveSpeed * (1f + (0.01f * es.moveSpeedPct));
        rb.linearVelocity = dir * finalSpeed;

        if (dir.x != 0)
        {
            float sign = Mathf.Sign(dir.x);
            if (sign != Mathf.Sign(cScale.x))
            {
                cScale.x *= -1;
                cTransform.localScale = cScale;
                es.enemyDirection *= -1;
            }
        }

        SetAnimator(dir != Vector2.zero);
    }

    private void SetAnimator(bool moving)
    {
        if (a != null && moving != wasMoving)
        {
            a.SetFloat(SpeedHash, moving ? 1f : 0f);
            wasMoving = moving;
        }
    }
    private void UpdateTargeting()
    {
        if (es.target != null) return;

        if (Time.time < nextTargetCheckTime) return;
        nextTargetCheckTime = Time.time + targetCheckInterval;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float minDist = math.INFINITY;
        GameObject targetPlayer = null;

        foreach (GameObject p in players)
        {
            float dist = Vector2.Distance(transform.position, p.transform.position);
            if (dist < minDist && dist <= es.detectionRange)
            {
                minDist = dist;
                targetPlayer = p;
            }
        }

        if (targetPlayer != null) es.target = targetPlayer;
    }
}