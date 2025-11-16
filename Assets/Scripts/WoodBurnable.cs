using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class WoodBurnable : MonoBehaviour
{
    [Header("Interact")]
    public KeyCode interactKey = KeyCode.E;

    [Header("Fire FX")]
    public GameObject firePrefab;      // Prefab with SpriteRenderer + Animator (plays once)
    public float burnSeconds = 1.2f;   // How long the fire stays before wood is removed
    public float fireScale = 1f;       // Scale for the spawned fire effect

    [Header("Optional: Chain burn")]
    public float chainRadius = 0f;     // >0 ignites nearby WoodBurnable automatically

    [Header("Puzzle Id (set per log)")]
    [Tooltip("0 = left, 1 = middle, 2 = right (or any custom id)")]
    public int logId = 0;

    // Events so puzzle/other systems can listen
    public Action<int> OnBurn;                 // Notifies with logId
    public Action<WoodBurnable> OnBurned;      // Notifies with this instance

    bool _playerInside;
    bool _burning;
    SpriteRenderer _sr;
    Collider2D _col;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _col = GetComponent<Collider2D>();
        _col.isTrigger = true; // ensure trigger
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) _playerInside = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) _playerInside = false;
    }

    void Update()
    {
        if (_playerInside && !_burning && Input.GetKeyDown(interactKey))
            StartCoroutine(BurnRoutine());
    }

    public void TriggerBurn()
    {
        if (!_burning) StartCoroutine(BurnRoutine());
    }

    IEnumerator BurnRoutine()
    {
        _burning = true;

        // Hide & stop retriggering
        if (_col) _col.enabled = false;
        if (_sr) _sr.enabled = false;

        // Spawn fire FX
        GameObject fire = null;
        if (firePrefab)
        {
            fire = Instantiate(firePrefab, transform.position, Quaternion.identity);
            fire.transform.localScale = Vector3.one * fireScale;

            // Match sorting so it renders where wood was
            var fireSR = fire.GetComponent<SpriteRenderer>();
            if (fireSR && _sr)
            {
                fireSR.sortingLayerID = _sr.sortingLayerID;
                fireSR.sortingOrder = _sr.sortingOrder;
            }
        }

        // Optional chain burn
        if (chainRadius > 0f)
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, chainRadius);
            foreach (var h in hits)
            {
                if (h && h.gameObject != gameObject)
                {
                    var other = h.GetComponent<WoodBurnable>();
                    if (other && !other._burning) other.TriggerBurn();
                }
            }
        }

        yield return new WaitForSeconds(burnSeconds);

        if (fire) Destroy(fire);

        // Notify puzzle listeners that THIS log burned
        BurnSequencePuzzle.Instance?.OnLogBurned(logId);
        OnBurn?.Invoke(logId);
        OnBurned?.Invoke(this);

        Destroy(gameObject); // remove the wood
    }

#if UNITY_EDITOR
    // Visualize chain radius
    void OnDrawGizmosSelected()
    {
        if (chainRadius > 0f)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.25f);
            Gizmos.DrawWireSphere(transform.position, chainRadius);
        }
    }
#endif
}
