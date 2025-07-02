using UnityEngine;

public class TankShadow : MonoBehaviour
{
    [Header("Shadow Settings")]
    public float shadowRadius = 1.2f;
    public float shadowOffset = -0.1f; // Qué tan abajo está la sombra
    public int shadowSortingOrder = -1; // Debajo del tanque
    
    [Header("Colors per Player")]
    public Color[] playerColors = new Color[]
    {
        new Color(1f, 0.3f, 0.3f, 0.6f), // Rojo semi-transparente
        new Color(0.3f, 0.3f, 1f, 0.6f), // Azul semi-transparente
        new Color(0.3f, 1f, 0.3f, 0.6f), // Verde semi-transparente
        new Color(1f, 1f, 0.3f, 0.6f), // Amarillo semi-transparente
        new Color(1f, 0.3f, 1f, 0.6f), // Magenta semi-transparente
        new Color(0.3f, 1f, 1f, 0.6f)  // Cian semi-transparente
    };
    
    private GameObject shadowObject;
    private SpriteRenderer shadowRenderer;
    
    public void CreateShadow(int playerIndex)
    {
        // Crear el objeto de sombra
        shadowObject = new GameObject("TankShadow");
        shadowObject.transform.parent = transform;
        shadowObject.transform.localPosition = new Vector3(0, shadowOffset, 0);
        
        // Añadir SpriteRenderer para la sombra
        shadowRenderer = shadowObject.AddComponent<SpriteRenderer>();
        shadowRenderer.sprite = CreateCircleSprite();
        shadowRenderer.sortingOrder = shadowSortingOrder;
        
        // Asignar color según el jugador
        Color shadowColor = playerColors[playerIndex % playerColors.Length];
        shadowRenderer.color = shadowColor;
        
        // Escalar la sombra
        float tankScale = Mathf.Max(transform.localScale.x, transform.localScale.y);
        shadowObject.transform.localScale = Vector3.one * shadowRadius * tankScale;
        
        Debug.Log($"Sombra creada para jugador {playerIndex} con color {shadowColor}");
    }
    
    private Sprite CreateCircleSprite()
    {
        // Crear una textura circular programáticamente
        int textureSize = 128;
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        
        Vector2 center = new Vector2(textureSize * 0.5f, textureSize * 0.5f);
        float radius = textureSize * 0.4f;
        
        for (int x = 0; x < textureSize; x++)
        {
            for (int y = 0; y < textureSize; y++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);
                
                if (distance <= radius)
                {
                    // Crear un gradiente suave en los bordes
                    float alpha = 1f - (distance / radius);
                    alpha = Mathf.SmoothStep(0f, 1f, alpha);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        
        // Crear sprite desde la textura
        return Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), 
                           new Vector2(0.5f, 0.5f), textureSize);
    }
    
    public void UpdateShadowScale()
    {
        if (shadowObject != null)
        {
            float tankScale = Mathf.Max(transform.localScale.x, transform.localScale.y);
            shadowObject.transform.localScale = Vector3.one * shadowRadius * tankScale;
        }
    }
    
    public void SetShadowColor(Color newColor)
    {
        if (shadowRenderer != null)
        {
            shadowRenderer.color = newColor;
        }
    }
    
    void OnDestroy()
    {
        if (shadowObject != null)
        {
            DestroyImmediate(shadowObject);
        }
    }
}