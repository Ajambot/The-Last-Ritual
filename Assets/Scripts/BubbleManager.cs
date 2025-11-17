using UnityEngine;

public class BubbleManager : MonoBehaviour
{
    public ChatBubble bubblePrefab;
    public float verticalOffset = 2f;
    public float horizontalOffset = 0f;
    public float lifeTime = 3f;

    public void CreateBubble(Transform target, string text)
    {
        if (bubblePrefab == null)
        {
            Debug.LogError("❌ BubbleManager: bubblePrefab is NOT assigned!");
            return;
        }


        Vector3 offset = new Vector3(horizontalOffset, verticalOffset, 0);

        ChatBubble bubble = Instantiate(bubblePrefab, target.position + offset, Quaternion.identity);
        bubble.Initialize(target, text, offset, lifeTime);
    }
}
