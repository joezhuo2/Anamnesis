using System.Collections.Generic;
using UnityEngine;

namespace CrystalFlux.StatusEffectSystem
{
    public interface IStatusEffectReceiver
    {
        void Apply(EffectAsset effect, GameObject source, Vector2 location = default);
        void ClearAllEffects();

        T GetActiveFirstEffectOfType<T>() where T : EffectAsset;
        void GetActiveEffectsOfType<T>(List<T> results) where T : EffectAsset;

        void RemoveEffectAfterDelay<T>(float delay) where T : EffectAsset;
        void RemoveEffect<T>() where T : EffectAsset;
        void RemoveStacks<T>(int stacksToRemove) where T : EffectAsset;
    }
}
