using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

public class InterludioUIManager : MonoBehaviour
{
    [Header("Referências dos Managers")]
    public InterludioManager interludioManager; // O seu InterludioManager atual
    public ManagerPontos managerPontos;
    public ManagerTurno managerTurno;

    [Header("Referências da UI")]
    public GameObject painelGeralUI; // O "PainelLoja_UI" que você criou
    public TextMeshProUGUI doacoesText;
    public Image setaFeedbackImage;
    public Button botaoAtualizarCartas;
    public Button botaoAtualizarAmuletos;
    public Button botaoSairDoInterludio;

    [Header("Sprites da Seta")]
    public Sprite setaCimaSprite;
    public Sprite setaBaixoSprite;

    [Header("Configurações da Loja")]
    [Tooltip("Custo em doações para atualizar as cartas na loja.")]
    public int custoAtualizarCartas = 50;
    [Tooltip("Custo em doações para atualizar os amuletos/totens na loja.")]
    public int custoAtualizarAmuletos = 50;

    private int doacoesExibidas;
    private Coroutine animacaoSetaCoroutine;

    void Start()
    {
        // Garante que a UI comece desligada e os botões configurados
        if (painelGeralUI != null) painelGeralUI.SetActive(false);
        setaFeedbackImage.gameObject.SetActive(false);

        // Atribui as funções para cada botão
        botaoAtualizarCartas.onClick.AddListener(OnAtualizarCartasClick);
        botaoAtualizarAmuletos.onClick.AddListener(OnAtualizarAmuletosClick);
        botaoSairDoInterludio.onClick.AddListener(OnSairClick);
    }

    void Update()
    {
        // Se a UI não estiver ativa, não há o que fazer aqui.
        if (!painelGeralUI.activeSelf) return;

        // Verifica a cada frame se o valor das doações mudou para iniciar a animação.
        if (managerPontos.Doacoes != doacoesExibidas)
        {
            AnimarMudancaDoacoes(managerPontos.Doacoes);
        }
    }

    // --- MÉTODOS DE CONTROLE (para serem chamados pelo ManagerTurno) ---

    public void AbrirLoja()
    {
        painelGeralUI.SetActive(true);
        AtualizarDisplayDoacoes(true); // Atualiza o valor de doações imediatamente, sem animação.
    }

    public void FecharLoja()
    {
        painelGeralUI.SetActive(false);
    }

    // --- LÓGICA DOS BOTÕES ---

    private void OnAtualizarCartasClick()
    {
        if (managerPontos.Doacoes >= custoAtualizarCartas)
        {
            managerPontos.Doacoes -= custoAtualizarCartas;
            Debug.Log($"Pagou {custoAtualizarCartas} para atualizar as cartas.");

            // A animação vai rolar automática no Update() quando detectar a mudança
            // Não precisa chamar nada extra aqui

            // Usa o método já existente no seu InterludioManager para limpar e respawnar
            interludioManager.ResetarInterludio();
            interludioManager.objetosJaEscolhidos = false; // Permite que o Update do InterludioManager rode de novo
            interludioManager.Interludio = true;         // Ativa a flag para o spawn acontecer
        }
        else
        {
            Debug.Log("Dinheiro insuficiente para atualizar as cartas.");
            AvisarSemDinheiro();
        }
    }

    private void OnAtualizarAmuletosClick()
    {
        // Esta lógica assume que os "Amuletos" são os "Totens"
        if (managerPontos.Doacoes >= custoAtualizarAmuletos)
        {
            managerPontos.Doacoes -= custoAtualizarAmuletos;
            Debug.Log($"Pagou {custoAtualizarAmuletos} para atualizar os amuletos/totens.");

            // A animação vai rolar automática no Update() quando detectar a mudança
            // Não precisa chamar nada extra aqui

            // A lógica é a mesma: reseta tudo e força o respawn
            interludioManager.ResetarInterludio();
            interludioManager.objetosJaEscolhidos = false;
            interludioManager.Interludio = true;
        }
        else
        {
            Debug.Log("Dinheiro insuficiente para atualizar os amuletos.");
            AvisarSemDinheiro();
        }
    }

    private void OnSairClick()
    {
        // Desativa a UI e avisa o ManagerTurno para começar o próximo turno
        FecharLoja();
        if (managerTurno != null)
        {
            // Chama o método que limpa o interlúdio e começa o próximo turno
            managerTurno.ComecoDoTurno();
        }
    }

    // --- ANIMAÇÕES E FEEDBACK VISUAL ---

    private void AtualizarDisplayDoacoes(bool imediato = false)
    {
        int valorAtual = managerPontos.Doacoes;
        if (imediato)
        {
            doacoesText.text = valorAtual.ToString() + "$";
        }
        doacoesExibidas = valorAtual;
    }

    private void AnimarMudancaDoacoes(int valorFinal)
    {
        int valorInicial = doacoesExibidas;
        doacoesExibidas = valorFinal; // Atualiza o valor que está sendo exibido

        // Animação de "contagem" do número
        DOTween.To(() => valorInicial, x => valorInicial = x, valorFinal, 0.5f)
            .OnUpdate(() => doacoesText.text = $"{valorInicial}$");

        // Animação da seta de feedback
        bool ganhouDinheiro = valorFinal > valorInicial;
        setaFeedbackImage.sprite = ganhouDinheiro ? setaCimaSprite : setaBaixoSprite;
        setaFeedbackImage.color = ganhouDinheiro ? Color.green : Color.red;

        // Para e reinicia a animação da seta se ela já estiver ativa
        if (animacaoSetaCoroutine != null) StopCoroutine(animacaoSetaCoroutine);
        animacaoSetaCoroutine = StartCoroutine(AnimarSeta());
    }

    private IEnumerator AnimarSeta()
    {
        setaFeedbackImage.gameObject.SetActive(true);
        setaFeedbackImage.DOFade(1, 0.2f); // Aparece
        yield return new WaitForSeconds(0.8f);
        setaFeedbackImage.DOFade(0, 0.3f).OnComplete(() => setaFeedbackImage.gameObject.SetActive(false)); // Desaparece
    }

    private void AvisarSemDinheiro()
    {
        // Pisca o texto de dinheiro em vermelho para dar feedback ao jogador
        doacoesText.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.3f, 10, 1);

        // Tenta com DOTween no TextMeshPro
        Color corOriginal = doacoesText.color;
        doacoesText.DOColor(Color.red, 0.15f)
            .OnComplete(() => doacoesText.DOColor(corOriginal, 0.15f));
    }

    private void FeedbackGastouDinheiro()
    {
        // Remove esse método - não precisa mais
        // A animação já acontece automaticamente no AnimarMudancaDoacoes()
    }
}