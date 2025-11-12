using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextFader : MonoBehaviour
{
    public TextMeshProUGUI text;
    [Tooltip("Fade duration in seconds")]
    public float fadeDuration = 0.6f;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        if (text == null) text = GetComponent<TextMeshProUGUI>();
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    public IEnumerator FadeIn(string newText)
    {
        text.text = newText;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            canvasGroup.alpha = Mathf.SmoothStep(0f, 1f, t);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    public IEnumerator FadeOut()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            canvasGroup.alpha = Mathf.SmoothStep(1f, 0f, t);
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }

    public IEnumerator ShowFor(string newText, float visibleSeconds)
    {
        yield return FadeIn(newText);
        yield return new WaitForSeconds(visibleSeconds);
        yield return FadeOut();
    }
}