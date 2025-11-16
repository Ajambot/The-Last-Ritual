using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneExit : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;   // Name of the next scene

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Make sure only the player triggers it
        if (!other.CompareTag("Player")) return;

        SceneManager.LoadScene(sceneToLoad);
    }
}
