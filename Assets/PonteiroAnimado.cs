using UnityEngine;

public class RelogioControlador : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Arraste aqui o componente Animator do seu relógio.")]
    public Animator animator;

    [Tooltip("Referência ao script que gerencia o tempo do paciente.")]
    public ManagerPontos managerPontos;

    [Header("Configuração da Animação")]
    [Tooltip("Nome EXATO do parâmetro de velocidade no Animator Controller.")]
    public string nomeParametroVelocidade = "Speed";

    // --- NOVA LÓGICA DE ESTADOS ---
    private enum EstadoRelogio { Parado, Avancando, Rebobinando }
    private EstadoRelogio estadoAtual = EstadoRelogio.Parado;
    // --------------------------------

    private const float DURACAO_ORIGINAL_ANIMACAO = 10.1f;
    private int hashVelocidadeAnim;

    void Start()
    {
        if (animator == null || managerPontos == null)
        {
            Debug.LogError("[RelogioControlador] ERRO CRÍTICO: 'Animator' e/ou 'ManagerPontos' não foram arrastados no Inspector!", this);
            this.enabled = false; // Desativa o script para evitar erros
            return;
        }

        hashVelocidadeAnim = Animator.StringToHash(nomeParametroVelocidade);
        PararRelogio(); // Garante que comece parado
    }

    void Update()
    {
        // Lógica principal baseada em estados
        switch (estadoAtual)
        {
            case EstadoRelogio.Parado:
                // Se o relógio está parado e o contador começar, inicie o avanço.
                if (managerPontos.Contando)
                {
                    IniciarRelogio();
                }
                break;

            case EstadoRelogio.Avancando:
                // Se o relógio está avançando e o contador parar, inicie o rebobinamento.
                if (!managerPontos.Contando)
                {
                    RebobinarRelogio();
                }
                break;

            case EstadoRelogio.Rebobinando:
                // Se o contador começar de novo enquanto rebobina, avance imediatamente.
                if (managerPontos.Contando)
                {
                    IniciarRelogio();
                    return; // Sai do Update para não fazer a verificação abaixo
                }

                // Pega o estado atual da animação
                var animStateInfo = animator.GetCurrentAnimatorStateInfo(0);

                // Se a animação chegou ao início (normalizedTime <= 0), pare o relógio.
                // Usamos 0.01f como margem de segurança.
                if (animStateInfo.normalizedTime <= 0.01f)
                {
                    PararRelogio();
                }
                break;
        }
    }

    private void IniciarRelogio()
    {
        Debug.Log("[RelogioControlador] MUDANDO ESTADO PARA: Avancando");
        estadoAtual = EstadoRelogio.Avancando;

        float tempoMaximoPaciente = managerPontos.GetTempoMaximoDoTurno();
        if (tempoMaximoPaciente <= 0)
        {
            PararRelogio();
            return;
        }

        float novaVelocidade = DURACAO_ORIGINAL_ANIMACAO / tempoMaximoPaciente;
        animator.SetFloat(hashVelocidadeAnim, novaVelocidade);
        Debug.Log($"[RelogioControlador] VELOCIDADE DEFINIDA PARA: {novaVelocidade}");
    }

    private void RebobinarRelogio()
    {
        Debug.Log("[RelogioControlador] MUDANDO ESTADO PARA: Rebobinando");
        estadoAtual = EstadoRelogio.Rebobinando;

        float velocidadeRebobinar = -8f;
        animator.SetFloat(hashVelocidadeAnim, velocidadeRebobinar);
        Debug.Log($"[RelogioControlador] VELOCIDADE DEFINIDA PARA: {velocidadeRebobinar}");
    }

    private void PararRelogio()
    {
        // Apenas muda o estado se já não estiver parado.
        if (estadoAtual != EstadoRelogio.Parado)
        {
            Debug.Log("[RelogioControlador] MUDANDO ESTADO PARA: Parado");
            estadoAtual = EstadoRelogio.Parado;
        }

        animator.SetFloat(hashVelocidadeAnim, 0f);
        Debug.Log($"[RelogioControlador] VELOCIDADE DEFINIDA PARA: 0");
    }
}