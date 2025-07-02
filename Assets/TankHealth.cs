using UnityEngine;
using UnityEngine.UI;

public class TankHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    private int currentHealth;
    
    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color damagedColor = Color.red;
    public float damageFlashDuration = 0.3f;
    
    [Header("UI")]
    public Text healthText; // Opcional: mostrar vida en UI
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isFlashing = false;
    
    // Eventos
    public System.Action<int, int> OnHealthChanged; // current, max
    public System.Action<GameObject> OnTankDestroyed;
    
    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>(); // Buscar en hijos también
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        UpdateHealthUI();
    }
    
    public void TakeDamage(int damage = 1)
    {
        if (currentHealth <= 0) return; // Ya está muerto
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth); // No bajar de 0
        
        // Feedback visual
        if (!isFlashing)
        {
            StartCoroutine(DamageFlash());
        }
        
        // Actualizar UI
        UpdateHealthUI();
        
        // Disparar evento
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        Debug.Log($"{gameObject.name} recibió daño. Vida: {currentHealth}/{maxHealth}");
        
        // Verificar si murió
        if (currentHealth <= 0)
        {
            DestroyTank();
        }
    }
    
    System.Collections.IEnumerator DamageFlash()
    {
        if (spriteRenderer == null) yield break;
        
        isFlashing = true;
        
        // Flash rojo
        spriteRenderer.color = damagedColor;
        yield return new WaitForSeconds(damageFlashDuration);
        
        // Volver al color original
        spriteRenderer.color = originalColor;
        
        isFlashing = false;
    }
    
    void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = $"Vida: {currentHealth}/{maxHealth}";
        }
    }
    
    void DestroyTank()
    {
        Debug.Log($"{gameObject.name} ha sido destruido!");
        
        // Disparar evento de destrucción
        OnTankDestroyed?.Invoke(gameObject);
        
        // Aquí puedes agregar efectos de explosión, sonidos, etc.
        
        // Destruir el tanque (o desactivarlo para respawn)
        gameObject.SetActive(false);
        // O usar: Destroy(gameObject);
    }
    
    public void Heal(int healAmount)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Min(maxHealth, currentHealth); // No superar el máximo
        
        UpdateHealthUI();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        Debug.Log($"{gameObject.name} se curó. Vida: {currentHealth}/{maxHealth}");
    }
    
    public bool IsAlive()
    {
        return currentHealth > 0;
    }
    
    public float GetHealthPercentage()
    {
        return (float)currentHealth / (float)maxHealth;
    }
}