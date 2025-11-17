using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ChatBubble : MonoBehaviour
{
    public TextMeshProUGUI chatText;
    public CanvasGroup canvasGroup;

    private Transform target;
    private Vector3 offset;

    public void Initialize(Transform followTarget, string text, Vector3 worldOffset, float lifeTime)
    {
        target = followTarget;
        offset = worldOffset;
        chatText.text = text;

        StartCoroutine(ShowAndHide(lifeTime));
    }

    private void Update()
    {
        if (target != null)
            transform.position = target.position + offset;
    }

    private IEnumerator ShowAndHide(float lifeTime)
    {
        canvasGroup.alpha = 0;

        // Fade in
        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.deltaTime * 3f;
            yield return null;
        }

        yield return new WaitForSeconds(lifeTime);

        // Fade out
        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.deltaTime * 3f;
            yield return null;
        }

        Destroy(gameObject);
    }
}
