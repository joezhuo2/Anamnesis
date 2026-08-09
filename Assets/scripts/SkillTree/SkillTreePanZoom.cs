using UnityEngine;
using UnityEngine.EventSystems;

public class SkillTreePanZoom : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
{
    [Header("References")]
    public RectTransform contentRect;

    [Header("Pan Settings")]
    public float panSpeed = 1f;
    public bool requireAltForPan = true;

    [Header("Zoom Settings")]
    public float zoomSpeed = 0.1f;
    public float minZoom = 0.3f;
    public float maxZoom = 3f;
    public float zoomLerpSpeed = 10f;

    private Vector2 dragStartScreenPos;
    private Vector2 contentStartAnchoredPos;
    private bool isPanning;
    private float targetScale = 1f;

    private void Update()
    {
        if (contentRect != null && Mathf.Abs(contentRect.localScale.x - targetScale) > 0.01f)
        {
            float newScale = Mathf.Lerp(contentRect.localScale.x, targetScale, zoomLerpSpeed * Time.unscaledDeltaTime);
            contentRect.localScale = new Vector3(newScale, newScale, 1f);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        bool shouldPan = eventData.button == PointerEventData.InputButton.Middle ||
                        (eventData.button == PointerEventData.InputButton.Left && requireAltForPan && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)));

        if (shouldPan && eventData.pointerEnter != null)
        {
            if (eventData.pointerEnter.GetComponentInParent<SkillNodeUI>() != null)
                shouldPan = false;
        }

        if (shouldPan && contentRect != null)
        {
            isPanning = true;
            dragStartScreenPos = eventData.position;
            contentStartAnchoredPos = contentRect.anchoredPosition;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isPanning || contentRect == null) return;

        Vector2 delta = eventData.position - dragStartScreenPos;

        float scale = contentRect.localScale.x;
        contentRect.anchoredPosition = contentStartAnchoredPos + delta / scale * panSpeed;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isPanning = false;
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (contentRect == null) return;

        float scroll = eventData.scrollDelta.y * zoomSpeed;
        targetScale = Mathf.Clamp(targetScale + scroll, minZoom, maxZoom);
    }

    public void ResetView()
    {
        if (contentRect != null)
        {
            contentRect.anchoredPosition = Vector2.zero;
            targetScale = 1f;
            contentRect.localScale = Vector3.one;
        }
    }
}