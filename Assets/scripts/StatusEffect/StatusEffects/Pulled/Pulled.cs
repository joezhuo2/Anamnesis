using UnityEngine;

[CreateAssetMenu(fileName = "se_pulled", menuName = "Status Effects/Debuff/Pulled")]
public class Pulled : StatusEffect
{
    [Header("Pull Settings")]
    [Tooltip("Base pull speed in units/sec")] public float pullSpeed = 5f;
    [Tooltip("Additional pull speed per extra stack")] public float pullSpeedPerStack = 2f;
    [Tooltip("Distance at which pull reaches full speed")] public float fullSpeedRadius = 5f;
    [Tooltip("Minimum distance before stopping pull")] public float deadZone = 0.1f;
    [Tooltip("Disable entity movement while pulled")] public bool disableMovement = false;

    [HideInInspector] public Vector2 pullCenter;
    private bool wasMovementDisabled;

    public override void OnApply()
    {
        if (projectile != null) pullCenter = projectile.transform.position;
        else if (source != null) pullCenter = source.transform.position;

        if (disableMovement && target != null && target.TryGetComponent<EntityStatManager>(out var esm))
        {
            wasMovementDisabled = !esm.s.canMove;
            esm.s.canMove = false;
        }
    }

    public override void OnTick()
    {
        if (target == null) return;

        Vector2 targetPos = target.transform.position;
        Vector2 dir = pullCenter - targetPos;
        float dist = dir.magnitude;

        if (dist <= deadZone) return;
        if (dist > fullSpeedRadius)
        {
            if (target.TryGetComponent<StatusEffectManager>(out var sem))
                sem.RemoveStacks<Pulled>(int.MaxValue);
            return;
        }

        float distFactor = Mathf.Clamp01(dist / fullSpeedRadius);
        float effectiveSpeed = (pullSpeed + (pullSpeedPerStack * (currentStacks - 1))) * distFactor;

        Vector2 move = effectiveSpeed * tickInterval * dir.normalized;

        if (move.magnitude > dist) move = dir;

        target.transform.position = targetPos + move;
    }

    public override void OnExpire()
    {
        if (disableMovement && !wasMovementDisabled && target != null
            && target.TryGetComponent<EntityStatManager>(out var esm))
        {
            esm.s.canMove = true;
        }
    }
}
