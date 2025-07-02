using UnityEngine;

public class TankController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 180f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.5f;
    private float nextFireTime = 0f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
    }

    void Update()
    {
        HandleMovement();
        HandleShooting();
    }

    void HandleMovement()
    {
        float moveInput = 0;
        float rotateInput = 0;

        // Controles con I,J,K,L para jugador 2
        if (Input.GetKey(KeyCode.I)) moveInput = 1;      // Adelante
        if (Input.GetKey(KeyCode.K)) moveInput = -1;     // Atrás  
        if (Input.GetKey(KeyCode.L)) rotateInput = 1;    
        if (Input.GetKey(KeyCode.J)) rotateInput = -1;   

        Vector2 moveDirection = transform.up * moveInput * moveSpeed;
        rb.linearVelocity = moveDirection;

        float rotation = -rotateInput * rotationSpeed * Time.deltaTime;
        transform.Rotate(0, 0, rotation);
    }

    void HandleShooting()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        bulletRb.linearVelocity = firePoint.up * 10f;

        // Decirle a la bala quién la disparó
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.SetShooter(gameObject);
    }
}