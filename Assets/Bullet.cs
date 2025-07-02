using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifetime = 3f;
    public int damage = 1;
    
    private GameObject shooter;
    
    void Start()
    {
        Destroy(gameObject, lifetime);
    }
    
    public void SetShooter(GameObject shooterTank)
    {
        shooter = shooterTank;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Ignorar colisión con el tanque que disparó
        if (other.gameObject == shooter)
            return;
            
        // Si la bala choca con un tanque
        if (other.CompareTag("Tank"))
        {
            TankHealth tankHealth = other.GetComponent<TankHealth>();
            if (tankHealth != null)
            {
                tankHealth.TakeDamage(damage);
            }
            
            Destroy(gameObject);
        }
        // Si la bala choca con un obstáculo
        else if (other.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
        // Si la bala choca con las paredes del mapa
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Manejar colisiones sólidas (paredes, obstáculos)
        if (collision.gameObject.CompareTag("Wall") || 
            collision.gameObject.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}