using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 10;
    public float lifeTime = 5f;

    // Bounds
    private readonly float minX = 15f;
    private readonly float maxX = 43f;
    private readonly float minY = 5f;
    private readonly float maxY = 20f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        RotateTowardsVelocity();
        CheckBounds();
    }

    // ------------------------------------------------------------
    // ROTATION
    // ------------------------------------------------------------
    void RotateTowardsVelocity()
    {
        if (rb != null && rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    // ------------------------------------------------------------
    // BOUNDS CHECK
    // ------------------------------------------------------------
    void CheckBounds()
    {
        Vector3 p = transform.position;
        Debug.Log(p);
        if (p.x < minX || p.x > maxX || p.y < minY || p.y > maxY)
        {
            Debug.Log("No projectile");
            Destroy(gameObject);
        }
    }

    // ------------------------------------------------------------
    // COLLISION HANDLING
    // ------------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other.gameObject);
    }

    void HandleHit(GameObject obj)
    {
        if (obj.CompareTag("Player"))
        {
            // 🔥 Direct call to your player's script
            PlayerController player = obj.GetComponent<PlayerController>();

            if (player != null)
            {
                player.Damage(damage);
            }
            Destroy(gameObject);
        }

    }
}
