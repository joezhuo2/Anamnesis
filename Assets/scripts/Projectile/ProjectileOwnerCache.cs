using System.Collections.Generic;
using CrystalFlux.Core;
using UnityEngine;

namespace CrystalFlux.ProjectileSystem
{
    [DisallowMultipleComponent]
    public class ProjectileOwnerCache : MonoBehaviour
    {
        public GameObject Proxy { get; private set; }
        public IAttackEffectSource EffectSource { get; private set; }
        public IStatProvider Stats { get; private set; }
        public ISummonTrigger Summon { get; private set; }
        public IResourcePool Pool { get; private set; }
        public IDamageable Dmg { get; private set; }
        public IOrbitRegister Orbit { get; private set; }
        public IChargeRegister Charge { get; private set; }
        public int Team { get; private set; }
        public bool Hazard { get; private set; }
        public IReadOnlyList<IOnHitEffect> OnHit => onHit;

        private readonly List<IOnHitEffect> onHit = new();
        private bool built;
        private bool proxied;

        public static ProjectileOwnerCache Get(GameObject owner)
        {
            if (owner == null) return null;
            if (!owner.TryGetComponent(out ProjectileOwnerCache c)) c = owner.AddComponent<ProjectileOwnerCache>();

            c.Validate();
            return c;
        }

        private void Validate()
        {
            if (built && !(proxied && Proxy == null)) return;
            Build();
        }

        private void Build()
        {
            built = true;
            Proxy = OwnerProxy.Resolve(gameObject);
            proxied = Proxy != gameObject;

            Proxy.TryGetComponent(out IAttackEffectSource es);
            TryGetComponent(out IStatProvider sp);
            Proxy.TryGetComponent(out ISummonTrigger st);
            Proxy.TryGetComponent(out IResourcePool rp);
            Proxy.TryGetComponent(out IDamageable dmg);
            TryGetComponent(out IOrbitRegister orb);
            TryGetComponent(out IChargeRegister chg);

            EffectSource = es;
            Stats = sp;
            Summon = st;
            Pool = rp;
            Dmg = dmg;
            Orbit = orb;
            Charge = chg;
            Team = TryGetComponent(out ITeamMember itm) ? itm.TeamID : 0;
            Hazard = TryGetComponent<HazardOwner>(out _);

            Proxy.GetComponents(onHit);
        }
    }
}
