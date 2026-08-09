using UnityEngine;
using UnityEngine.InputSystem;

public class SkillTreePanZoom : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The RectTransform that gets panned/zoomed (the node container). Auto-resolved if left empty.")]
    public RectTransform contentRect;

    [Header("Pan Settings")]
    public float panSpeed = 1f;
    public bool requireAltForPan = true;
    public float dragStartThreshold = 5f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 0.1f;
    public float minZoom = 0.3f;
    public float maxZoom = 5f;
    public float zoomLerpSpeed = 10f;
    public bool zoomToCursor = true;

    private float targetScale = 1f;
    private float initialScale = 1f;
    private Vector2 initialAnchoredPos;

    private bool isPanning;
    private Vector2 panStartScreenPos;
    private Vector2 contentStartAnchoredPos;

    private void Awake() => ResolveContentRect();

    private void Start()
    {
        if (contentRect != null)
        {
            initialScale = contentRect.localScale.x;
            initialAnchoredPos = contentRect.anchoredPosition;
            targetScale = initialScale;
        }
    }

    private void ResolveContentRect()
    {
        if (contentRect != null && contentRect != (RectTransform)transform)
            return;

        var treeUI = GetComponentInParent<SkillTreeUI>();
        if (treeUI != null && treeUI.nodeContainer != null)
        {
            contentRect = treeUI.nodeContainer;
            return;
        }

        if (transform.parent != null)
        {
            var found = transform.parent.Find("NodesContainer");
            if (found != null)
            {
                contentRect = found as RectTransform;
                return;
            }
        }

        if (contentRect == null)
            contentRect = transform as RectTransform;
    }

    private void Update()
    {
        if (contentRect == null) return;

        HandleZoom();
        HandlePan();

        if (Mathf.Abs(contentRect.localScale.x - targetScale) > 0.001f)
        {
            float newScale = Mathf.Lerp(contentRect.localScale.x, targetScale, zoomLerpSpeed * Time.unscaledDeltaTime);
            contentRect.localScale = new Vector3(newScale, newScale, 1f);
        }
    }

    private void HandleZoom()
    {
        if (Mouse.current == null) return;

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) < 0.01f) return;

        float newTarget = Mathf.Clamp(targetScale + scroll * zoomSpeed, minZoom, maxZoom);
        if (Mathf.Abs(newTarget - targetScale) < 0.0001f) return;

        if (zoomToCursor)
        {
            Vector2 mouseScreen = Mouse.current.position.ReadValue();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(contentRect, mouseScreen, null, out Vector2 localPoint);

            Vector3 worldBefore = contentRect.TransformPoint(localPoint);
            targetScale = newTarget;
            contentRect.localScale = new Vector3(newTarget, newTarget, 1f);
            Vector3 worldAfter = contentRect.TransformPoint(localPoint);
            contentRect.position += worldBefore - worldAfter;
        }
        else
        {
            targetScale = newTarget;
        }
    }

    private void HandlePan()
    {
        if (Mouse.current == null) return;

        bool altHeld = Keyboard.current != null &&
            (Keyboard.current.leftAltKey.isPressed || Keyboard.current.rightAltKey.isPressed);
        bool leftHeld = Mouse.current.leftButton.isPressed;
        bool rightHeld = Mouse.current.rightButton.isPressed;
        bool middleHeld = Mouse.current.middleButton.isPressed;

        bool panButtonHeld = middleHeld ||
            (requireAltForPan ? (altHeld && (leftHeld || rightHeld)) : (leftHeld || rightHeld));

        Vector2 mouseScreen = Mouse.current.position.ReadValue();

        if (panButtonHeld && !isPanning)
        {
            isPanning = true;
            panStartScreenPos = mouseScreen;
            contentStartAnchoredPos = contentRect.anchoredPosition;
        }
        else if (panButtonHeld && isPanning)
        {
            Vector2 delta = mouseScreen - panStartScreenPos;
            if (delta.magnitude > dragStartThreshold)
            {
                float scale = contentRect.localScale.x;
                contentRect.anchoredPosition = contentStartAnchoredPos + delta / scale * panSpeed;
            }
        }
        else if (!panButtonHeld && isPanning)
        {
            isPanning = false;
        }
    }

    public void ResetView()
    {
        if (contentRect == null) return;

        contentRect.anchoredPosition = initialAnchoredPos;
        targetScale = initialScale;
        contentRect.localScale = new Vector3(initialScale, initialScale, 1f);
        isPanning = false;
    }
}