using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    public string[] lines;
    private int index = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (index >= lines.Length)
                index = 0;

            BubbleManager manager = FindObjectOfType<BubbleManager>();
            manager.CreateBubble(transform, lines[index]);

            index++;
        }
    }
}
