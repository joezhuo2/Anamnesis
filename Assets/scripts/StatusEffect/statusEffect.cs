using UnityEngine;
using CrystalFlux.Core;

namespace CrystalFlux.StatusEffectSystem
{
    public abstract class StatusEffect : EffectAsset
    {
        [Header("Basic")]
        [HideInInspector] public float currentTime;
        public float duration;
        [Tooltip("How often effect triggers")] public float tickInterval;
        public bool isBuff = false;

        [Header("UI")]
        public Sprite icon;
        public string effName;
        [TextArea(3, 10)] public string desc;

        [Header("Stacking")]
        public int maxStacks = 1;
        public bool loseAllStacksOnExpire;

        [HideInInspector] public GameObject target;
        [HideInInspector] public GameObject source;
        [HideInInspector] public Vector2 location;
        [HideInInspector] public int currentStacks = 0;
        [HideInInspector] public float potencyMultiplier = 1f;
        [HideInInspector] public StatusEffect origin;

        public int Generation { get; private set; }
        public bool Released { get; private set; }

        public void Setup(StatusEffect src, GameObject tgt, GameObject srcObj, Vector2 loc)
        {
            Generation++;
            Released = false;
            duration = src.duration;
            tickInterval = src.tickInterval;
            potencyMultiplier = src.potencyMultiplier;
            currentTime = 0f;
            currentStacks = 1;
            target = tgt;
            source = srcObj;
            location = loc;
            origin = src.origin != null ? src.origin : src;
            ResetRuntime();
        }

        public void Release()
        {
            Released = true;
            target = null;
            source = null;
            currentStacks = 0;
            currentTime = 0f;
            ResetRuntime();
        }

        protected virtual void ResetRuntime() {}

        public virtual void OnTick() {}
        public virtual void OnApply() {}
        public virtual void OnExpire() {}
        public virtual void OnStack() {}
    }
}
