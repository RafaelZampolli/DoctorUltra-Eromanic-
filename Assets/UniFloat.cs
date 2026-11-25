using UnityEngine;

/// <summary>
/// Adiciona um efeito de flutuação avançado e suave a qualquer GameObject,
/// com controle sobre movimento vertical, horizontal, rotação contínua e rotação de balanço.
/// </summary>
public class AdvancedFloating : MonoBehaviour
{
    [Header("Eixo de Movimento Vertical (Y)")]
    [Tooltip("Marque para ativar a flutuação para cima e para baixo.")]
    public bool habilitarMovimentoY = true;
    [Range(0f, 2f)] public float amplitudeY = 0.25f;
    [Range(0f, 5f)] public float velocidadeY = 1f;

    [Header("Eixo de Movimento Horizontal (X)")]
    [Tooltip("Marque para ativar a flutuação para esquerda e direita.")]
    public bool habilitarMovimentoX = false;
    [Range(0f, 2f)] public float amplitudeX = 0.2f;
    [Range(0f, 5f)] public float velocidadeX = 0.8f;

    [Header("Rotação de Balanço (Vai e Vem)")]
    [Tooltip("Marque para ativar a rotação de balanço.")]
    public bool habilitarRotacaoDeBalanco = false;
    [Tooltip("O ângulo máximo, em graus, que o objeto irá balançar para cada lado.")]
    [Range(0f, 90f)] public float anguloMaximoDeBalanco = 15f;
    [Tooltip("A velocidade do balanço.")]
    [Range(0f, 5f)] public float velocidadeDeBalanco = 1.2f;

    [Header("Rotação Contínua (Giro)")]
    [Tooltip("Marque para ativar a rotação contínua. Desativa o balanço se marcado.")]
    public bool habilitarRotacaoContinua = false;
    [Tooltip("A velocidade com que o objeto irá girar.")]
    [Range(0f, 360f)] public float velocidadeRotacaoContinua = 50f;

    [Header("Configuração Global da Rotação")]
    [Tooltip("O eixo em torno do qual o objeto irá girar ou balançar (ex: (0, 1, 0) para girar em torno de Y).")]
    public Vector3 eixoDeRotacao = new Vector3(0, 1, 0);

    // Variáveis privadas
    private Vector3 posicaoInicial;
    private Quaternion rotacaoInicial;
    private float deslocamentoAleatorio;

    void Start()
    {
        // Guarda a posição e rotação iniciais como referência
        posicaoInicial = transform.position;
        rotacaoInicial = transform.rotation;

        // Gera um valor aleatório para desincronizar a animação de outros objetos
        deslocamentoAleatorio = Random.Range(0f, 2f * Mathf.PI);
    }

    void Update()
    {
        // --- CÁLCULO DA POSIÇÃO DE FLUTUAÇÃO ---
        CalcularPosicao();

        // --- CÁLCULO DA ROTAÇÃO ---
        // A rotação contínua tem prioridade sobre a de balanço
        if (habilitarRotacaoContinua)
        {
            CalcularRotacaoContinua();
        }
        else if (habilitarRotacaoDeBalanco)
        {
            CalcularRotacaoDeBalanco();
        }
    }

    void CalcularPosicao()
    {
        float deslocamentoX = 0f;
        float deslocamentoY = 0f;

        if (habilitarMovimentoX)
        {
            deslocamentoX = Mathf.Cos((Time.time * velocidadeX) + deslocamentoAleatorio) * amplitudeX;
        }

        if (habilitarMovimentoY)
        {
            deslocamentoY = Mathf.Sin((Time.time * velocidadeY) + deslocamentoAleatorio) * amplitudeY;
        }

        transform.position = new Vector3(
            posicaoInicial.x + deslocamentoX,
            posicaoInicial.y + deslocamentoY,
            posicaoInicial.z
        );
    }

    void CalcularRotacaoDeBalanco()
    {
        // Calcula o ângulo atual usando uma onda senoidal
        float anguloDeBalanço = Mathf.Sin((Time.time * velocidadeDeBalanco) + deslocamentoAleatorio) * anguloMaximoDeBalanco;

        // Cria uma rotação baseada no ângulo calculado em torno do eixo definido
        Quaternion rotacaoOffset = Quaternion.AngleAxis(anguloDeBalanço, eixoDeRotacao);

        // Aplica a rotação de balanço à rotação inicial do objeto
        transform.rotation = rotacaoInicial * rotacaoOffset;
    }

    void CalcularRotacaoContinua()
    {
        // Gira o objeto continuamente
        transform.Rotate(eixoDeRotacao, velocidadeRotacaoContinua * Time.deltaTime, Space.World);
    }
}