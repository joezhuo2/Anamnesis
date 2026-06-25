using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance { get; private set; }

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        HideTooltip();
    }

    private void Update()
    {
        GetComponent<RectTransform>().position = PlayerInputHandler.mousePos + new Vector2(15f, -15f);
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