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
            if (speed <= 0f)
            {
                target.localScale = goal;
                return;
            }

            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            target.localScale = Vector3.Lerp(target.localScale, goal, 1f - Mathf.Exp(-speed * dt));
        }

        public void OnPointerEnter(PointerEventData eventData) => hovered = true;
        public void OnPointerExit(PointerEventData eventData) => hovered = false;

        private void OnMouseEnter() => hovered = true;
        private void OnMouseExit() => hovered = false;

        public void SetHoverScale(float scale) => hoverScale = scale;
    }
}
