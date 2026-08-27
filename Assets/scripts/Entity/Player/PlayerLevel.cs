using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    private EntityStatManager esm;
    private int cLv;

    private void Start() => esm = GetComponent<EntityStatManager>();

    public void GainExp(float amount)
    {
        if (amount <= 0f || esm.GetStat(StatType.isAlive) <= 0f) return;

        var xp = esm.GetStat(StatType.Xp);
        var xpReq = esm.GetStat(StatType.XpReq);

        float finalAmt = amount * (1f + (esm.GetStat(StatType.ExpBonus) * 0.01f));
        esm.AddStat(new(StatType.Xp, xp + finalAmt));

        TextIndicatorSpawner.Instance.SpawnTextIndicator(
            Mathf.RoundToInt(finalAmt),
            transform.position,
            Color.magenta,
            0.7f + UnityEngine.Random.Range(0f, 0.15f),
            UnityEngine.Random.Range(0.5f, 0.7f),
            UnityEngine.Random.Range(0.8f, 1.2f),
            UnityEngine.Random.Range(0f, 0.2f),
            true
        );

        while (xp >= xpReq)
        {
            esm.AddStat(new(StatType.Xp, xpReq), false);
            LevelUp();
        }
    }

    private void LevelUp()
    {
        cLv = Mathf.RoundToInt(esm.GetStat(StatType.Level));
        esm.AddStat(new(StatType.Level, 1));

        GetComponent<PlayerSkillTree>().skillPoints++;

        esm.AddStat(new(StatType.maxHp, 3));
        esm.AddStat(new(StatType.attack, 1));
        esm.AddStat(new(StatType.Intelligence, 1));
        esm.AddStat(new(StatType.moveSpeed, 0.005f));

        GameController.Instance.SetTitleForDuration("Levelled Up!", 0.4f, 0.2f, 0.2f);
        GameController.Instance.SetSubtitleForDuration($"{cLv} → {Mathf.RoundToInt(esm.GetStat(StatType.Level))}", 0.4f, 0.2f, 0.2f);
    }
}