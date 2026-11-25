using UnityEngine;

public class ModeloLoja : MonoBehaviour
{
    [Header("Configurações da Loja")]
    [Tooltip("Se deve criar uma Point Light quando estiver na loja")]
    [SerializeField] private bool criarLuzNaLoja = true;

    [Header("Configurações da Luz")]
    [Tooltip("Intensidade da Point Light (menor = mais suave)")]
    [SerializeField] private float intensidadeLuz = 1.5f;

    [Tooltip("Alcance da Point Light (maior = cobre mais área)")]
    [SerializeField] private float alcanceLuz = 10f;

    [Tooltip("Cor da luz")]
    [SerializeField] private Color corLuz = Color.white;

    [Tooltip("Posição da luz relativa ao modelo (fora do objeto)")]
    [SerializeField] private Vector3 posicaoLuz = new Vector3(2f, 3f, 2f);

    [Header("Rotação na Loja")]
    [Tooltip("Velocidade de rotação no eixo Y")]
    [SerializeField] private float velocidadeRotacao = 30f;

    [Tooltip("Eixo de rotação (Y=vertical, X=horizontal, Z=profundidade)")]
    [SerializeField] private Vector3 eixoRotacao = Vector3.up;

    // Referência da luz criada dinamicamente
    private GameObject objetoLuz;
    private Light componenteLight;
    private bool estaNaLoja = false;

    void Start()
    {
        // Por padrão, assume que está na loja quando o script inicia
        EntrarNaLoja();
    }

    void Update()
    {
        // Rotação suave infinita apenas quando está na loja
        if (estaNaLoja)
        {
            transform.Rotate(eixoRotacao * velocidadeRotacao * Time.deltaTime);
        }
    }

    /// <summary>
    /// Chama quando o modelo entra na loja - cria a luz
    /// </summary>
    public void EntrarNaLoja()
    {
        if (estaNaLoja) return; // Já está na loja

        estaNaLoja = true;

        if (criarLuzNaLoja)
        {
            CriarLuzExterna();
        }

        Debug.Log($"Modelo {gameObject.name} entrou na loja - luz criada");
    }

    /// <summary>
    /// Chama quando o modelo sai da loja (volta pro jogo) - remove a luz
    /// </summary>
    public void SairDaLoja()
    {
        if (!estaNaLoja) return; // Já saiu da loja

        estaNaLoja = false;

        RemoverLuz();

        Debug.Log($"Modelo {gameObject.name} saiu da loja - luz removida");
    }

    /// <summary>
    /// Cria uma Point Light externa posicionada pra iluminar o modelo sem sombras
    /// </summary>
    private void CriarLuzExterna()
    {
        if (objetoLuz != null) return; // Já tem luz

        // Cria um GameObject filho pra luz
        objetoLuz = new GameObject("LuzLoja");
        objetoLuz.transform.SetParent(transform);

        // Posiciona a luz fora do modelo
        objetoLuz.transform.localPosition = posicaoLuz;

        // Adiciona e configura o componente Light
        componenteLight = objetoLuz.AddComponent<Light>();
        componenteLight.type = LightType.Point;
        componenteLight.intensity = intensidadeLuz;
        componenteLight.range = alcanceLuz;
        componenteLight.color = corLuz;

        // Remove sombras pra evitar artifacts
        componenteLight.shadows = LightShadows.None;

        // Configura pra só afetar a layer desse objeto
        componenteLight.cullingMask = 1 << gameObject.layer;

        Debug.Log($"Point Light externa criada para {gameObject.name}");
    }

    /// <summary>
    /// Remove a Point Light
    /// </summary>
    private void RemoverLuz()
    {
        if (objetoLuz != null)
        {
            DestroyImmediate(objetoLuz);
            objetoLuz = null;
            componenteLight = null;
            Debug.Log($"Point Light removida de {gameObject.name}");
        }
    }

    /// <summary>
    /// Para a rotação
    /// </summary>
    public void PararRotacao()
    {
        estaNaLoja = false;
    }

    /// <summary>
    /// Retoma a rotação
    /// </summary>
    public void RetomarRotacao()
    {
        estaNaLoja = true;
    }

    void OnDestroy()
    {
        // Limpa a luz se o objeto for destruído
        RemoverLuz();
    }
}