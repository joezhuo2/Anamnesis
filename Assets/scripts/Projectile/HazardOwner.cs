using UnityEngine;

namespace CrystalFlux.ProjectileSystem
{
    // Marks a hidden, ownerless source (e.g. a wave event) whose projectiles hit every team and skip hit feedback.
    [DisallowMultipleComponent]
    public class HazardOwner : MonoBehaviour, IAimProvider
    {
        [HideInInspector] public Transform target;

        public Vector2 AimPoint => target != null ? (Vector2)target.position : (Vector2)transform.position;
    }
}
