using UnityEngine;

public class TestEnemy : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    
    private float currentHealth;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    
    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        Debug.Log($"🎯 Enemy spawned with {maxHealth} HP");
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);
        
        Debug.Log($"💔 Enemy hit! Damage: {damage} | Health: {currentHealth}/{maxHealth}");
        
        // Visual feedback - flash red
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashRed());
        }
        
        // Check if dead
        if (currentHealth <= 0f)
        {
            Die();
        }
    }
    
    private System.Collections.IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }
    
    private void Die()
    {
        Debug.Log("💀 Enemy destroyed!");
        Destroy(gameObject);
    }
    
    // Public property to check if enemy is alive
    public bool IsAlive => currentHealth > 0f;
    public float CurrentHealth => currentHealth;
}
