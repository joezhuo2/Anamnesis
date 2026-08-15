using TMPro;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance { get; private set; }

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Vector2 offset;

    private RectTransform crt;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        CacheRectTransform();
        HideTooltip();
    }

    private void Update()
    {
        CacheRectTransform();
        crt.position = PlayerInputHandler.mousePos + offset;
    }

    public void ShowTooltip(string title, string description, Vector2 os)
    {
        gameObject.SetActive(true);

        offset = os;
        titleText.text = title;
        descriptionText.text = description;
    }

    public void HideTooltip() => gameObject.SetActive(false);
    private void CacheRectTransform()
    {
        if (crt == null) crt = GetComponent<RectTransform>();
    }
}