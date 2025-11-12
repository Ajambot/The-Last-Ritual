using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class CinematicLine
{
    [TextArea(1,4)]
    public string text;
    [Tooltip("Visible time after fade-in (seconds)")]
    public float visibleTime = 2.5f;
    [Tooltip("Optional: play a SFX at start of this line")]
    public AudioClip sfx;
    [Tooltip("Optional: start or stop chant/ambience")]
    public bool startAmbience;
    public bool stopAmbience;
}

public class CinematicController : MonoBehaviour
{
    [Header("UI")]
    public GameObject cinematicTextObject; // TextMeshProUGUI GameObject (with TextFader)
    private TextFader textFader;

    [Header("Audio")]
    public AudioSource ambienceSource;   // rain loop
    public AudioSource chantSource;      // chant loop or drone
    public AudioSource sfxSource;        // for one-shots
    [Range(0,1)] public float ambienceVolume = 0.65f;
    [Range(0,1)] public float chantVolume = 0.4f;

    [Header("Sequence")]
    public List<CinematicLine> lines = new List<CinematicLine>();

    [Header("Flicker")]
    public UnityEngine.UI.Image flickerOverlay;
    public float flickerMin = 0.02f;
    public float flickerMax = 0.18f;

    void Awake()
    {
        if (cinematicTextObject == null) Debug.LogError("Assign cinematicTextObject in inspector");
        textFader = cinematicTextObject.GetComponent<TextFader>();
        if (textFader == null) Debug.LogError("TextFader missing on cinematicTextObject");
    }

    void Start()
    {
        // ensure audio sources start silent
        if (ambienceSource) ambienceSource.volume = 0f;
        if (chantSource) chantSource.volume = 0f;

        StartCoroutine(RunCinematic());
        if (flickerOverlay) flickerOverlay.color = new Color(1,1,1,0);
    }

    IEnumerator RunCinematic()
    {
        // Example: initial rain starts right away
        if (ambienceSource)
        {
            ambienceSource.Play();
            StartCoroutine(FadeAudio(ambienceSource, 0.5f, ambienceVolume, 1f)); // fade up
        }

        yield return new WaitForSeconds(0.2f);

        // sequence through lines
        foreach (var line in lines)
        {
            if (line.startAmbience && chantSource)
            {
                chantSource.Play();
                StartCoroutine(FadeAudio(chantSource, 0.6f, chantVolume, 0.6f));
            }
            if (line.stopAmbience && chantSource)
            {
                StartCoroutine(FadeAudio(chantSource, chantSource.volume, 0f, 0.6f));
            }
            if (line.sfx != null && sfxSource)
            {
                sfxSource.PlayOneShot(line.sfx);
            }

            // flicker effect small alpha pulse
            if (flickerOverlay)
                StartCoroutine(FlickerPulse(0.12f));

            yield return StartCoroutine(textFader.ShowFor(line.text, line.visibleTime));
        }

        // final title - example
        yield return new WaitForSeconds(0.3f);

        // stop ambience/chant gently
        if (chantSource) StartCoroutine(FadeAudio(chantSource, chantSource.volume, 0f, 0.8f));
        if (ambienceSource) StartCoroutine(FadeAudio(ambienceSource, ambienceSource.volume, 0f, 1.2f));

        // optional play title SFX (assign last line.sfx in inspector)
        yield return new WaitForSeconds(0.7f);

        // (At this point, you can enable game UI or load first level)
        Debug.Log("Cinematic finished.");
    }

    IEnumerator FadeAudio(AudioSource source, float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        source.volume = to;
        if (to <= 0f) source.Stop();
        yield return null;
    }

    IEnumerator FlickerPulse(float maxAlpha)
    {
        // tiny flicker pulse
        if (flickerOverlay == null) yield break;
        float t = 0f;
        float dur = 0.12f;
        // fade in
        while (t < dur)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0f, maxAlpha, t / dur);
            flickerOverlay.color = new Color(1,1,1,a);
            yield return null;
        }
        // fade out
        t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(maxAlpha, 0f, t / dur);
            flickerOverlay.color = new Color(1,1,1,a);
            yield return null;
        }
        flickerOverlay.color = new Color(1,1,1,0f);
    }
}
