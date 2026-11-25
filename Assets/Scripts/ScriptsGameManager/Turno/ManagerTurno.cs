using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerTurno : MonoBehaviour
{
    [Header("Referências")]
    public Animator animatorPaciente;
    public GameObject interludio;
    public InterludioManager Isnterludio;
    public ManagerPontos cronometro;
    public ManagerPacientes managerPacientes;
    public ManagerPrancheta managerPrancheta;
    public SistemaDeducaoPrancheta sistemaDeducao;

    [Header("Controle de Turno")]
    public int MinimoPacientes = 4;
    public int MaximoPacientes = 8;
    public int PacientesAtual;
    public int PacientesTotal;
    public bool CuidarPacientes;
    public bool comecoDoTurno;

    private bool malariaCurada = false;
    private bool sarampoCurado = false;
    private bool pneumoniaCurada = false;
    private bool coleraCurada = false;

    public List<GameObject> listaDeCartas;

    void Start()
    {
        ComecoDoTurno();
        listaDeCartas = new List<GameObject>();
    }

    public void ReduzirTotalDePacientes(int quantidade)
    {
        if (PacientesTotal > PacientesAtual) // Evita que o total fique menor que o já atendido
        {
            PacientesTotal -= quantidade;
            Debug.Log($"Número total de pacientes reduzido para {PacientesTotal}.");
        }
    }

    void Update()
    {
        // Se não for para cuidar, não houver doença, OU SE FOR UMA AÇÃO, não faz nada aqui.
        if (!CuidarPacientes || managerPacientes?.doencaAtual == null || managerPacientes.doencaAtual.eAcao)
        {
            return;
        }

        Doenca doencaAtual = managerPacientes.doencaAtual;

        if (malariaCurada && doencaAtual.tipoCura == TipoDeCura.Malaria) CurarPaciente();
        else if (sarampoCurado && doencaAtual.tipoCura == TipoDeCura.Sarampo) CurarPaciente();
        else if (pneumoniaCurada && doencaAtual.tipoCura == TipoDeCura.Pneumonia) CurarPaciente();
        else if (coleraCurada && doencaAtual.tipoCura == TipoDeCura.Colera) CurarPaciente();
    }

    public void CuraMalarias()
    {
        Debug.Log("Sinal de cura para Malária recebido da prancheta.");
        malariaCurada = true;
    }

    public void CuraPneumonias()
    {
        Debug.Log("Sinal de cura para Pneumonia recebido da prancheta.");
        pneumoniaCurada = true;
    }

    public void CuraSarampos()
    {
        Debug.Log("Sinal de cura para Sarampo recebido da prancheta.");
        sarampoCurado = true;
    }

    public void CuraColeras()
    {
        Debug.Log("Sinal de cura para Cólera recebido da prancheta.");
        coleraCurada = true;
    }

    public void FimDoPaciente()
    {
        CuidarPacientes = false;
        malariaCurada = false;
        sarampoCurado = false;
        pneumoniaCurada = false;
        coleraCurada = false;

        AtualizarAnimatorPaciente();
        if (animatorPaciente != null) animatorPaciente.SetTrigger("saindo");

        float duracaoAnimacaoSaida = 2.0f;
        managerPacientes.RemoverPacienteAtual(duracaoAnimacaoSaida);

        if (cronometro.Contando) cronometro.PararContador();

        Invoke(nameof(ChamarPaciente), duracaoAnimacaoSaida);
        Debug.Log($"Paciente saindo, próximo em {duracaoAnimacaoSaida} segundos");
    }

    public void CurarPaciente()
    {
        if (!CuidarPacientes) return;

        Debug.Log($"✅ Paciente curado de {managerPacientes.doencaAtual.nome}!");

        if (sistemaDeducao != null) sistemaDeducao.PacienteCurado();
        else ResetarPrancheta("Paciente curado (fallback)");

        if (cronometro != null) cronometro.AdicionarPontos(100);

        AplicarEfeitoCura();
        FimDoPaciente();
    }

    public void ComecoDoTurno()
    {
        comecoDoTurno = true;

        if (cronometro.Rodada == 0 || cronometro.Rodada == 1)
        {
            PacientesTotal = 3;
            cronometro.doacoes = 100; // Começa com 100 de dinheiro
            cronometro.pontos = 0;    // Garante que os pontos comecem zerados
        }
        else
        {
            PacientesTotal = Random.Range(MinimoPacientes, MaximoPacientes + 1);
        }

        PacientesAtual = 0;
        Debug.Log($"NOVO TURNO - Total de pacientes: {PacientesTotal}");
        ResetarPrancheta("Início do turno");
        ChamarPaciente();
        foreach (var carta in listaDeCartas)
        {
            // Verificação de segurança, caso uma carta tenha sido destruída enquanto a prancheta estava aberta.

                carta.SetActive(true);

        }
        interludio.SetActive(false);
        cronometro.Rodada++;
        LimparInterludio();
    }

    public void ChamarPaciente()
    {
        // --- ADIÇÃO PARA O AMULETO ---
        // Se a Bola de Cristal estiver ativa na cena, chama seu efeito.
        BolaDeCristalManager.Instance?.PreverEEliminarDoenca();
        // -----------------------------

        if (PacientesAtual < PacientesTotal)
        {
            PacientesAtual++;
            Debug.Log($"Chamando paciente {PacientesAtual} de {PacientesTotal}");
            managerPacientes.DecidirDoenca();
            AtualizarAnimatorPaciente();
            if (animatorPaciente != null) animatorPaciente.SetTrigger("chegando");
            ResetarPrancheta($"Novo paciente #{PacientesAtual}");
            CuidarPaciente();
            cronometro.IniciarContador();
        }
        else
        {
            FimDoTurno();
        }
    }

    private void AtualizarAnimatorPaciente()
    {
        GameObject pacienteAtual = GameObject.FindWithTag("Paciente");
        if (pacienteAtual == null) pacienteAtual = managerPacientes.pacienteAtualObject;

        if (pacienteAtual != null)
        {
            animatorPaciente = pacienteAtual.GetComponent<Animator>();
            if (animatorPaciente == null) Debug.LogWarning("Paciente não tem Animator component!");
        }
        else
        {
            animatorPaciente = null;
        }
    }

    // ALTERADO: Fecha a prancheta com delay de 1 segundo ao cuidar do paciente
    public void CuidarPaciente()
    {
        CuidarPacientes = true;
        Debug.Log("Iniciando cuidado do paciente");
        if (managerPrancheta != null)
            StartCoroutine(FecharPranchetaComDelay());
    }

    // Nova corrotina para fechar com delay
    private IEnumerator FecharPranchetaComDelay()
    {
        yield return new WaitForSeconds(1f);
        managerPrancheta.FecharPrancheta();
    }

    void FimDoTurno()
    {
        interludio.SetActive(true);
        Debug.Log("FIM DO TURNO - Iniciando interlúdio");
        Isnterludio.Interludio = true;
        listaDeCartas.Clear();

        // 2. Encontra todas as cartas ATIVAS na cena e as adiciona à nossa lista.
        foreach (var carta in GameObject.FindGameObjectsWithTag("carta"))
        {
            listaDeCartas.Add(carta);
        }

        Debug.Log($"[DEBUG] Abrindo prancheta. Encontradas e armazenadas {listaDeCartas.Count} cartas.");

        // 3. Desativa cada carta da nossa lista recém-criada.
        foreach (var carta in listaDeCartas)
        {

                carta.SetActive(false);

        }
    }

    private void AplicarEfeitoCura()
    {
        AtualizarAnimatorPaciente();
        if (animatorPaciente != null) animatorPaciente.SetTrigger("curado");
    }

    void ResetarPrancheta(string motivo)
    {
        Debug.Log($"RESETANDO PRANCHETA: {motivo}");
        if (sistemaDeducao != null)
        {
            sistemaDeducao.NovoPaciente();
            return;
        }
        SistemaDeducaoPrancheta sistema = FindObjectOfType<SistemaDeducaoPrancheta>();
        if (sistema != null)
        {
            sistema.NovoPaciente();
            return;
        }
        if (managerPrancheta != null) Debug.Log("Fallback: resetando ManagerPrancheta tradicional");
    }

    void LimparInterludio()
    {
        Isnterludio.objetosJaEscolhidos = false;
        if (Isnterludio.escolhidoTotem1 != null) Isnterludio.escolhidoTotem1.SetActive(false);
        if (Isnterludio.escolhidoTotem2 != null) Isnterludio.escolhidoTotem2.SetActive(false);
        GameObject[] cartasInterludio = new GameObject[] { Isnterludio.cartaAtual1, Isnterludio.cartaAtual2, Isnterludio.cartaAtual3, Isnterludio.cartaAtual4 };
        foreach (var carta in cartasInterludio)
        {
            if (carta != null) Destroy(carta);
        }
        Isnterludio.cartaAtual1 = null;
        Isnterludio.cartaAtual2 = null;
        Isnterludio.cartaAtual3 = null;
        Isnterludio.cartaAtual4 = null;
    }

    [ContextMenu("Debug - Forçar Reset Prancheta")] void DebugForcarReset() => ResetarPrancheta("Debug manual");
    [ContextMenu("Debug - Spawnar Novo Paciente")] void DebugSpawnarPaciente() => managerPacientes?.DecidirDoenca();
    [ContextMenu("Debug - Curar Paciente Atual")] void DebugCurarPaciente() => CurarPaciente();
}