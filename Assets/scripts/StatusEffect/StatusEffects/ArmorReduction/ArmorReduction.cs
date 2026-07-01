using UnityEngine;

[CreateAssetMenu(fileName = "se_armorReduction", menuName = "Status Effects/Debuff/ArmorReduction")]
public class ArmorReduction : StatusEffect
{
    [Tooltip("% Armor reduction per stack")] public float redPerStack = 10f;
    private StatBuff? currentActiveDebuff = null;

    public override void OnApply() => ApplyArmorReduction();
    public override void OnStack()
    {
        UndoCurrentDebuff();
        ApplyArmorReduction();
    }
    public override void OnExpire() => UndoCurrentDebuff();
    private void ApplyArmorReduction()
    {
        if (target == null || !target.TryGetComponent<EntityStatManager>(out var esm)) return;

        float redPct = Mathf.Clamp(redPerStack * 0.01f * currentStacks, 0f, 0.9f);

        StatBuff newDebuff = new(StatType.armor, redPct * esm.s.EffArmor);
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