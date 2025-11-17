using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class EnemyPatrolAndAttack : MonoBehaviour
{
    //[Header("Movement Mode")]
    public enum MovementMode { Stationary, Patrolling, Tracking }
    public MovementMode movementMode = MovementMode.Patrolling;

    [Header("Patrol (offsets from start X)")]
    public float patrolLeftOffset = 2f;   // how far left from spawn
    public float patrolRightOffset = 2f;  // how far right from spawn
    public float moveSpeed = 2f;
    public bool keepChasingOnceSeen = true;

    [Header("Detection")]
    public Transform player;
    public float detectionRange = 5f;
    public float attackRange = 1f;

    [Header("Health")]
    public int maxHealth = 3;

    [Header("Attack Hitbox")]
    public Collider2D swordHitbox;
    public int swordDamage = 1;
    public int swordKnockback = 1;

    [Header("Hurt")]
    public AnimationClip hurtClip;
    bool isHurting = false;
    float hurtTimer = 0f;

    [Header("Audio")]
    public AudioClip attackSfx;
    public AudioClip hurtSfx;
    public AudioClip deathSfx;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    Rigidbody2D rb;
    Animator anim;
    AudioSource audioSource;

    float originX;
    float leftX;
    float rightX;

    bool movingRight = true;
    bool dead = false;
    int currentHealth;
    bool hasSeenPlayer = false;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }

        currentHealth = maxHealth;

        // Patrol bounds based on where the enemy spawns
        originX = transform.position.x;
        leftX  = originX - Mathf.Abs(patrolLeftOffset);
        rightX = originX + Mathf.Abs(patrolRightOffset);

        anim.SetBool("IsMoving", movementMode == MovementMode.Patrolling);

        if (!player)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
    }

    void ChaseBehaviour()
    {
        // walking animation
        anim.SetBool("IsMoving", true);

        if (!player)
        {
            IdleBehaviour();
            return;
        }

        // direction toward player (-1 = left, +1 = right)
        float dir = Mathf.Sign(player.position.x - transform.position.x);

        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);

        // face the player
        if (dir > 0f && !movingRight)
            Flip();
        else if (dir < 0f && movingRight)
            Flip();
    }


    void IdleBehaviour()
    {
        anim.SetBool("IsMoving", false);
        anim.SetBool("IsAttacking", false);
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }
    void PatrolBehaviour()
    {
        anim.SetBool("IsAttacking", false);
        anim.SetBool("IsMoving", true);

        float dir = movingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);

        if (movingRight && transform.position.x >= rightX)
            Flip();
        else if (!movingRight && transform.position.x <= leftX)
            Flip();
    }
    void HurtMovementOnly()
    {
        anim.SetBool("IsMoving", false);
        anim.SetBool("IsAttacking", false);
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    void Update()
    {
//#if UNITY_EDITOR
//    if (Input.GetKeyDown(KeyCode.H))
//    {
//        Debug.Log("Debug: enemy TakeDamage(1)");
//        TakeDamage(1);      // should play Hurt if health > 1
//    }

//    if (Input.GetKeyDown(KeyCode.K))
//    {
//        Debug.Log("Debug: enemy TakeDamage(999)");
//        TakeDamage(999);    // should go straight to Die
//    }
//#endif
        if (dead) return;
        if (isHurting)
        {
            if (hurtTimer > 0f)
            {
                hurtTimer -= Time.deltaTime;
                HurtMovementOnly();
                return;
            }
            else
            {
                isHurting = false;    // hurt anim finished, resume normal logic
            }
        }


        float dist = player ? Vector2.Distance(transform.position, player.position) : Mathf.Infinity;

        bool inAttackRange = dist <= attackRange;
        bool playerDetected;

        if (movementMode == MovementMode.Tracking)
        {
            //Boss-style: always aware if a player exists
            playerDetected = (player != null);
        }
        else
        {
            bool inDetectionRadius = dist <= detectionRange;

            if (keepChasingOnceSeen)
            {
                // Once inside detectionRange at least once, remember the player
                if (player && inDetectionRadius)
                    hasSeenPlayer = true;

                playerDetected = hasSeenPlayer && player != null;
            }
            else
            {
                playerDetected = inDetectionRadius && player != null;
                hasSeenPlayer = false;
            }
        }

        AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
        bool inHurtState = state.IsName("SwordHurt");
        bool inAttackState = state.IsName("SwordAttack1");


        // While in the attack state: don't switch to patrol/chase, just let the animation play/loop
        if (inAttackState)
        {
            AttackMovementOnly();  // stop and face player

            // Keep looping only while still close and "aware"
            bool keepAttacking = playerDetected && inAttackRange;
            anim.SetBool("IsAttacking", keepAttacking);
            return;
        }


        // Not currently in attack state: choose behaviour based on movementMode
        if (movementMode == MovementMode.Stationary)
        {
            if (playerDetected && inAttackRange)
            {
                // close enough to hit start an attack immediately
                anim.SetBool("IsAttacking", true);
                anim.Play("SwordAttack1", 0, 0f);     // jump to start of attack clip
                //PlaySfx(attackSfx);
                AttackMovementOnly();
            }
            else if (playerDetected)
            {
                // saw the player now we start moving/chasing
                anim.SetBool("IsAttacking", false);
                ChaseBehaviour();
            }
            else
            {
                // before seeing the player: stand idle
                IdleBehaviour();
            }
        }
        else if (movementMode == MovementMode.Patrolling)
        {
            if (playerDetected && inAttackRange)
            {
                anim.SetBool("IsAttacking", true);
                anim.Play("SwordAttack1", 0, 0f);
                //PlaySfx(attackSfx);
                AttackMovementOnly();
            }
            else if (playerDetected)
            {
                // sees the player but not in attack range yet chase
                anim.SetBool("IsAttacking", false);
                ChaseBehaviour();
            }
            else
            {
                // no player in detection range normal back-and-forth patrol
                anim.SetBool("IsAttacking", false);
                PatrolBehaviour();
            }
        }
        else // MovementMode.Tracking
        {
            // Tracking: always hunts player (ignores detectionRange)
            if (playerDetected && inAttackRange)
            {
                anim.SetBool("IsAttacking", true);
                anim.Play("SwordAttack1", 0, 0f);
                //PlaySfx(attackSfx);
                AttackMovementOnly();
            }
            else if (playerDetected)
            {
                anim.SetBool("IsAttacking", false);
                ChaseBehaviour();
            }
            else
            {
                // No player reference at all just idle
                IdleBehaviour();
            }
        }
    }

    void AttackMovementOnly()
    {
        anim.SetBool("IsMoving", false);
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        if (player)
        {
            if (player.position.x > transform.position.x && !movingRight)
                Flip();
            else if (player.position.x < transform.position.x && movingRight)
                Flip();
        }
    }

    void Flip()
    {
        movingRight = !movingRight;
        Vector3 s = transform.localScale;
        s.x *= -1f;
        transform.localScale = s;
    }

    public void TakeDamage(int amount)
    {
        if (dead) return;

        currentHealth -= amount;
        Debug.Log($"Enemy took {amount}, hp = {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Cancel current attack & movement
            anim.SetBool("IsAttacking", false);
            anim.SetBool("IsMoving", false);

            if (swordHitbox) swordHitbox.enabled = false;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            // Force Hurt animation and start hurt phase
            isHurting = true;

            // Play the hurt state immediately (same trick as death)
            anim.Play("SwordHurt", 0, 0f);
            PlaySfx(hurtSfx);

            // Use clip length as stun duration (fallback if not assigned)
            if (hurtClip != null)
                hurtTimer = hurtClip.length;
            else
                hurtTimer = 0.4f;  // fallback guess so it still works
        }
    }
    void PlaySfx(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip, sfxVolume);
    }
    public void PlayAttackSoundEvent()
    {
        PlaySfx(attackSfx);
    }


    public void Die()
    {
        if (dead) return;
        dead = true;

        // Force the death animation state immediately
        anim.Play("SwordDie", 0, 0f);
        anim.SetBool("Dead", true);
        anim.SetBool("IsAttacking", false);
        anim.SetBool("IsMoving", false);

        PlaySfx(deathSfx);

        // Freeze physics where he died
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        // No more hits from his weapon
        if (swordHitbox) swordHitbox.enabled = false;

        // Disable body collider so abilities don't see the corpse
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        // IMPORTANT: stop this script from running Update/LateUpdate after death
        this.enabled = false;
    }


    public void EnableSwordHitbox()
    {
        if (swordHitbox) swordHitbox.enabled = true;
    }

    public void DisableSwordHitbox()
    {
        if (swordHitbox) swordHitbox.enabled = false;
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!swordHitbox || !swordHitbox.enabled)
            return;

        if (!other.CompareTag("Player"))
            return;

        PlayerController player = other.GetComponent<PlayerController>();
        Debug.Log(player);
        if (player == null)
            player = other.GetComponentInParent<PlayerController>();

        if (player != null)
        {
            player.Damage(swordDamage);
            Debug.Log("Enemy hit player, player health = ");
        }
    }
    void LateUpdate()
    {
        //Safety: if we are not in the attack state, make sure the sword hitbox is off
        if (swordHitbox == null) return;

        var state = anim.GetCurrentAnimatorStateInfo(0);
        bool inAttack = state.IsName("SwordAttack1");

        if (!inAttack && swordHitbox.enabled)
        {
            swordHitbox.enabled = false;
        }
    }

}