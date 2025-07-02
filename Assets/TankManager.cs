using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankManager : MonoBehaviour
{
    [Header("Tank Spawning")]
    public GameObject tankPrefab;
    public int numberOfTanks = 2;
    [Range(0.1f, 0.9f)]
    public float marginFromEdge = 0.15f;
    
    [Header("Tank Appearance")]
    public Sprite[] tankSprites;
    public float desiredTankSize = 2.0f;
    
    [Header("Shadow Settings")]
    public bool enableShadows = true;
    public float shadowRadius = 25f;
    public float shadowOffset = -2f;
    public int shadowSortingOrder = -1;
    
    [Header("Shadow Colors per Player")]
    public Color[] playerShadowColors = new Color[]
    {
        new Color(0f, 0f, 0f, 0.5f),      // Negro semi-transparente
        new Color(0.2f, 0.2f, 0.2f, 0.5f), // Gris oscuro
        new Color(0.1f, 0.1f, 0.3f, 0.5f), // Azul oscuro
        new Color(0.3f, 0.1f, 0.1f, 0.5f)  // Rojo oscuro
    };

    void Start()
    {
        SpawnTanks();
    }

    void SpawnTanks()
    {
        Debug.Log($"=== INICIANDO SPAWN DE {numberOfTanks} TANQUES ===");
        
        if (tankPrefab == null)
        {
            Debug.LogError("Tank Prefab es NULL! Asigna el prefab en el inspector.");
            return;
        }
        
        Vector3[] spawnPositions = CalculateSpawnPositions(numberOfTanks);
        Debug.Log($"Posiciones calculadas: {spawnPositions.Length}");
        
        for (int i = 0; i < numberOfTanks; i++)
        {
            Vector3 spawnPos = spawnPositions[i];
            Debug.Log($"Creando tanque {i+1} en posición: {spawnPos}");
            
            GameObject tank = Instantiate(tankPrefab, spawnPos, Quaternion.identity);
            tank.name = $"Tank_{i + 1}";
            
            Debug.Log($"Tanque {tank.name} creado exitosamente");
            
            // Asignar apariencia y sombra
            AssignTankAppearance(tank, i);
        }
        
        Debug.Log("=== SPAWN COMPLETADO ===");
    }

    Vector3[] CalculateSpawnPositions(int playerCount)
    {
        Vector3[] positions = new Vector3[playerCount];
        
        // Obtener límites de la cámara
        Camera cam = Camera.main;
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        
        // Aplicar margen desde los bordes
        float xMargin = halfWidth * marginFromEdge;
        float yMargin = halfHeight * marginFromEdge;
        
        // Posiciones disponibles (esquinas y centro de bordes)
        Vector3[] availablePositions = new Vector3[]
        {
            // Esquinas
            new Vector3(-halfWidth + xMargin, halfHeight - yMargin, 0),    // Superior izquierda
            new Vector3(halfWidth - xMargin, halfHeight - yMargin, 0),     // Superior derecha
            new Vector3(-halfWidth + xMargin, -halfHeight + yMargin, 0),   // Inferior izquierda
            new Vector3(halfWidth - xMargin, -halfHeight + yMargin, 0),    // Inferior derecha
            
            // Centro de bordes (para más jugadores)
            new Vector3(0, halfHeight - yMargin, 0),                       // Superior centro
            new Vector3(0, -halfHeight + yMargin, 0),                      // Inferior centro
            new Vector3(-halfWidth + xMargin, 0, 0),                       // Izquierda centro
            new Vector3(halfWidth - xMargin, 0, 0)                         // Derecha centro
        };

        // Seleccionar posiciones según el número de jugadores
        int[] selectionOrder = GetSpawnOrder(playerCount);
        
        for (int i = 0; i < playerCount; i++)
        {
            positions[i] = availablePositions[selectionOrder[i]];
        }
        
        return positions;
    }

    int[] GetSpawnOrder(int playerCount)
    {
        switch (playerCount)
        {
            case 1:
                return new int[] { 0 };
            case 2:
                return new int[] { 0, 3 };
            case 3:
                return new int[] { 0, 1, 2 };
            case 4:
                return new int[] { 0, 1, 2, 3 };
            case 5:
                return new int[] { 0, 1, 2, 3, 4 };
            case 6:
                return new int[] { 0, 1, 2, 3, 4, 5 };
            case 7:
                return new int[] { 0, 1, 2, 3, 4, 5, 6 };
            default:
                return new int[] { 0, 1, 2, 3, 4, 5, 6, 7 };
        }
    }

    public void AssignTankAppearance(GameObject tank, int playerIndex)
    {
        Debug.Log($"=== AssignTankAppearance para {tank.name}, playerIndex: {playerIndex} ===");
        
        SpriteRenderer tankRenderer = tank.GetComponent<SpriteRenderer>();
        Debug.Log($"SpriteRenderer encontrado: {tankRenderer != null}");
        Debug.Log($"Tank sprites length: {tankSprites.Length}");
        
        if (tankRenderer != null && tankSprites.Length > 0)
        {
            Debug.Log("Asignando sprite y escala...");
            tankRenderer.sprite = tankSprites[playerIndex % tankSprites.Length];
            tank.transform.localScale = Vector3.one * desiredTankSize;
            
            Debug.Log($"Sprite asignado: {tankRenderer.sprite.name}");
            Debug.Log($"Escala aplicada: {tank.transform.localScale}");
            
            // *** ACTUALIZAR COLLIDER DESPUÉS DE CAMBIAR ESCALA ***
            UpdateColliderSize(tank);
            
            if (enableShadows)
            {
                Debug.Log("Creando sombra...");
                CreateTankShadow(tank, playerIndex);
            }
            
            Debug.Log("=== AssignTankAppearance COMPLETADO ===");
        }
        else
        {
            Debug.LogError($"FALLO: tankRenderer={tankRenderer}, tankSprites.Length={tankSprites.Length}");
        }
    }

    // *** NUEVA FUNCIÓN PARA ACTUALIZAR COLLIDER ***
    private void UpdateColliderSize(GameObject tank)
    {
        BoxCollider2D boxCollider = tank.GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            SpriteRenderer spriteRenderer = tank.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                // Ajustar el collider al tamaño real del sprite escalado
                Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
                boxCollider.size = spriteSize;
                
                Debug.Log($"Collider actualizado - Tamaño: {boxCollider.size}");
            }
        }
        else
        {
            Debug.LogWarning($"No se encontró BoxCollider2D en {tank.name}");
        }
    }

    private void CreateTankShadow(GameObject tank, int playerIndex)
    {
        GameObject shadowObject = new GameObject("TankShadow");
        shadowObject.transform.SetParent(tank.transform, false);
        shadowObject.transform.localPosition = new Vector3(0, shadowOffset, 0);
        
        SpriteRenderer shadowRenderer = shadowObject.AddComponent<SpriteRenderer>();
        shadowRenderer.sprite = CreateCircleSprite();
        shadowRenderer.sortingOrder = shadowSortingOrder;
        
        Color shadowColor = playerShadowColors[playerIndex % playerShadowColors.Length];
        shadowRenderer.color = shadowColor;
        
        shadowObject.transform.localScale = Vector3.one * shadowRadius;
        
        TankShadowComponent shadowComponent = tank.GetComponent<TankShadowComponent>();
        if (shadowComponent == null)
        {
            shadowComponent = tank.AddComponent<TankShadowComponent>();
        }
        shadowComponent.shadowObject = shadowObject;
        shadowComponent.shadowRenderer = shadowRenderer;
    }

    private Sprite CreateCircleSprite()
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size);
        
        Color[] pixels = new Color[size * size];
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float radius = size * 0.4f;
        
        for (int i = 0; i < pixels.Length; i++)
        {
            int x = i % size;
            int y = i / size;
            float distance = Vector2.Distance(new Vector2(x, y), center);
            
            if (distance <= radius)
            {
                pixels[i] = Color.white;
            }
            else
            {
                pixels[i] = Color.clear;
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}