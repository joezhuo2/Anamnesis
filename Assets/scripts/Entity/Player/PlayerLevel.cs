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

        float finalAmt = amount * (1f + (p.expBonus * 0.01f));
        p.exp += finalAmt;

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