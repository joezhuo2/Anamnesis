using UnityEngine;

namespace CrystalFlux.Core
{
    public interface IUnlockRequirement
    {
        bool Has(GameObject target);
    }
}
