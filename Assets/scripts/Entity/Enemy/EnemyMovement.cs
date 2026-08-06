using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public struct AppliedForce
{
    public float force;
    public float time;
    public Vector2 dir;
}

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMovement : MonoBehaviour
{
    public bool cardinalOnly = true;
    public bool canDeaggro = true;
    public float stoppingDistance = 0;
    public bool flipRotation = false;

    private List<AppliedForce> currentForces = new();
    private static readonly int SpeedHash = Animator.StringToHash("speed");
    [HideInInspector] public EnemyStats es;
    private Rigidbody2D rb;
    private Animator a;
    private bool wasMoving = false;
    private readonly float targetCheckInterval = 0.25f;
    private float nextTargetCheckTime = 0f;
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
        UpdateTargeting();

        Vector2 velocity = GetKnockbackVelocity();

        if (es.target != null && es.canMove && es.isAlive)
            velocity += GetMovementVelocity();

        rb.linearVelocity = velocity;
        SetAnimator(velocity != Vector2.zero);
    }
    private Vector2 GetKnockbackVelocity()
    {
        if (currentForces.Count == 0) return Vector2.zero;

        Vector2 totalForce = Vector2.zero;
        List<AppliedForce> remainingForces = new();

        foreach (var f in currentForces)
        {
            float timeRemaining = f.time - Time.deltaTime;
            if (timeRemaining <= 0f) continue;
            float kbf = f.force * (1f - (es.kbRes * 0.01f));
            remainingForces.Add(new() { dir = f.dir, force = kbf, time = timeRemaining });
            totalForce += f.dir * kbf;
        }

        currentForces = remainingForces;
        return totalForce;
    }
    public void ApplyKnockback(Vector2 d, float f, float t)
    {
        currentForces.Add(new() {dir = d, force = f, time = t});
    }
    public void SetTarget(GameObject target) => es.target = target;
    private Vector2 GetMovementVelocity()
    {
        Vector2 dist = es.target.transform.position - cTransform.position;
        float distMag = dist.magnitude;

        if (distMag > 0 && distMag <= stoppingDistance)
            return Vector2.zero;

        if (canDeaggro && distMag > es.detectionRange)
        {
            es.target = null;
            return Vector2.zero;
        }

        Vector2 dir = dist.normalized;
        if (cardinalOnly)
        {
            dir = Mathf.Abs(dir.x) > Mathf.Abs(dir.y)
                        ? new Vector2(Mathf.Sign(dir.x), 0)
                        : new Vector2(0, Mathf.Sign(dir.y));
        }

        if (dir.x != 0)
        {
            float directionSign = Mathf.Sign(dir.x);
            bool shouldMirror = flipRotation ? directionSign > 0 : directionSign < 0;
            float targetScaleX = Mathf.Abs(cScale.x) * (shouldMirror ? -1f : 1f);

            if (!Mathf.Approximately(cTransform.localScale.x, targetScaleX))
            {
                cScale.x = targetScaleX;
                cTransform.localScale = cScale;
            }
        }

        return dir * es.FinalSpd;
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