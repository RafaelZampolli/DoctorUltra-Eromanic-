using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Camera))]
public class BuckshotSimpleFinal : MonoBehaviour
{
    [Header("🎮 BUCKSHOT EFFECT")]
    [SerializeField] private bool effectEnabled = true;
    
    [Header("Pixelization")]
    [Range(1, 64)] public int pixelSize = 12;
    
    [Header("Color Grading")]
    [Range(2, 32)] public int posterizeSteps = 8;
    [Range(0.5f, 3f)] public float contrast = 1.5f;
    [Range(0f, 1f)] public float saturation = 0.8f;
    
    [Header("Effects")]
    [Range(0f, 1f)] public float vignetteStrength = 0.7f;
    [Range(0f, 0.1f)] public float noiseStrength = 0.02f;
    
    [Header("Debug")]
    public KeyCode toggleKey = KeyCode.Tab;
    public bool showDebugInfo = true;
    
    private Material effectMaterial;
    private Camera cam;
    private RenderTexture pixelatedRT;
    
    // Shader inline simples que FUNCIONA
    private const string SimpleShaderCode = @"
Shader ""Hidden/BuckshotSimpleFinal""
{
    Properties
    {
        _MainTex (""Texture"", 2D) = ""white"" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include ""UnityCG.cginc""
            
            sampler2D _MainTex;
            float _Posterize;
            float _Contrast;
            float _Saturation;
            float _Vignette;
            float _Noise;
            
            fixed4 frag (v2f_img i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Posterização
                col.rgb = floor(col.rgb * _Posterize) / _Posterize;
                
                // Contraste
                col.rgb = saturate((col.rgb - 0.5) * _Contrast + 0.5);
                
                // Saturação
                float gray = dot(col.rgb, float3(0.299, 0.587, 0.114));
                col.rgb = lerp(float3(gray, gray, gray), col.rgb, _Saturation);
                
                // Vignette
                float2 uv = i.uv * (1.0 - i.uv);
                float vig = uv.x * uv.y * 15.0;
                vig = pow(vig, _Vignette);
                col.rgb *= vig;
                
                // Noise
                float noise = frac(sin(dot(i.uv + _Time.y, float2(12.9898, 78.233))) * 43758.5453);
                col.rgb += noise * _Noise;
                
                return col;
            }
            ENDCG
        }
    }
}";

    void Start()
    {
        cam = GetComponent<Camera>();
        SetupMaterial();
        Debug.Log("✅ Buckshot Effect Inicializado!");
        Debug.Log("📌 Aperte TAB para ligar/desligar");
    }
    
    void SetupMaterial()
    {
        // Tenta achar o shader
        Shader shader = Shader.Find("Hidden/BuckshotSimpleFinal");
        
        // Se não achar, usa um fallback GARANTIDO
        if (shader == null)
        {
            shader = Shader.Find("Hidden/Internal-Colored");
            if (shader == null)
                shader = Shader.Find("Sprites/Default");
            if (shader == null)
                shader = Shader.Find("GUI/Text Shader");
                
            Debug.LogWarning($"⚠️ Usando shader fallback: {shader?.name}");
        }
        
        if (shader != null)
        {
            effectMaterial = new Material(shader);
            effectMaterial.hideFlags = HideFlags.DontSave;
        }
        else
        {
            Debug.LogError("❌ Nenhum shader disponível!");
            enabled = false;
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            effectEnabled = !effectEnabled;
            Debug.Log($"🎮 Buckshot: {(effectEnabled ? "ON" : "OFF")}");
        }
        
        // Atalhos para ajustar em runtime
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
                pixelSize = Mathf.Min(pixelSize + 2, 64);
            if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
                pixelSize = Mathf.Max(pixelSize - 2, 1);
        }
    }
    
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!effectEnabled || effectMaterial == null)
        {
            Graphics.Blit(source, destination);
            return;
        }
        
        // Primeiro passa: Pixelização
        int width = source.width / pixelSize;
        int height = source.height / pixelSize;
        
        if (width < 1) width = 1;
        if (height < 1) height = 1;
        
        // Cria RT pixelado
        if (pixelatedRT != null)
            RenderTexture.ReleaseTemporary(pixelatedRT);
            
        pixelatedRT = RenderTexture.GetTemporary(width, height, 0);
        pixelatedRT.filterMode = FilterMode.Point; // IMPORTANTE: Point filtering!
        
        // Renderiza pixelado
        Graphics.Blit(source, pixelatedRT);
        
        // Segunda passa: Efeitos de cor
        effectMaterial.SetFloat("_Posterize", posterizeSteps);
        effectMaterial.SetFloat("_Contrast", contrast);
        effectMaterial.SetFloat("_Saturation", saturation);
        effectMaterial.SetFloat("_Vignette", vignetteStrength);
        effectMaterial.SetFloat("_Noise", noiseStrength);
        
        // Renderiza final
        Graphics.Blit(pixelatedRT, destination, effectMaterial);
        
        // Limpa
        RenderTexture.ReleaseTemporary(pixelatedRT);
    }
    
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 14;
        style.normal.textColor = effectEnabled ? Color.green : Color.red;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.Label($"🎮 BUCKSHOT: {(effectEnabled ? "ON" : "OFF")}", style);
        GUILayout.Label($"Pixel Size: {pixelSize}", style);
        GUILayout.Label($"[TAB] Toggle ON/OFF", style);
        GUILayout.Label($"[Ctrl + / Ctrl -] Adjust Pixels", style);
        GUILayout.EndArea();
    }
    
    void OnDestroy()
    {
        if (effectMaterial != null)
            DestroyImmediate(effectMaterial);
        if (pixelatedRT != null)
            RenderTexture.ReleaseTemporary(pixelatedRT);
    }
}