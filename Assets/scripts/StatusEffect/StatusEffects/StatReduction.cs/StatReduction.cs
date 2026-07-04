using UnityEngine;

[CreateAssetMenu(fileName = "se_stat_reduction", menuName = "Status Effects/Debuff/Stat Reduction")]
public class StatReduction : StatusEffect
{
    [Tooltip("% reduction per stack")] public float redPerStack = 10f;
    private StatBuff? currentActiveDebuff = null;
    public StatType statType = StatType.EffAtk;
    public StatType scalingStat = StatType.EffAtk;
    public float maxRed = 0.9f;
    public float minRed = 0f;

    public override void OnApply() => ApplyReduction();
    public override void OnStack()
    {
        UndoCurrentDebuff();
        ApplyReduction();
    }
    public override void OnExpire() => UndoCurrentDebuff();
    private void ApplyReduction()
    {
        if (target == null || !target.TryGetComponent<EntityStatManager>(out var esm)) return;

        float redPct = Mathf.Clamp(redPerStack * 0.01f * currentStacks, minRed, maxRed);

        StatBuff newDebuff = new(statType, redPct * esm.GetStat(scalingStat));
        currentActiveDebuff = newDebuff;

        esm.AddStat(newDebuff, false);
    }

    private void UndoCurrentDebuff()
    {
        if (target != null && target.TryGetComponent<EntityStatManager>(out var esm))
        {
            if (currentActiveDebuff.HasValue)
            {
                esm.AddStat(currentActiveDebuff.Value, true);
                currentActiveDebuff = null;
            }
        }
    }
}