using UnityEngine;

public class FakeServerLights : MonoBehaviour
{
    public Material fakeLightMaterial;
    public float blinkSpeed = 0.5f;
    public Color[] ledColors = { Color.green, Color.blue, Color.cyan };
    
    void Start()
    {
        // Crea pequeños quad automáticamente
        for (int i = 0; i < 30; i++)
        {
            GameObject led = GameObject.CreatePrimitive(PrimitiveType.Quad);
            led.transform.parent = this.transform;
            led.transform.localPosition = new Vector3(
                Random.Range(-8f, 8f),
                Random.Range(-3f, 3f),
                Random.Range(-2f, 2f)
            );
            led.transform.localScale = Vector3.one * 0.1f;
            
            // Material con emisión
            led.GetComponent<MeshRenderer>().material = fakeLightMaterial;
            led.GetComponent<MeshRenderer>().material.color = ledColors[Random.Range(0, ledColors.Length)];
            
            // Script de parpadeo
            led.AddComponent<BlinkEffect>().speed = blinkSpeed + Random.Range(-0.2f, 0.2f);
        }
    }
}



public class BlinkEffect : MonoBehaviour
{
    public float speed = 0.5f;
    private Material mat;
    private Color originalColor;
    private float minIntensity = 0.2f;
    private float maxIntensity = 1.5f;
    
    void Start()
    {
        mat = GetComponent<MeshRenderer>().material;
        originalColor = mat.GetColor("_EmissionColor");
        
        // Si el material no tiene emisión activada, la activamos
        mat.EnableKeyword("_EMISSION");
    }
    
    void Update()
    {
        // Parpadeo usando seno (más suave y sin errores)
        float blink = (Mathf.Sin(Time.time * speed) + 1f) / 2f;
        float currentIntensity = Mathf.Lerp(minIntensity, maxIntensity, blink);
        
        // Aplicar el color con intensidad variable
        Color finalColor = originalColor * currentIntensity;
        mat.SetColor("_EmissionColor", finalColor);
    }
}