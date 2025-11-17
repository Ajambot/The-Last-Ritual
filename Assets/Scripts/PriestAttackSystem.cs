using UnityEngine;

public class PriestAttackSystem : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float basicAttackCooldown = 0.5f;
    [SerializeField] private float chargePerAttack = 25f;
    [SerializeField] private float basicAttackDamage = 20f;
    [SerializeField] private float basicAttackRange = 2f;
    
    [Header("AOE Attack Settings")]
    [SerializeField] private float maxCharge = 100f;
    [SerializeField] private float aoeAttackDamage = 100f;
    [SerializeField] private float aoeAttackRadius = 5f;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject aoeEffectPrefab;
    
    private Animator animator;
    private float currentCharge = 0f;
    private float lastAttackTime = 0f;
    private bool isAttacking = false;

    public bool isAOEAttacking = false;
    
    // Animation parameter hashes
    private static readonly int AttackTrigger = Animator.StringToHash("attack");
    private static readonly int CastingTrigger = Animator.StringToHash("casting");
    
    public float CurrentCharge => currentCharge;
    public float MaxCharge => maxCharge;
    public bool IsChargeFull => currentCharge >= maxCharge;
    public bool IsAttacking => isAttacking;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        
        if (animator == null)
        {
            Debug.LogWarning("PriestAttackSystem: No Animator found. Animations will be skipped.");
        }
        
        Debug.Log("=== PRIEST ATTACK SYSTEM INITIALIZED ===");
        Debug.Log($"Charge per attack: {chargePerAttack}%");
        Debug.Log($"Attacks needed for full charge: {Mathf.CeilToInt(maxCharge / chargePerAttack)}");
        Debug.Log("Controls: Left Click = Basic Attack, Right Click/Space = AOE Attack");
    }
    
    void Update()
    {
        HandleAttackInput();
    }
    
    private void HandleAttackInput()
    {
        // Basic attack (Left Mouse Button)
        if (Input.GetMouseButtonDown(0) && CanPerformBasicAttack())
        {
            Invoke(nameof(PerformBasicAttack), 0.2f);
        }
        
        // AOE attack (Right Mouse Button)
        if (Input.GetMouseButtonDown(1) && CanPerformAOEAttack())
        {
            PerformAOEAttack();
        }
    }
    
    private bool CanPerformBasicAttack()
    {
        return !isAttacking && Time.time >= lastAttackTime + basicAttackCooldown;
    }
    
    private bool CanPerformAOEAttack()
    {
        return !isAttacking && IsChargeFull;
    }
    
    public void PerformBasicAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        Debug.Log("⚔️ BASIC ATTACK performed!");

        animator.SetTrigger("attack");
        
        // Deal damage to enemies in range after a short delay
        Invoke(nameof(DealBasicAttackDamage), 0.2f);
        
        // End attack after a short delay
        Invoke(nameof(EndAttack), 0.5f);
    }
    
    private void DealBasicAttackDamage()
    {
        // Find enemies in range (circular area in front of player)
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, basicAttackRange);
        
        bool hitEnemy = false;
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Skeleton"))
            {
                EnemyPatrolAndAttack enemy = hit.GetComponent<EnemyPatrolAndAttack>();
                if (enemy != null)
                {
                    enemy.TakeDamage((int)basicAttackDamage);
                    hitEnemy = true;
                }
            }
            else
            {
                TestEnemy enemy = hit.GetComponent<TestEnemy>();
                if (enemy != null && enemy.IsAlive)
                {
                    enemy.TakeDamage(basicAttackDamage);
                    hitEnemy = true;
                }
            }
        }
        
        if (hitEnemy)
        {
            // Add charge when we hit an enemy
            AddCharge(chargePerAttack);
        }
        else
        {
            Debug.Log("⚔️ Attack missed! No enemies in range.");
        }
    }
    
    
    public void PerformAOEAttack()
    {
        isAttacking = true;
        isAOEAttacking = true;

        Debug.Log("💥 AOE ATTACK ACTIVATED! Holy burst unleashed!");

        // Trigger animation if animator exists
        if (animator != null)
        {
            animator.SetTrigger(CastingTrigger);
        }

        PlayerController pc = GetComponent<PlayerController>();
        if (pc)
        {
            pc.HealToFull();
        }

        
        // Spawn visual effect
        if (aoeEffectPrefab != null)
        {
            Debug.Log("AOE Effect Instantiated");
            GameObject effect = Instantiate(aoeEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        // Deal AOE damage after a short delay
        Invoke(nameof(DealAOEDamage), 0.25f);
        
        // Reset charge
        SetCharge(0f);
        Debug.Log("Charge reset to 0%");
        
        // End attack after animation
        Invoke(nameof(EndAttack), 1.0f);
    }
    
    private void DealAOEDamage()
    {
        Debug.Log($"🌟 Holy burst searching for enemies in {aoeAttackRadius}m radius...");
        
        // Find all enemies in AOE radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, aoeAttackRadius);
        
        int enemiesHit = 0;
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Skeleton"))
            {
                EnemyPatrolAndAttack enemy = hit.GetComponent<EnemyPatrolAndAttack>();
                if (enemy != null)
                {
                    enemy.TakeDamage((int)aoeAttackDamage);
                }
            }
            else
            {
                TestEnemy enemy = hit.GetComponent<TestEnemy>();
                if (enemy != null && enemy.IsAlive)
                {
                    enemy.TakeDamage(aoeAttackDamage);
                }
            }
        }
        
        if (enemiesHit > 0)
        {
            Debug.Log($"⚡ AOE struck {enemiesHit} enemy(ies)!");
        }
        else
        {
            Debug.Log("💨 AOE attack hit no enemies.");
        }
        
    }
    
    
    private void EndAttack()
    {
        isAttacking = false;
        isAOEAttacking = false;
    }
    
    public void AddCharge(float amount)
    {
        float previousCharge = currentCharge;
        currentCharge = Mathf.Min(currentCharge + amount, maxCharge);
        
        float chargePercent = (currentCharge / maxCharge) * 100f;
        Debug.Log($"📊 Charge: {chargePercent:F0}% ({currentCharge}/{maxCharge})");
        
        // Check if charge just became full
        if (previousCharge < maxCharge && currentCharge >= maxCharge)
        {
            Debug.Log("⚡ CHARGE FULL! AOE attack ready! Press Right Click or Space to unleash!");
        }
    }
    
    // Draw gizmos to visualize attack ranges
    private void OnDrawGizmosSelected()
    {
        // Basic attack range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, basicAttackRange);
        
        // AOE attack range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, aoeAttackRadius);
    }
    
    public void SetCharge(float amount)
    {
        currentCharge = Mathf.Clamp(amount, 0f, maxCharge);
        float chargePercent = (currentCharge / maxCharge) * 100f;
        Debug.Log($"📊 Charge: {chargePercent:F0}% ({currentCharge}/{maxCharge})");
    }
    
    
}
