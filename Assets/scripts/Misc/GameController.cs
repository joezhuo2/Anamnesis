using System.Collections;
using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    public TextMeshProUGUI title;
    public TextMeshProUGUI subtitle;
    public GameObject UIPanel;

    private Coroutine titleRoutine;
    private Coroutine subtitleRoutine;
    private bool started = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        SetupEventSystem();
    }

    private void SetupEventSystem()
    {
        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            DontDestroyOnLoad(es);
        }
    }

    private void Start()
    {
        SetTitle("Anamnesis");
        SetSubtitle("Conquer the waves");

        started = false;
    }

    public void OnGameStart()
    {
        if (started) return;

        started = true;

        DisableTitle();
        DisableSubtitle();

        UIPanel.SetActive(true);
    }

    public void SetTitle(string text)
    {
        title.gameObject.SetActive(true);
        title.text = text;
    }
    public void SetSubtitle(string text)
    {
        subtitle.gameObject.SetActive(true);
        subtitle.text = text;
    }
    public void DisableTitle()
    {
        title.text = "";
        title.gameObject.SetActive(false);
    }
    public void DisableSubtitle()
    {
        subtitle.text = "";
        subtitle.gameObject.SetActive(false);
    }
    public void SetTitleForDuration(string newTitle, float duration, float fadeInTime = 0f, float fadeOutTime = 0f)
    {
        if (titleRoutine != null) StopCoroutine(titleRoutine);
        titleRoutine = StartCoroutine(ShowTextForDuration(title, newTitle, duration, null, Color.white, fadeInTime, fadeOutTime));
    }

    public void SetTitleForDuration(string newTitle, float duration, TMP_FontAsset font, Color color, float fadeInTime = 0f, float fadeOutTime = 0f)
    {
        if (titleRoutine != null) StopCoroutine(titleRoutine);
        titleRoutine = StartCoroutine(ShowTextForDuration(title, newTitle, duration, font, color, fadeInTime, fadeOutTime));
    }

    public void SetSubtitleForDuration(string newSubtitle, float duration, float fadeInTime = 0f, float fadeOutTime = 0f)
    {
        if (subtitleRoutine != null) StopCoroutine(subtitleRoutine);
        subtitleRoutine = StartCoroutine(ShowTextForDuration(subtitle, newSubtitle, duration, null, Color.white, fadeInTime, fadeOutTime));
    }

    public void SetSubtitleForDuration(string newSubtitle, float duration, TMP_FontAsset font, Color color, float fadeInTime = 0f, float fadeOutTime = 0f)
    {
        if (subtitleRoutine != null) StopCoroutine(subtitleRoutine);
        subtitleRoutine = StartCoroutine(ShowTextForDuration(subtitle, newSubtitle, duration, font, color, fadeInTime, fadeOutTime));
    }

    private IEnumerator ShowTextForDuration(TMP_Text text, string newText, float duration, TMP_FontAsset font, Color color, float fadeInTime, float fadeOutTime)
    {
        text.gameObject.SetActive(true);
        text.text = newText;

        if (font != null)
        {
            text.font = font;
        }

        text.color = color;

        if (fadeInTime > 0f)
        {
            yield return FadeRoutine(text, 0f, 1f, fadeInTime);
        }
        else
        {
            SetAlpha(text, 1f);
        }

        yield return new WaitForSeconds(duration);

        if (fadeOutTime > 0f)
        {
            yield return FadeRoutine(text, 1f, 0f, fadeOutTime);
        }
        else
        {
            SetAlpha(text, 0f);
        }
    }

    private IEnumerator FadeRoutine(TMP_Text text, float from, float to, float time)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, elapsed / time);
            SetAlpha(text, alpha);
            yield return null;
        }
        SetAlpha(text, to);
    }

    private void SetAlpha(TMP_Text text, float alpha)
    {
        Color color = text.color;
        color.a = alpha;
        text.color = color;
    }
}