using UnityEngine;

public class AlinaTalk : MonoBehaviour
{
    [Header("Script")]
    public AudioClip Talk;
    public GameObject Line; // TextMeshProUGUI GameObject (with TextFader)
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private AudioSource audioSource;     // <- Only one audio source needed
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Line.SetActive(true);
            audioSource.PlayOneShot(Talk);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Line.SetActive(false);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
