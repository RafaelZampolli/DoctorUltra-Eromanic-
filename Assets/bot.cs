using UnityEngine;
using UnityEngine.UI;

public class BotaoAlternadorSprites : MonoBehaviour
{
    [Header("Sprites do Botão")]
    [Tooltip("Array com os 3 sprites que vão alternar")]
    public Sprite[] sprites = new Sprite[3];

    [Header("Configurações")]
    [Tooltip("Índice atual do sprite (0, 1 ou 2)")]
    [SerializeField] private int indiceAtual = 0;

    [Header("Componentes")]
    [Tooltip("Image component do botão (auto-preenchido se vazio)")]
    public Image imagemBotao;

    [Header("Game Feel")]
    public bool usarAnimacao = true;
    [Range(0.05f, 0.3f)] public float duracaoAnimacao = 0.1f;
    public AudioClip somClique;

    // Cache dos componentes
    private Button botao;
    private AudioSource audioSource;
    private Vector3 escalaOriginal;

    void Start()
    {
        InicializarComponentes();
        AtualizarSprite();
    }

    void InicializarComponentes()
    {
        // Auto-encontra a Image se não foi configurada
        if (imagemBotao == null)
        {
            imagemBotao = GetComponent<Image>();
            if (imagemBotao != null)
            {
                Debug.Log("Image component encontrado automaticamente!");
            }
        }

        // Configura o botão
        botao = GetComponent<Button>();
        if (botao != null)
        {
            botao.onClick.AddListener(AlternarSprite);
        }

        // AudioSource para som
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && somClique != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Escala original para animação
        escalaOriginal = transform.localScale;

        // Validações
        ValidarConfiguracao();
    }

    void ValidarConfiguracao()
    {
        if (imagemBotao == null)
        {
            Debug.LogWarning($"[{name}] Image component não encontrado!");
        }

        if (sprites.Length != 3)
        {
            Debug.LogWarning($"[{name}] Array de sprites deve ter exatamente 3 elementos!");
        }

        // Verifica se todos os sprites estão configurados
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] == null)
            {
                Debug.LogWarning($"[{name}] Sprite no índice {i} não está configurado!");
            }
        }

        // Garante que o índice está dentro dos limites
        indiceAtual = Mathf.Clamp(indiceAtual, 0, sprites.Length - 1);
    }

    public void AlternarSprite()
    {
        // Avança para o próximo sprite (volta ao 0 depois do 2)
        indiceAtual = (indiceAtual + 1) % sprites.Length;

        // Atualiza a imagem
        AtualizarSprite();

        // Game feel
        TocarSom();
        if (usarAnimacao)
        {
            AnimarClique();
        }

        Debug.Log($"Sprite alternado para índice: {indiceAtual}");
    }

    void AtualizarSprite()
    {
        if (imagemBotao == null || sprites.Length == 0) return;

        // Garante que o índice é válido
        if (indiceAtual >= 0 && indiceAtual < sprites.Length)
        {
            if (sprites[indiceAtual] != null)
            {
                imagemBotao.sprite = sprites[indiceAtual];
            }
            else
            {
                Debug.LogWarning($"Sprite no índice {indiceAtual} é null!");
            }
        }
    }

    void TocarSom()
    {
        if (somClique != null && audioSource != null)
        {
            audioSource.PlayOneShot(somClique, 0.7f);
        }
    }

    void AnimarClique()
    {
        // Para animações anteriores
        StopAllCoroutines();
        StartCoroutine(AnimacaoClique());
    }

    System.Collections.IEnumerator AnimacaoClique()
    {
        // Scale down
        Vector3 escalaReduzida = escalaOriginal * 0.9f;
        float tempo = 0f;
        Vector3 escalaInicial = transform.localScale;

        while (tempo < duracaoAnimacao)
        {
            tempo += Time.deltaTime;
            float t = tempo / duracaoAnimacao;
            transform.localScale = Vector3.Lerp(escalaInicial, escalaReduzida, t);
            yield return null;
        }

        // Scale back up
        tempo = 0f;
        while (tempo < duracaoAnimacao)
        {
            tempo += Time.deltaTime;
            float t = tempo / duracaoAnimacao;
            transform.localScale = Vector3.Lerp(escalaReduzida, escalaOriginal, t);
            yield return null;
        }

        transform.localScale = escalaOriginal;
    }

    // ========== MÉTODOS PÚBLICOS EXTRAS ==========

    /// <summary>
    /// Define o sprite para um índice específico
    /// </summary>
    public void DefinirSprite(int indice)
    {
        if (indice >= 0 && indice < sprites.Length)
        {
            indiceAtual = indice;
            AtualizarSprite();
            Debug.Log($"Sprite definido manualmente para índice: {indiceAtual}");
        }
        else
        {
            Debug.LogWarning($"Índice {indice} fora do range válido (0-{sprites.Length - 1})!");
        }
    }

    /// <summary>
    /// Retorna o índice do sprite atual
    /// </summary>
    public int ObterIndiceAtual()
    {
        return indiceAtual;
    }

    /// <summary>
    /// Retorna o sprite atual
    /// </summary>
    public Sprite ObterSpriteAtual()
    {
        if (indiceAtual >= 0 && indiceAtual < sprites.Length)
        {
            return sprites[indiceAtual];
        }
        return null;
    }

    // ========== DEBUG TOOLS ==========

    [ContextMenu("Alternar Sprite (Debug)")]
    void DebugAlternarSprite()
    {
        AlternarSprite();
    }

    [ContextMenu("Resetar para Sprite 0")]
    void DebugResetarSprite()
    {
        DefinirSprite(0);
    }

    [ContextMenu("Mostrar Estado Atual")]
    void DebugMostrarEstado()
    {
        Debug.Log($"=== ESTADO DO BOTÃO ===");
        Debug.Log($"Índice Atual: {indiceAtual}");
        Debug.Log($"Total de Sprites: {sprites.Length}");
        Debug.Log($"Sprite Atual: {(sprites[indiceAtual] != null ? sprites[indiceAtual].name : "NULL")}");
        Debug.Log($"Image Component: {(imagemBotao != null ? "OK" : "NULL")}");
    }
}