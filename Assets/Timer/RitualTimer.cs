using UnityEngine;
using TMPro;

public class RitualTimer : MonoBehaviour
{
    [Header("Ritual settings")]
    public float ritualDuration = 60f;   // total duration in seconds

    [Header("Trigger")]
    public bool FinalRitual = false;     // set this true from your ritual script

    [Header("UI")]
    public TMP_Text timerText;           // TextMeshProUGUI in the top-right
    public bool hideWhenNotRunning = true;

    public bool IsRunning { get; private set; }
    public bool IsFinished { get; private set; }
    public float RemainingTime { get; private set; }

    float endTime;

    void Start()
    {
        if (timerText != null)
        {
            if (hideWhenNotRunning)
                timerText.enabled = false;          // disable ONLY the text component
            else
                timerText.text = FormatTime(ritualDuration);
        }
    }

    void Update()
    {
        // not started yet: wait for FinalRitual
        if (!IsRunning && !IsFinished)
        {
            if (FinalRitual)
            {
                IsRunning  = true;
                IsFinished = false;
                endTime = Time.time + ritualDuration;

                if (timerText != null && hideWhenNotRunning)
                    timerText.enabled = true;
            }
            return;
        }

        // running
        if (IsRunning && !IsFinished)
        {
            RemainingTime = Mathf.Max(0f, endTime - Time.time);

            if (timerText != null)
                timerText.text = FormatTime(RemainingTime);

            if (RemainingTime <= 0f)
            {
                IsRunning  = false;
                IsFinished = true;
                RemainingTime = 0f;

                if (timerText != null)
                {
                    timerText.text = FormatTime(0f);
                    if (hideWhenNotRunning)
                        timerText.enabled = false;
                }
            }
        }
    }

    string FormatTime(float t)
    {
        int totalSeconds = Mathf.CeilToInt(t);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return $"{minutes:0}:{seconds:00}";
    }
}
