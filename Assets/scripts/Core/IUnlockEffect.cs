using UnityEngine;

public interface IUnlockEffect
{
    void Apply(GameObject target);
    void Remove(GameObject target);
}