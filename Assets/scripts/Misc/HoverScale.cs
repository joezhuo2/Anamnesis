using UnityEngine;
using UnityEngine.EventSystems;

namespace CrystalFlux.Utils
{
    public class HoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Transform target;
        [SerializeField] private float hoverScale = 1.1f;
        [SerializeField] private float speed = 12f;
        [SerializeField] private bool useUnscaledTime = true;

        private Vector3 baseScale;
        private bool hovered;

        private void Awake()
        {
            if (target == null) target = transform;
            baseScale = target.localScale;
        }

        private void OnEnable() => Setup();

        private void Setup()
        {
            hovered = false;
            if (target != null) target.localScale = baseScale;
        }

        private void OnDisable() => Setup();

        private void Update()
        {
            if (target == null) return;

            Vector3 goal = hovered ? baseScale * hoverScale : baseScale;
            Vector3 cur = target.localScale;
            if (cur == goal) return;

            if (speed <= 0f)
            {
                target.localScale = goal;
                return;
            }

            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            Vector3 next = Vector3.Lerp(cur, goal, 1f - Mathf.Exp(-speed * dt));
            target.localScale = (next - goal).sqrMagnitude < 1e-6f ? goal : next;
        }

        public void OnPointerEnter(PointerEventData eventData) => hovered = true;
        public void OnPointerExit(PointerEventData eventData) => hovered = false;

        private void OnMouseEnter() => hovered = true;
        private void OnMouseExit() => hovered = false;

        public void SetHoverScale(float scale) => hoverScale = scale;
    }
}
