using UnityEngine;

[CreateAssetMenu(fileName = "se_soulrend", menuName = "Status Effects/Buff/SoulRend")]
public class SoulRend : StatusEffect
{
    [Header("Increase per stack")]
    public float addPerStack = 1f;
    public float defShredAmt = 5f; // every 5
    public float respenAmt = 2f; // every 10
    public float physDmgPct = 10f; // every 20
    public float critDmgAmt = 20f; // every 25
    public float ultDmgAmt = 100f; // every 50

    private StatBuff? currentActiveAtkBuff = null;
    private StatBuff? currentDefShredBuff = null;
    private StatBuff? currentResPenBuff = null;
    private StatBuff? currentPhysDmgBuff = null;
    private StatBuff? currentCritDmgbuff = null;
    private StatBuff? currentUltDmgBuff = null;

    public override void OnApply() => ApplyBuffs();
    public override void OnStack()
    {
        UndoAllBuffs();
        ApplyBuffs();
    }
    public override void OnExpire() => UndoAllBuffs();
    private void ApplyBuffs()
    {
        if (target == null || !target.TryGetComponent<EntityStatManager>(out var esm)) return;

        int defShredMultiplier = currentStacks / 5;
        if (defShredMultiplier > 0)
        {
            StatBuff defShredBuff = new(StatType.defShred, defShredMultiplier * defShredAmt);
            currentDefShredBuff = defShredBuff;
            esm.AddStat(defShredBuff);
        }

        int resPenMultiplier = currentStacks / 10;
        if (resPenMultiplier > 0)
        {
            StatBuff resPenBuff = new(StatType.resPen, resPenMultiplier * respenAmt);
            currentResPenBuff = resPenBuff;
            esm.AddStat(resPenBuff);
        }

        int physDmgMultiplier = currentStacks / 20;
        if (physDmgMultiplier > 0)
        {
            StatBuff physDmgBuff = new(StatType.physicalDmgPct, physDmgMultiplier * physDmgPct);
            currentPhysDmgBuff = physDmgBuff;
            esm.AddStat(physDmgBuff);
        }

        int critDmgMultiplier = currentStacks / 25;
        if (critDmgMultiplier > 0)
        {
            StatBuff critDmgBuff = new(StatType.critDamage, critDmgMultiplier * critDmgAmt);
            currentCritDmgbuff = critDmgBuff;
            esm.AddStat(critDmgBuff);
        }

        int ultDmgMultiplier = currentStacks / 50;
        if (ultDmgMultiplier > 0)
        {
            StatBuff ultDmgBuff = new(StatType.UltDmgPct, ultDmgMultiplier * ultDmgAmt);
            currentUltDmgBuff = ultDmgBuff;
            esm.AddStat(ultDmgBuff);
        }

        float addAtkPct = addPerStack * currentStacks;
        StatBuff atkBuff = new(StatType.atkPct, addAtkPct);
        currentActiveAtkBuff = atkBuff;

        esm.AddStat(atkBuff);
    }

    private void UndoCurrentAtkBuff()
    {
        if (target != null && target.TryGetComponent<EntityStatManager>(out var esm))
        {
            if (currentActiveAtkBuff.HasValue) esm.AddStat(currentActiveAtkBuff.Value, false, true);
            currentActiveAtkBuff = null;
        }
    }
    private void UndoCurrentDefShredBuff()
    {
        if (target != null && target.TryGetComponent<EntityStatManager>(out var esm))
        {
            if (currentDefShredBuff.HasValue) esm.AddStat(currentDefShredBuff.Value, false, true);
            currentDefShredBuff = null;
        }
    }
    private void UndoCurrentResPenBuff()
    {
        if (target != null && target.TryGetComponent<EntityStatManager>(out var esm))
        {
            if (currentResPenBuff.HasValue) esm.AddStat(currentResPenBuff.Value, false, true);
            currentResPenBuff = null;
        }
    }
    private void UndoCurrentPhysDmgBuff()
    {
        if (target != null && target.TryGetComponent<EntityStatManager>(out var esm))
        {
            if (currentPhysDmgBuff.HasValue) esm.AddStat(currentPhysDmgBuff.Value, false, true);
            currentPhysDmgBuff = null;
        }
    }
    private void UndoCurrentCritDmgbuff()
    {
        if (target != null && target.TryGetComponent<EntityStatManager>(out var esm))
        {
            if (currentCritDmgbuff.HasValue) esm.AddStat(currentCritDmgbuff.Value, false, true);
            currentCritDmgbuff = null;
        }
    }
    private void UndoCurrentUltDmgBuff()
    {
        if (target != null && target.TryGetComponent<EntityStatManager>(out var esm))
        {
            if (currentUltDmgBuff.HasValue) esm.AddStat(currentUltDmgBuff.Value, false, true);
            currentUltDmgBuff = null;
        }
    }
    private void UndoAllBuffs()
    {
        if (target != null && target.TryGetComponent<EntityStatManager>(out var esm))
        {
            if (currentActiveAtkBuff.HasValue) esm.AddStat(currentActiveAtkBuff.Value, false);
            if (currentDefShredBuff.HasValue) esm.AddStat(currentDefShredBuff.Value, false);
            if (currentResPenBuff.HasValue) esm.AddStat(currentResPenBuff.Value, false);
            if (currentPhysDmgBuff.HasValue) esm.AddStat(currentPhysDmgBuff.Value, false);
            if (currentCritDmgbuff.HasValue) esm.AddStat(currentCritDmgbuff.Value, false);
            if (currentUltDmgBuff.HasValue) esm.AddStat(currentUltDmgBuff.Value, false);

            currentActiveAtkBuff = null;
            currentDefShredBuff = null;
            currentResPenBuff = null;
            currentPhysDmgBuff = null;
            currentCritDmgbuff = null;
            currentUltDmgBuff = null;
        }
    }
}