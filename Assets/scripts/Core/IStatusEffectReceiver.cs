using System.Collections.Generic;
using UnityEngine;

namespace CrystalFlux.StatusEffectSystem
{
    public interface IStatusEffectReceiver
    {
        void Apply(StatusEffect effect, GameObject source, Vector2 location = default);
        void ClearAllEffects();
        T GetActiveFirstEffectOfType<T>() where T : StatusEffect;
        void GetActiveEffectsOfType<T>(List<T> results) where T : StatusEffect;
        void RemoveEffectAfterDelay<T>(float delay) where T : StatusEffect;
        void RemoveEffect<T>() where T : StatusEffect;
    }
}
