using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    [HideInInspector] public PlayerStats p;

    private void Start()
    {
        p = GetComponent<EntityStatManager>()?.s as PlayerStats;
    }

    public void GainExp(float amount)
    {
        if (amount <= 0f || !p.isAlive) return;

        p.exp += amount * (1f + (p.expBonus * 0.01f));

        while (p.exp >= p.ExpReq)
        {
            p.exp -= p.ExpReq;
            LevelUp();
        }
    }
    private void LevelUp()
    {
        p.level++;

        GetComponent<PlayerSkillTree>().skillPoints++;

        p.maxHp += 3;
        p.attack++;
        p.intelligence++;
        p.moveSpeed += 0.005f;

        GameController.Instance.SetTitleForDuration("Levelled Up!", 0.4f, 0.2f, 0.2f);
        GameController.Instance.SetSubtitleForDuration($"{p.level - 1} → {p.level}", 0.4f, 0.2f, 0.2f);
    }
}