using UnityEngine;
using System.Collections.Generic;

public class ObstacleGenerator : MonoBehaviour
{
    [Header("Obstacle Settings")]
    public GameObject obstaclePrefab;
    public Sprite[] obstacleSprites;
    public int obstacleCount = 5;
    public Vector2 mapSize = new Vector2(15, 10);
    public float minDistanceFromTank = 3f;
    public Vector2 obstacleSize = new Vector2(1.5f, 1.5f);
    
    [Header("Obstacle Appearance")]
    public Color obstacleGrayTint = new Color(0.7f, 0.7f, 0.7f, 1f);
    
    [Header("Collider Settings")]
    public bool usePolygonCollider = true; // Usar PolygonCollider2D para mejor ajuste
    public float colliderSimplification = 0.05f; // Simplificar el collider para mejor rendimiento
    
    public Transform tankTransform;
    
    private List<GameObject> activeObstacles = new List<GameObject>();
    
    void Start()
    {
        // Obtener el tamaño dinámico del mapa
        MapManager mapManager = FindObjectOfType<MapManager>();
        if (mapManager != null)
        {
            mapSize = mapManager.GetMapSize();
        }
        
        GenerateObstacles();
    }
    
    void GenerateObstacles()
    {
        for (int i = 0; i < obstacleCount; i++)
        {
            CreateSingleObstacle();
        }
    }
    
    void CreateSingleObstacle()
    {
        Vector2 randomPosition = FindValidPosition();
        
        GameObject obstacle = Instantiate(obstaclePrefab, randomPosition, Quaternion.identity);
        AssignRandomObstacleSprite(obstacle);
        activeObstacles.Add(obstacle);
    }
    
    Vector2 FindValidPosition()
    {
        Vector2 randomPosition;
        int attempts = 0;
        
        do
        {
            randomPosition = new Vector2(
                Random.Range(-mapSize.x/2, mapSize.x/2),
                Random.Range(-mapSize.y/2, mapSize.y/2)
            );
            attempts++;
        }
        while (IsPositionTooClose(randomPosition) && attempts < 50);
        
        return randomPosition;
    }
    
    bool IsPositionTooClose(Vector2 position)
    {
        if (tankTransform != null && Vector2.Distance(position, tankTransform.position) < minDistanceFromTank)
            return true;
            
        foreach (GameObject obstacle in activeObstacles)
        {
            if (obstacle != null && Vector2.Distance(position, obstacle.transform.position) < 2f)
                return true;
        }
        
        return false;
    }
    
    public void ReplaceObstacle(GameObject destroyedObstacle)
    {
        activeObstacles.Remove(destroyedObstacle);
        CreateSingleObstacle();
        Debug.Log("Obstáculo destruido y reemplazado en nueva posición");
    }
    
    void AssignRandomObstacleSprite(GameObject obstacle)
    {
        if (obstacleSprites.Length == 0)
        {
            Debug.LogWarning("No hay sprites de obstáculos asignados!");
            return;
        }
        
        int randomIndex = Random.Range(0, obstacleSprites.Length);
        
        SpriteRenderer spriteRenderer = obstacle.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = obstacleSprites[randomIndex];
            spriteRenderer.color = obstacleGrayTint;
            
            if (obstacleSprites[randomIndex] != null)
            {
                Vector2 spriteSize = obstacleSprites[randomIndex].bounds.size;
                float scaleX = obstacleSize.x / spriteSize.x;
                float scaleY = obstacleSize.y / spriteSize.y;
                
                float uniformScale = Mathf.Min(scaleX, scaleY);
                obstacle.transform.localScale = new Vector3(uniformScale, uniformScale, 1f);
                
                // Crear colliders adaptados al sprite
                CreateAdaptiveColliders(obstacle, obstacleSprites[randomIndex]);
            }
        }
        
        if (obstacle.GetComponent<DestructibleObstacle>() == null)
        {
            obstacle.AddComponent<DestructibleObstacle>();
        }
    }
    
    void CreateAdaptiveColliders(GameObject obstacle, Sprite sprite)
    {
        // Eliminar colliders existentes
        BoxCollider2D[] boxColliders = obstacle.GetComponents<BoxCollider2D>();
        for (int i = 0; i < boxColliders.Length; i++)
        {
            DestroyImmediate(boxColliders[i]);
        }
        
        PolygonCollider2D[] polygonColliders = obstacle.GetComponents<PolygonCollider2D>();
        for (int i = 0; i < polygonColliders.Length; i++)
        {
            DestroyImmediate(polygonColliders[i]);
        }
        
        if (usePolygonCollider)
        {
            // Crear PolygonCollider2D que se adapta perfectamente al sprite
            
            // Collider sólido para físicas (tanques chocan)
            PolygonCollider2D solidCollider = obstacle.AddComponent<PolygonCollider2D>();
            solidCollider.isTrigger = false;
            
            // Simplificar el collider para mejor rendimiento
            if (colliderSimplification > 0)
            {
                // Unity automáticamente genera el polygon basado en el sprite
                // Podemos ajustar la densidad de puntos si es necesario
            }
            
            // Collider trigger para detectar balas (copia del sólido)
            PolygonCollider2D triggerCollider = obstacle.AddComponent<PolygonCollider2D>();
            triggerCollider.isTrigger = true;
            
            // Copiar los puntos del collider sólido al trigger
            triggerCollider.points = solidCollider.points;
        }
        else
        {
            // Usar BoxCollider2D como respaldo
            BoxCollider2D solidCollider = obstacle.AddComponent<BoxCollider2D>();
            solidCollider.isTrigger = false;
            
            BoxCollider2D triggerCollider = obstacle.AddComponent<BoxCollider2D>();
            triggerCollider.isTrigger = true;
            
            // Ajustar el tamaño basado en el sprite
            Vector2 spriteSize = sprite.bounds.size;
            solidCollider.size = spriteSize;
            triggerCollider.size = spriteSize;
        }
        
        Debug.Log($"Colliders adaptativos creados para sprite: {sprite.name}");
    }
}