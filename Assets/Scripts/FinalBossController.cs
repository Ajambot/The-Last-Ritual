using UnityEngine;

public class FinalBossController : MonoBehaviour
{
    [Header("Movement")]
    public float hoverSpeed = 1.5f;
    public float hoverRadius = 0.5f;
    public float moveSpeed = 2f;

    private Vector3 hoverOffset;
    private float hoverTime;

    [Header("Combat")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 5f;
    public float shootCooldown = 2.5f;
    private float shootTimer;

    [Header("References")]
    public GameObject player;
    private Animator animator;
    private AudioSource audioSource;     // <- Only one audio source needed

    [Header("Audio Clips")]
    public AudioClip attackSFX;
    public AudioClip hurtSFX;
    public AudioClip summonSFX;
    public AudioClip deathSFX;

    private bool isDead = false;

    void Start()
    {
        hoverOffset = transform.position;
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();

        if (audioSource == null)
            Debug.LogError("FatherMathias has no AudioSource component!");
    }

    void Update()
    {
        if (isDead) return;

        HoverMovement();
        HandleShooting();
    }


    // ----------------------------------------------------------
    // MOVEMENT: Floating hover motion
    // ----------------------------------------------------------
    private void HoverMovement()
    {
        animator.SetBool("isRunning", true);

        hoverTime += Time.deltaTime * hoverSpeed;

        Vector3 offset = new Vector3(
            Mathf.Sin(hoverTime) * hoverRadius,
            Mathf.Cos(hoverTime * 1.3f) * hoverRadius,
            0f
        );

        transform.position = hoverOffset + offset;
    }


    // ----------------------------------------------------------
    // SHOOTING BEHAVIOUR
    // ----------------------------------------------------------
    private void HandleShooting()
    {
        shootTimer -= Time.deltaTime;

        if (shootTimer <= 0)
        {
            ShootProjectile();
            shootTimer = shootCooldown;
        }
    }

    private void ShootProjectile()
    {
        animator.SetTrigger("Attack");

        // Play attack sound through the shared audio source
        if (attackSFX != null)
            audioSource.PlayOneShot(attackSFX);

        if (projectilePrefab == null || player == null) return;

        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        Transform playerTransform = player.GetComponent<Transform>();

        Vector2 dir = (playerTransform.position - transform.position).normalized;

        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = dir * projectileSpeed;
    }


    // ----------------------------------------------------------
    // SUMMON (GameManager calls this)
    // ----------------------------------------------------------
    public void Summon()
    {
        if (isDead) return;

        animator.SetTrigger("Summon");

        if (summonSFX != null)
            audioSource.PlayOneShot(summonSFX);
    }


    // ----------------------------------------------------------
    // DAMAGE & STAGGER
    // ----------------------------------------------------------
    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        animator.SetTrigger("Hurt");

        if (hurtSFX != null)
            audioSource.PlayOneShot(hurtSFX);
    }


    // ----------------------------------------------------------
    // DEATH (Only triggered by timer)
    // ----------------------------------------------------------
    public void KillFromTimer()
    {
        if (isDead) return;

        isDead = true;
        animator.SetTrigger("Die");

        if (deathSFX != null)
            audioSource.PlayOneShot(deathSFX);
    }
}