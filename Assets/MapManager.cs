using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("Visual Scale")]
    public float gameScale = 8f;
    
    [Header("Screen Adaptation")]
    public bool adaptToScreenSize = true;
    public float screenMargin = 0f; // Cambiar a 0 para que llegue a los bordes
    
    [Header("Manual Map Settings (si no se adapta a pantalla)")]
    public Vector2 manualMapSize = new Vector2(15, 10);
    public Transform cameraTransform;
    
    [Header("Background")]
    public Sprite[] backgroundSprites;
    public Color backgroundTint = Color.white;
    public int backgroundSortingOrder = -10;
    
    [Header("Map Boundaries")]
    public float wallThickness = 0.5f;
    public bool showBoundaryGizmos = true;
    
    private Vector2 actualMapSize;
    private GameObject backgroundParent;
    private GameObject boundariesParent;
    private Sprite selectedBackgroundSprite;
    
    void Start()
    {
        // Crear el tag si no existe
        CreateWallTagIfNeeded();
        SetupMap();
    }
    
    void CreateWallTagIfNeeded()
    {
        // Este método asegura que tenemos el tag, pero Unity maneja los tags en tiempo de compilación
        // Por eso es mejor crearlos manualmente en el editor
    }
    
    void SetupMap()
    {
        SetupCamera();
        CalculateMapSize();
        SelectRandomBackgroundSprite();
        CreateSingleBackgroundImage();
        CreateMapBoundaries();
    }
    
    void SetupCamera()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        
        if (cameraTransform != null)
        {
            cameraTransform.position = new Vector3(0, 0, -10);
            
            Camera cam = cameraTransform.GetComponent<Camera>();
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = gameScale;
                
                Debug.Log($"Cámara configurada con escala: {gameScale}");
            }
        }
    }
    
    void CalculateMapSize()
    {
        if (adaptToScreenSize)
        {
            Camera cam = Camera.main;
            if (cameraTransform != null)
                cam = cameraTransform.GetComponent<Camera>();
            
            if (cam != null)
            {
                // Calcular el área EXACTA visible por la cámara
                float cameraHeight = cam.orthographicSize * 2f;
                float cameraWidth = cameraHeight * cam.aspect;
                
                // USAR TODA LA PANTALLA - sin margen interno
                actualMapSize = new Vector2(cameraWidth, cameraHeight);
                
                Debug.Log($"Mapa usando TODA la pantalla: {actualMapSize}");
                Debug.Log($"Área visible de cámara: {cameraWidth}x{cameraHeight}");
            }
            else
            {
                actualMapSize = manualMapSize;
            }
        }
        else
        {
            actualMapSize = manualMapSize;
        }
    }
    
    void SelectRandomBackgroundSprite()
    {
        if (backgroundSprites == null || backgroundSprites.Length == 0)
        {
            Debug.LogWarning("No hay sprites de fondo asignados!");
            return;
        }
        
        int randomIndex = Random.Range(0, backgroundSprites.Length);
        selectedBackgroundSprite = backgroundSprites[randomIndex];
        
        Debug.Log($"Sprite de fondo seleccionado: {selectedBackgroundSprite.name}");
    }
    
    void CreateSingleBackgroundImage()
    {
        backgroundParent = new GameObject("Background");
        backgroundParent.transform.parent = transform;
        
        if (selectedBackgroundSprite == null)
        {
            Debug.LogWarning("No hay sprite de fondo seleccionado!");
            return;
        }
        
        GameObject backgroundObject = new GameObject("BackgroundImage");
        backgroundObject.transform.parent = backgroundParent.transform;
        backgroundObject.transform.position = Vector3.zero;
        
        SpriteRenderer sr = backgroundObject.AddComponent<SpriteRenderer>();
        sr.sprite = selectedBackgroundSprite;
        sr.color = backgroundTint;
        sr.sortingOrder = backgroundSortingOrder;
        
        if (sr.sprite != null)
        {
            Vector2 spriteSize = sr.sprite.bounds.size;
            
            // Escalar para cubrir EXACTAMENTE el área visible + un poco extra para evitar bordes
            float scaleX = (actualMapSize.x + 1f) / spriteSize.x;
            float scaleY = (actualMapSize.y + 1f) / spriteSize.y;
            
            float scale = Mathf.Max(scaleX, scaleY);
            backgroundObject.transform.localScale = new Vector3(scale, scale, 1f);
            
            Debug.Log($"Fondo escalado {scale:F2}x para cubrir {actualMapSize}");
        }
    }
    
    void CreateMapBoundaries()
    {
        boundariesParent = new GameObject("Map Boundaries");
        boundariesParent.transform.parent = transform;
        
        float halfWidth = actualMapSize.x / 2f;
        float halfHeight = actualMapSize.y / 2f;
        
        // Crear paredes EXACTAMENTE en los bordes de la pantalla
        // Pared superior - justo en el borde superior
        CreateBoundaryWall(
            new Vector3(0, halfHeight + wallThickness/2, 0),
            new Vector2(actualMapSize.x + wallThickness*2, wallThickness),
            "TopWall"
        );
        
        // Pared inferior - justo en el borde inferior
        CreateBoundaryWall(
            new Vector3(0, -halfHeight - wallThickness/2, 0),
            new Vector2(actualMapSize.x + wallThickness*2, wallThickness),
            "BottomWall"
        );
        
        // Pared izquierda - justo en el borde izquierdo
        CreateBoundaryWall(
            new Vector3(-halfWidth - wallThickness/2, 0, 0),
            new Vector2(wallThickness, actualMapSize.y + wallThickness*2),
            "LeftWall"
        );
        
        // Pared derecha - justo en el borde derecho
        CreateBoundaryWall(
            new Vector3(halfWidth + wallThickness/2, 0, 0),
            new Vector2(wallThickness, actualMapSize.y + wallThickness*2),
            "RightWall"
        );
        
        Debug.Log($"Límites creados en los bordes exactos de la pantalla: {actualMapSize}");
    }
    
    void CreateBoundaryWall(Vector3 position, Vector2 size, string wallName)
    {
        GameObject wall = new GameObject(wallName);
        wall.transform.parent = boundariesParent.transform;
        wall.transform.position = position;
        
        // Usar el tag si existe, si no, solo usar el nombre
        try
        {
            wall.tag = "Wall";
        }
        catch
        {
            Debug.LogWarning($"Tag 'Wall' no existe. Crea el tag 'Wall' en Project Settings → Tags and Layers");
        }
        
        BoxCollider2D collider = wall.AddComponent<BoxCollider2D>();
        collider.size = size;
        collider.isTrigger = false;
    }
    
    public void RecalculateForScreenSize()
    {
        if (backgroundParent != null) DestroyImmediate(backgroundParent);
        if (boundariesParent != null) DestroyImmediate(boundariesParent);
        
        SetupMap();
    }
    
    public bool IsPositionInBounds(Vector2 position)
    {
        return Mathf.Abs(position.x) <= actualMapSize.x/2 && 
               Mathf.Abs(position.y) <= actualMapSize.y/2;
    }
    
    public Vector2 GetMapSize()
    {
        return actualMapSize;
    }
    
    void OnDrawGizmos()
    {
        if (showBoundaryGizmos)
        {
            Vector2 mapSize = Application.isPlaying ? actualMapSize : 
                             (adaptToScreenSize ? new Vector2(20, 15) : manualMapSize);
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(mapSize.x, mapSize.y, 0));
            
            Gizmos.color = Color.red;
            float halfWidth = mapSize.x / 2f;
            float halfHeight = mapSize.y / 2f;
            
            // Mostrar exactamente donde están las paredes
            Gizmos.DrawWireCube(new Vector3(0, halfHeight + wallThickness/2, 0), 
                               new Vector3(mapSize.x + wallThickness*2, wallThickness, 0));
            Gizmos.DrawWireCube(new Vector3(0, -halfHeight - wallThickness/2, 0), 
                               new Vector3(mapSize.x + wallThickness*2, wallThickness, 0));
            Gizmos.DrawWireCube(new Vector3(-halfWidth - wallThickness/2, 0, 0), 
                               new Vector3(wallThickness, mapSize.y + wallThickness*2, 0));
            Gizmos.DrawWireCube(new Vector3(halfWidth + wallThickness/2, 0, 0), 
                               new Vector3(wallThickness, mapSize.y + wallThickness*2, 0));
        }
    }
}