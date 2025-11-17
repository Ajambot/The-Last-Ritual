using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

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

    public AudioSource Music;
    public AudioClip FinalBossMusic;
    public GameObject fatherMathias;

    public TextMeshProUGUI FatherMathiasText;
    float endTime;

    // ✅ NEW METHOD
    public void StartFinalRitual()
    {
        // optional safety so the ritual can’t be restarted
        if (IsRunning || IsFinished) return;
        FinalRitual = true;
        fatherMathias.SetActive(true);
        FatherMathiasText.text = "You cannot stop me, Lucien. I am the keeper of death!";
        Music.clip = FinalBossMusic;
        Music.loop = true;
        Music.Play();
        Invoke(nameof(ClearLine), 5.0f);
    }

    private void ClearLine()
    {
        FatherMathiasText.text = "";
    }

    void Start()
    {
        if (timerText != null)
        {
            if (hideWhenNotRunning)
                timerText.enabled = false;
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
                IsRunning = true;
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
                IsRunning = false;
                IsFinished = true;
                RemainingTime = 0f;
                FinalBossController fbc = fatherMathias.GetComponent<FinalBossController>();
                fbc.KillFromTimer();
                FatherMathiasText.text = "The light… I see it now…";
                Invoke(nameof(EndScene), 5f);
                GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag("Skeleton");

                // Loop through each found GameObject
                foreach (GameObject obj in taggedObjects)
                {
                    EnemyPatrolAndAttack epaa = obj.GetComponent<EnemyPatrolAndAttack>();
                    epaa.Die();
                }

                if (timerText != null)
                {
                    timerText.text = FormatTime(0f);
                    if (hideWhenNotRunning)
                        timerText.enabled = false;
                }
            }
        }
    }

    void EndScene()
    {
        SceneManager.LoadScene("Ending Scene");
    }

    string FormatTime(float t)
    {
        int totalSeconds = Mathf.CeilToInt(t);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return $"{minutes:0}:{seconds:00}";
    }
}
