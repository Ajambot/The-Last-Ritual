using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

[System.Serializable]
public class DialogueLine
{
    [TextArea(1,4)]
    public string text;
    [Tooltip("Visible time after fade-in (seconds)")]
    public float visibleTime = 2.5f;
    public bool isMathiasTalking = false;
}

public class Act2Dialogue : MonoBehaviour
{
    public AudioClip LucianTalk;
    public AudioClip MathiasTalk;
    public TextMeshProUGUI Text;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private AudioSource audioSource;     // <- Only one audio source needed

    public List<DialogueLine> lines = new List<DialogueLine>();

    private bool hasStartedCinematic = false;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasStartedCinematic) return;
        if (other.CompareTag("Player"))
        {
            hasStartedCinematic = true;
            StartCoroutine(RunCinematic());
        }
    }

    IEnumerator RunCinematic()
    {
        // sequence through lines
        foreach (var line in lines)
        {
            Text.text = line.text;
            audioSource.PlayOneShot(line.isMathiasTalking ? MathiasTalk : LucianTalk);
            Text.color = line.isMathiasTalking ? Color.red : Color.white;
            yield return new WaitForSeconds(line.visibleTime);
        }

        Animator anim = GetComponent<Animator>();
        if (anim)
        {
            anim.SetTrigger("Die");
        }
        Text.text = "";
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}