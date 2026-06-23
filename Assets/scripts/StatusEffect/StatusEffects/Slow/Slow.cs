using UnityEngine;

[CreateAssetMenu(fileName = "se_slow", menuName = "Status Effects/Debuff/Slow")]
public class Slow : StatusEffect
{
    [Tooltip("% Attack reduction per stack")] public float redPerStack = 10f;
    private StatBuff? currentActiveDebuff = null;

    public override void OnApply() => ApplySlowReduction();
    public override void OnStack()
    {
        UndoCurrentDebuff();
        ApplySlowReduction();
    }
    public override void OnExpire() => UndoCurrentDebuff();
    private void ApplySlowReduction()
    {
        if (target == null || !target.TryGetComponent<EntityStatManager>(out var esm)) return;

        float redPct = Mathf.Clamp(redPerStack * 0.01f * currentStacks, 0f, 0.9f);

        StatBuff newDebuff = new(StatType.moveSpeed, redPct * esm.s.FinalSpd);
        currentActiveDebuff = newDebuff;

        esm.AddStat(newDebuff, false, true);
    }

    private void UndoCurrentDebuff()
    {
        if (target != null && target.TryGetComponent<EntityStatManager>(out var esm))
        {
            if (currentActiveDebuff.HasValue) esm.AddStat(currentActiveDebuff.Value, true, true);
            currentActiveDebuff = null;
        }
    }
}