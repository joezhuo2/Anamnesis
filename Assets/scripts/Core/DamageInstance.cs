using UnityEngine;

namespace CrystalFlux.Core
{
    public enum DamageType { Physical, Spell, DoT, True, Heal, Consume }

    public struct DamageInstance
    {
        public DamageType type;
        public float amount;
        public bool isCrit;
        public Color indicatorColor;
        public GameObject owner;

        public DamageInstance(DamageType type, float amount, bool isCrit = false, Color indicatorColor = default, GameObject owner = null)
        {
            this.type = type;
            this.amount = amount;
            this.isCrit = isCrit;
            this.indicatorColor = indicatorColor;
            this.owner = owner;
        }
    }
}
