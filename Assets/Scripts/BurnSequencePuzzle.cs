using System.Collections;
using TMPro;
using UnityEngine;

public class BurnSequencePuzzle : MonoBehaviour
{
    public static BurnSequencePuzzle Instance { get; private set; }

    [Header("Door to open when solved")]
    public DoorController door;

    [Header("UI message when door opens")]
    public TextMeshProUGUI doorMessageText;   // drag DoorMessageText here
    public float messageDuration = 2f;

    [Header("Required sequence of logIds")]
    // For “middle → left → middle”, set to {1, 0, 1}
    public int[] required = new int[] { 1, 0, 1 };

    [Header("Spawn config")]
    public WoodBurnable woodPrefab;   // Prefab of a single log with WoodBurnable
    public Transform leftSpawn;       // world position for Left (id 0)
    public Transform middleSpawn;     // world position for Middle (id 1)
    public Transform rightSpawn;      // world position for Right (id 2)
    public float respawnDelay = 0.75f;

    [Header("(Optional) If you already placed scene logs, assign them here")]
    public WoodBurnable left;         // logId 0
    public WoodBurnable middle;       // logId 1
    public WoodBurnable right;        // logId 2

    int _progress = 0;
    bool _solved = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // If logs weren’t placed in scene, spawn them from the prefab.
        if (left == null || middle == null || right == null)
        {
            RespawnAllImmediately();
        }
        else
        {
            // Ensure ids and wiring are correct for scene-placed logs.
            Wire(left, 0);
            Wire(middle, 1);
            Wire(right, 2);

            // If spawn points not set, use current positions as spawns.
            if (!leftSpawn) leftSpawn = MakeTempSpawn("LeftSpawn", left.transform.position);
            if (!middleSpawn) middleSpawn = MakeTempSpawn("MiddleSpawn", middle.transform.position);
            if (!rightSpawn) rightSpawn = MakeTempSpawn("RightSpawn", right.transform.position);
        }
    }

    Transform MakeTempSpawn(string name, Vector3 pos)
    {
        var go = new GameObject(name);
        go.transform.position = pos;
        return go.transform;
    }

    void Wire(WoodBurnable wb, int id)
    {
        if (!wb) return;
        wb.logId = id;
        // nothing else needed; WoodBurnable already notifies us via Instance.OnLogBurned(logId)
    }

    /// <summary>
    /// Called by WoodBurnable when a log finishes burning (after its burnSeconds).
    /// </summary>
    public void OnLogBurned(int id)
    {
        if (_solved) return; // ignore once solved
        if (_progress >= required.Length) return;

        if (id == required[_progress])
        {
            _progress++;

            if (_progress == required.Length)
            {
                _solved = true;
                if (door != null)
                {
                    // If your API is OpenDoor(), change this call.
                     door.OpenDoor();
                    ShowDoorMessage();   // ⬅️ NEW


                }
            }
        }
        else
        {
            // Wrong log: reset sequence and respawn all logs.
            _progress = 0;
            StartCoroutine(ResetAndRespawn());
        }
        void ShowDoorMessage()
        {
            if (doorMessageText == null) return;

            doorMessageText.text = "The door is now open.";
            doorMessageText.gameObject.SetActive(true);

            // restart coroutine if triggered again for some reason
            StopAllCoroutines();
            StartCoroutine(HideDoorMessageAfterDelay());
        }
        IEnumerator HideDoorMessageAfterDelay()
        {
            yield return new WaitForSeconds(messageDuration);
            if (doorMessageText != null)
                doorMessageText.gameObject.SetActive(false);
        }
    }

    IEnumerator ResetAndRespawn()
    {
        yield return new WaitForSeconds(respawnDelay);
        RespawnAllImmediately();
    }

    void RespawnAllImmediately()
    {
        // Clean up any existing (half-burned) logs first
        if (left) Destroy(left.gameObject);
        if (middle) Destroy(middle.gameObject);
        if (right) Destroy(right.gameObject);

        // Need a prefab and spawn points
        if (!woodPrefab)
        {
            Debug.LogError("[BurnSequencePuzzle] Missing woodPrefab.", this);
            return;
        }
        if (!leftSpawn || !middleSpawn || !rightSpawn)
        {
            Debug.LogError("[BurnSequencePuzzle] Missing spawn transforms.", this);
            return;
        }

        // Spawn fresh logs
        left = Instantiate(woodPrefab, leftSpawn.position, Quaternion.identity);
        middle = Instantiate(woodPrefab, middleSpawn.position, Quaternion.identity);
        right = Instantiate(woodPrefab, rightSpawn.position, Quaternion.identity);

        // Set ids and wire
        Wire(left, 0);
        Wire(middle, 1);
        Wire(right, 2);
    }
}
