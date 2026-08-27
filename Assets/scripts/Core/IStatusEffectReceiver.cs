using UnityEngine;

public interface IStatusEffectReceiver
{
    void Apply(StatusEffect effect, GameObject source);
}