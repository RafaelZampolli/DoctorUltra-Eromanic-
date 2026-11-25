using UnityEngine;

public class MenuScreenShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float shakeIntensity = 0.05f;
    [SerializeField] private float shakeFrequency = 0.8f;
    [SerializeField] private bool usePerlinNoise = true;
    
    [Header("Shake Variations")]
    [SerializeField] private float rotationShakeIntensity = 0.2f;
    [SerializeField] private bool enableRotationShake = true;
    
    [Header("Breathing Effect")]
    [SerializeField] private bool enableBreathing = true;
    [SerializeField] private float breathingSpeed = 0.3f;
    [SerializeField] private float breathingIntensity = 0.02f;
    
    private Vector3 originalPosition;
    private Vector3 originalRotation;
    private float timeOffset;
    private Camera targetCamera;
    
    void Start()
    {
        targetCamera = GetComponent<Camera>();
        if (targetCamera == null)
            targetCamera = Camera.main;
            
        originalPosition = transform.localPosition;
        originalRotation = transform.localEulerAngles;
        
        // Offset aleatório pra cada instância não ficar sincronizada
        timeOffset = Random.Range(0f, 100f);
    }
    
    void Update()
    {
        ApplyScreenShake();
    }
    
    void ApplyScreenShake()
    {
        float currentTime = Time.time + timeOffset;
        Vector3 shakeOffset = Vector3.zero;
        Vector3 rotationOffset = Vector3.zero;
        
        if (usePerlinNoise)
        {
            // Perlin noise é mais orgânico que seno/coseno
            float xNoise = Mathf.PerlinNoise(currentTime * shakeFrequency, 0f) - 0.5f;
            float yNoise = Mathf.PerlinNoise(0f, currentTime * shakeFrequency) - 0.5f;
            float zNoise = Mathf.PerlinNoise(currentTime * shakeFrequency * 0.5f, currentTime * shakeFrequency * 0.3f) - 0.5f;
            
            shakeOffset = new Vector3(xNoise, yNoise, zNoise * 0.3f) * shakeIntensity;
            
            if (enableRotationShake)
            {
                float rotX = (Mathf.PerlinNoise(currentTime * shakeFrequency * 0.7f, 50f) - 0.5f) * rotationShakeIntensity;
                float rotY = (Mathf.PerlinNoise(50f, currentTime * shakeFrequency * 0.7f) - 0.5f) * rotationShakeIntensity;
                float rotZ = (Mathf.PerlinNoise(currentTime * shakeFrequency * 0.4f, 80f) - 0.5f) * rotationShakeIntensity * 0.5f;
                
                rotationOffset = new Vector3(rotX, rotY, rotZ);
            }
        }
        else
        {
            // Versão com seno/coseno mais previsível
            float x = Mathf.Sin(currentTime * shakeFrequency * 2.3f) * shakeIntensity;
            float y = Mathf.Cos(currentTime * shakeFrequency * 1.7f) * shakeIntensity;
            float z = Mathf.Sin(currentTime * shakeFrequency * 0.8f) * shakeIntensity * 0.3f;
            
            shakeOffset = new Vector3(x, y, z);
            
            if (enableRotationShake)
            {
                float rotX = Mathf.Sin(currentTime * shakeFrequency * 1.1f) * rotationShakeIntensity;
                float rotY = Mathf.Cos(currentTime * shakeFrequency * 0.9f) * rotationShakeIntensity;
                
                rotationOffset = new Vector3(rotX, rotY, 0f);
            }
        }
        
        // Breathing effect - adiciona uma respiração mais lenta e sutil
        if (enableBreathing)
        {
            float breathingX = Mathf.Sin(currentTime * breathingSpeed) * breathingIntensity;
            float breathingY = Mathf.Cos(currentTime * breathingSpeed * 0.7f) * breathingIntensity * 0.6f;
            
            shakeOffset += new Vector3(breathingX, breathingY, 0f);
        }
        
        // Aplica as transformações
        transform.localPosition = originalPosition + shakeOffset;
        
        if (enableRotationShake)
        {
            transform.localEulerAngles = originalRotation + rotationOffset;
        }
    }
    
    [Header("Runtime Controls")]
    [SerializeField] private bool debugMode = false;
    
    void OnValidate()
    {
        // Preview no editor
        if (debugMode && Application.isPlaying)
        {
            ApplyScreenShake();
        }
    }
    
    // Métodos públicos pra controlar via código
    public void SetShakeIntensity(float intensity)
    {
        shakeIntensity = intensity;
    }
    
    public void EnableShake(bool enable)
    {
        enabled = enable;
        if (!enable)
        {
            transform.localPosition = originalPosition;
            transform.localEulerAngles = originalRotation;
        }
    }
    
    public void PulseShake(float duration, float multiplier = 2f)
    {
        StartCoroutine(PulseShakeCoroutine(duration, multiplier));
    }
    
    private System.Collections.IEnumerator PulseShakeCoroutine(float duration, float multiplier)
    {
        float originalIntensity = shakeIntensity;
        float originalRotIntensity = rotationShakeIntensity;
        
        shakeIntensity *= multiplier;
        rotationShakeIntensity *= multiplier;
        
        yield return new WaitForSeconds(duration);
        
        // Smooth return
        float elapsed = 0f;
        float returnDuration = 0.3f;
        
        while (elapsed < returnDuration)
        {
            float t = elapsed / returnDuration;
            shakeIntensity = Mathf.Lerp(shakeIntensity, originalIntensity, t);
            rotationShakeIntensity = Mathf.Lerp(rotationShakeIntensity, originalRotIntensity, t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        shakeIntensity = originalIntensity;
        rotationShakeIntensity = originalRotIntensity;
    }
}