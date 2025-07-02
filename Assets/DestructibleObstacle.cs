using UnityEngine;

public class DestructibleObstacle : MonoBehaviour
{
    [Header("Destructible Settings")]
    public int maxHealth = 5;
    private int currentHealth;
    
    [Header("Visual Feedback")]
    public Color normalColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    public Color damagedColor = Color.red;
    
    private SpriteRenderer spriteRenderer;
    private ObstacleGenerator obstacleGenerator;
    private bool isDestroyed = false;
    
    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        obstacleGenerator = FindObjectOfType<ObstacleGenerator>();
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }
    }
    
    public void TakeDamage(int damage = 1)
    {
        if (isDestroyed) return;
        
        currentHealth -= damage;
        UpdateVisualFeedback();
        
        if (currentHealth <= 0)
        {
            DestroyObstacle();
        }
        
        Debug.Log($"Obstáculo recibió daño. Vida restante: {currentHealth}/{maxHealth}");
    }
    
    void UpdateVisualFeedback()
    {
        if (spriteRenderer == null) return;
        
        float damagePercent = 1f - ((float)currentHealth / (float)maxHealth);
        Color currentColor = Color.Lerp(normalColor, damagedColor, damagePercent);
        spriteRenderer.color = currentColor;
    }
    
    void DestroyObstacle()
    {
        if (isDestroyed) return;
        
        isDestroyed = true;
        
        if (obstacleGenerator != null)
        {
            obstacleGenerator.ReplaceObstacle(gameObject);
        }
        
        Destroy(gameObject);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDestroyed) return;
        
        if (other.CompareTag("Bullet"))
        {
            Bullet bulletScript = other.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                TakeDamage(1);
            }
        }
    }
}