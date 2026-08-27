using UnityEngine;

public class EnemyPhase : MonoBehaviour
{
    public float[] phaseThresholds = null;
    public PhaseBuff[] phaseBuffs;
    public int phase;

    private EntityStatManager esm;

    private void Awake() => esm = GetComponent<EntityStatManager>();

    public void UpdatePhase(int newPhase)
    {
        if (phase == newPhase) return;

        int previousPhase = phase;
        phase = newPhase;

        if (phaseBuffs == null || esm == null) return;

        if (newPhase > previousPhase)
        {
            foreach (var pb in phaseBuffs)
            {
                if (pb.phaseReq > previousPhase && pb.phaseReq <= newPhase)
                    esm.AddStat(pb.buff);
            }
        }
        else if (newPhase < previousPhase)
        {
            foreach (var pb in phaseBuffs)
            {
                if (pb.phaseReq > newPhase && pb.phaseReq <= previousPhase)
                    esm.AddStat(pb.buff, false);
            }
        }
    }
}

[System.Serializable]
public struct PhaseBuff
{
    public StatBuff buff;
    public int phaseReq;
}