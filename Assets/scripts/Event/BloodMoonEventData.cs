using System.Collections;
using UnityEngine;

namespace CrystalFlux.WaveEventSystem
{
    [CreateAssetMenu(fileName = "Blood Moon Event", menuName = "Data/Events/Blood Moon")]
    public class BloodMoonEventData : WaveEventData
    {
        [Header("Blood Moon")]
        [Tooltip("Seconds the buff lasts. Paused between waves and during Drought")] [Min(0f)] public float duration = 15f;
        [Tooltip("Enemies deal this much more damage (added to damagePct), take this much more damage and move this much faster (added to moveSpeedPct)")] public float bonusPct = 25f;

        [Header("Screen Tint")]
        public Color tintColor = new(0.6f, 0f, 0f, 1f);
        [Range(0f, 1f)] public float tintOpacity = 0.2f;
        [Tooltip("Seconds to fade the tint in")] [Min(0f)] public float fadeIn = 1f;
        [Tooltip("Seconds to fade the tint out after the buff ends")] [Min(0f)] public float fadeOut = 1.5f;
        [Tooltip("Overlay canvas sorting order. Below 0 keeps it under the HUD")] public int tintSortingOrder = -1;

        public override IEnumerator Run(WaveEventContext ctx)
        {
            if (duration <= 0f) yield break;

            BloodMoonEffect fx = BloodMoonEffect.Create(this, ctx);
            while (fx != null) yield return null;
        }
    }
}
