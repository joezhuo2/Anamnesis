using TMPro;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance { get; private set; }

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Vector2 offset;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        HideTooltip();
    }

    private void Update()
    {
        GetComponent<RectTransform>().position = PlayerInputHandler.mousePos + offset;
    }

    public void ShowTooltip(string title, string description)
    {
        gameObject.SetActive(true);
        titleText.text = title;
        descriptionText.text = description;
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }
}