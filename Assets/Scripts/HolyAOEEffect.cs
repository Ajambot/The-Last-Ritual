using UnityEngine;

public class HolyAOEEffect : MonoBehaviour
{
    [Header("Effect Settings")]
    [SerializeField] private float lifetime = 1.5f;
    [SerializeField] private float expandSpeed = 3f;
    [SerializeField] private float maxScale = 5f;
    
    private SpriteRenderer spriteRenderer;
    private Light lightComponent;
    private float elapsedTime = 0f;
    private Vector3 startScale;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        lightComponent = GetComponent<Light>();
        
        startScale = transform.localScale;
        
        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);
        
        Debug.Log("✨ Holy AOE effect spawned!");
    }
    
    void Update()
    {
        elapsedTime += Time.deltaTime;
        float progress = elapsedTime / lifetime;
        
        // Expand effect
        float scale = Mathf.Lerp(0f, maxScale, progress * expandSpeed);
        transform.localScale = startScale * scale;
        
        // Fade out
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = Mathf.Lerp(1f, 0f, progress);
            spriteRenderer.color = color;
        }
        
        // Fade light
        if (lightComponent != null)
        {
            lightComponent.intensity = Mathf.Lerp(2f, 0f, progress);
        }
    }
}
