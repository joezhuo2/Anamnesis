using System;
using UnityEngine;
using UnityEngine.UI;
using CrystalFlux.Core;

namespace CrystalFlux.WaveSystem
{
    public class RewardLockButton : MonoBehaviour
    {
        [Header("UI Visual Elements")]
        public Button button;
        public Image iconImage;
        public Sprite lockedIcon;
        public Sprite unlockedIcon;
        public Vector2 offset;

        private Action onClickCallback;

        public void Setup(bool show, bool locked, Action clickCallback)
        {
            CacheComponents();
            onClickCallback = clickCallback;

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(HandleClick);
                button.interactable = true;
            }

            SetLocked(locked);

            if (TryGetComponent<ITooltipDisplay>(out var td))
                td.ShowTooltip("Lock", "Keeps this reward when rerolling.\nOnly one reward can be locked. The lock is released after each reroll.", offset);

            gameObject.SetActive(show);
        }

        public void SetLocked(bool locked)
        {
            CacheComponents();
            if (iconImage != null) iconImage.sprite = locked ? lockedIcon : unlockedIcon;
        }

        public void SetInteractable(bool interactable)
        {
            CacheComponents();
            if (button != null) button.interactable = interactable;
        }

        public void ResetForPooling()
        {
            onClickCallback = null;
            CacheComponents();

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.interactable = true;
            }

            SetLocked(false);
            gameObject.SetActive(false);
        }

        private void CacheComponents()
        {
            if (button == null) TryGetComponent(out button);
            if (iconImage == null) TryGetComponent(out iconImage);
        }

        private void HandleClick() => onClickCallback?.Invoke();
    }
}
