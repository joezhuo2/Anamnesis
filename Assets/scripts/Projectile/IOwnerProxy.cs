using UnityEngine;

namespace CrystalFlux.ProjectileSystem
{
    public interface IOwnerProxy
    {
        GameObject ProxyOwner { get; }
    }

    public static class OwnerProxy
    {
        public static GameObject Resolve(GameObject go)
        {
            if (go == null) return null;
            if (!go.TryGetComponent<IOwnerProxy>(out var p)) return go;

            GameObject o = p.ProxyOwner;
            return o != null ? o : go;
        }
    }
}
