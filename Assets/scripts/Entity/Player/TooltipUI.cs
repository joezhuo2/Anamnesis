using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using TMPro;
using UnityEngine;

namespace CrystalFlux.UISystem
{
    [RequireComponent(typeof(RectTransform))]
    public class TooltipUI : MonoBehaviour
    {
        public static TooltipUI Instance { get; private set; }

        public TextMeshProUGUI titleText;
        public TextMeshProUGUI descriptionText;
        public Vector2 offset;

        private RectTransform crt;
        private Vector2 lastPos;
        private bool posDirty = true;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            CacheRectTransform();
            HideTooltip();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            Vector2 p = InputState.mousePos + offset;
            if (!posDirty && p == lastPos) return;

            CacheRectTransform();
            crt.position = p;
            lastPos = p;
            posDirty = false;
        }

        public void ShowTooltip(string title, string description, Vector2 os)
        {
            gameObject.SetActive(true);

            if (offset != os) posDirty = true;
            offset = os;
            if (titleText != null) titleText.text = title;
            if (descriptionText != null) descriptionText.text = description;
        }

        public void HideTooltip()
        {
            posDirty = true;
            gameObject.SetActive(false);
        }
        private void CacheRectTransform()
        {
            if (crt == null) crt = GetComponent<RectTransform>();
        }
    }
}
