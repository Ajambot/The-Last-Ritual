using UnityEngine;
using TMPro;
using System.Collections;

public class RelicPickup : MonoBehaviour
{
    [Header("Relic info")]
    public string relicName = "Salt";

    [Header("UI message")]
    public TextMeshProUGUI pickupText;   // assign PickupMessage here
    public float messageDuration = 2f;   // seconds on screen

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Show message on screen
        if (pickupText != null)
        {
            pickupText.text = relicName + " received";
            pickupText.gameObject.SetActive(true);

            // start a timer to hide it
            pickupText.StopAllCoroutines();          // just in case
            pickupText.StartCoroutine(HideMessage());
        }

        // TODO: add to inventory later if you want

        Destroy(gameObject);   // remove relic from world
    }

    private IEnumerator HideMessage()
    {
        yield return new WaitForSeconds(messageDuration);
        pickupText.gameObject.SetActive(false);
    }
}
