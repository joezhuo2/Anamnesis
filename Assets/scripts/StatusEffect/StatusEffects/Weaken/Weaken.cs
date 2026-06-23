using UnityEngine;

[CreateAssetMenu(fileName = "se_weaken", menuName = "Status Effects/Debuff/Weaken")]
public class Weaken : StatusEffect
{
    [Tooltip("% Attack reduction per stack")] public float redPerStack = 10f;
    private StatBuff? currentActiveDebuff = null;

    public override void OnApply() => ApplyWeakenReduction();
    public override void OnStack()
    {
        UndoCurrentDebuff();
        ApplyWeakenReduction();
    }
    public override void OnExpire() => UndoCurrentDebuff();
    private void ApplyWeakenReduction()
    {
        if (target == null || !target.TryGetComponent<EntityStatManager>(out var esm)) return;

        float redPct = Mathf.Clamp(redPerStack * 0.01f * currentStacks, 0f, 0.9f);

        StatBuff newDebuff = new(StatType.attack, redPct * esm.s.EffAtk);
        currentActiveDebuff = newDebuff;

        esm.AddStat(newDebuff, false, true);
    }

    private void UndoCurrentDebuff()
    {
        if (target != null && target.TryGetComponent<EntityStatManager>(out var esm))
        {
            if (currentActiveDebuff.HasValue)
            {
                esm.AddStat(currentActiveDebuff.Value, true, true);
                currentActiveDebuff = null;
            }
        }
    }
}