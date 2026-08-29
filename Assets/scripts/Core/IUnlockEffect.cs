using UnityEngine;

namespace CrystalFlux.Core
{
    public interface IUnlockEffect
    {
        void Apply(GameObject target);
        void Remove(GameObject target);
    }
}
