using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using DG.Tweening;

public class ComportamentoCarta : MonoBehaviour
{
    [Header("Dados da Carta")]
    public CartaData dadosCarta;

    [Header("Evento Personalizado")]
    public UnityEvent OnCartaClicada;

    [Header("Configurações de Hover - APENAS DESCRIÇÃO")]
    public float tempoParaMostrarDescricao = 0.8f;
    public GameObject textoDescricao;

    [Header("Game Feel Loja")]
    public AudioClip somCompra;
    public AudioClip somErro;

    [Header("Animação de Uso da Carta")]
    public float duracaoVoo = 0.6f;
    public float duracaoDissolve = 0.8f;
    public AudioClip somUsoCarta;
    public GameObject prefabParticulasDissolve;

    private InterludioManager interludioManager;
    private ManagerPontos managerPontos;
    private ManagerMao managerMao;
    private bool ehItemDaLoja = false;
    private bool jaComprado = false;
    private bool estaUsandoCarta = false;

    private Coroutine corotinaHover;
    private bool mouseEmCima = false;
    private AudioSource audioSource;
    private CartaHoverSuave cartaHoverSuave;
    private SpriteRenderer spriteRenderer;
    private Collider2D meuCollider;
    private Canvas canvasUI;

    void Start()
    {
        if (textoDescricao != null)
            textoDescricao.SetActive(false);

        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        cartaHoverSuave = GetComponent<CartaHoverSuave>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        meuCollider = GetComponent<Collider2D>();
        canvasUI = GetComponentInChildren<Canvas>();
    }

    public void ConfigurarManager(ManagerMao manager)
    {
        this.managerMao = manager;
    }

    public void ConfigurarComoItemLoja(InterludioManager interludio, ManagerPontos pontos, ManagerMao mao)
    {
        interludioManager = interludio;
        managerPontos = pontos;
        managerMao = mao;
        ehItemDaLoja = true;
    }

    void OnMouseEnter()
    {
        if (estaUsandoCarta) return;
        mouseEmCima = true;
        if (corotinaHover != null) StopCoroutine(corotinaHover);
        corotinaHover = StartCoroutine(MostrarDescricaoComDelay());
    }

    void OnMouseExit()
    {
        if (estaUsandoCarta) return;
        mouseEmCima = false;
        if (textoDescricao != null) textoDescricao.SetActive(false);
        if (corotinaHover != null)
        {
            StopCoroutine(corotinaHover);
            corotinaHover = null;
        }
    }

    void OnMouseDown()
    {
        if (estaUsandoCarta || !mouseEmCima) return;

        if (ehItemDaLoja)
        {
            TentarComprarCarta();
        }
        else
        {
            UsarCartaDaMao();
        }
    }

    void UsarCartaDaMao()
    {
        if (dadosCarta == null) return;
        StartCoroutine(AnimacaoUsoCarta());
    }

    void ExecutarEfeitoDaCarta()
    {
        OnCartaClicada?.Invoke();
    }

    IEnumerator AnimacaoUsoCarta()
    {
        estaUsandoCarta = true;
        if (meuCollider != null) meuCollider.enabled = false;
        if (textoDescricao != null) textoDescricao.SetActive(false);
        if (cartaHoverSuave != null) cartaHoverSuave.DesabilitarHover();
        if (somUsoCarta != null && audioSource != null) audioSource.PlayOneShot(somUsoCarta);
        if (spriteRenderer != null) spriteRenderer.sortingOrder = 1000;

        Vector3 centroTela = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, 10f));
        transform.DOMove(centroTela, duracaoVoo).SetEase(Ease.InOutQuart);

        yield return new WaitForSeconds(duracaoVoo);

        ExecutarEfeitoDaCarta();

        if (CartasEffectManager.Instance != null && dadosCarta != null)
        {
            CartasEffectManager.Instance.RegistrarUltimaCartaUsada(dadosCarta);
        }

        bool foiSalvaPeloTrevo = TrevoDe4Folhas.TentarSalvarCarta();
        if (foiSalvaPeloTrevo)
        {
            // --- CORREÇÃO 1 (Linha ~165) ---
            // Usa a lógica correta de encontrar o índice e chamar com INT.
            ChamarCartaPeloNome(dadosCarta.nomeCarta);
        }

        if (managerMao != null)
        {
            managerMao.RemoverCarta(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void TentarComprarCarta()
    {
        if (jaComprado || managerPontos == null || dadosCarta == null) return;

        int precoFinal = dadosCarta.preco;

        // --- LÓGICA DO DESCONTO DO SOL ---
        if (ehItemDaLoja && CartasEffectManager.DescontoSolAtivo)
        {
            // Aplica 75% de desconto
            precoFinal = Mathf.CeilToInt(precoFinal * 0.25f);
            Debug.Log($"☀️ Desconto do Sol aplicado! Novo preço: {precoFinal}");
        }
        // ------------------------------------

        if (managerPontos.Doacoes >= precoFinal)
        {
            // --- CONSUMIR O DESCONTO ---
            // Se a compra foi bem-sucedida e o desconto foi usado, desativa ele.
            if (ehItemDaLoja && CartasEffectManager.DescontoSolAtivo)
            {
                CartasEffectManager.DescontoSolAtivo = false;
                Debug.Log("☀️ Desconto do Sol foi consumido.");
            }
            // ---------------------------

            managerPontos.Doacoes -= precoFinal;
            jaComprado = true;

            ChamarCartaPeloNome(dadosCarta.nomeCarta);

            Debug.Log($"Carta '{dadosCarta.nomeCarta}' comprada por {precoFinal} doações!");
            CompraComSucesso();
        }
        else
        {
            Debug.Log($"Dinheiro insuficiente! Precisa: {precoFinal}, Tem: {managerPontos.Doacoes}");
            FeedbackDinheiroInsuficiente();
        }
    }

    /// <summary>
    /// Método auxiliar para encontrar uma carta pelo nome na lista de prefabs do ManagerMao e chamar a criação com o índice correto.
    /// </summary>
    private void ChamarCartaPeloNome(string nomeDaCarta)
    {
        if (managerMao == null)
        {
            Debug.LogError("ManagerMao é nulo, não é possível chamar a carta!");
            return;
        }

        for (int i = 0; i < managerMao.PrefabsCartas.Count; i++)
        {
            ComportamentoCarta compPrefab = managerMao.PrefabsCartas[i].GetComponent<ComportamentoCarta>();
            if (compPrefab != null && compPrefab.dadosCarta.nomeCarta == nomeDaCarta)
            {
                // Encontrou! Chama o método com o índice INT.
                managerMao.ChamarCartaEspecifica(i);
                return; // Sai do método após encontrar e chamar.
            }
        }

        // Se o loop terminar e não encontrar, avisa.
        Debug.LogWarning($"Não foi possível encontrar o prefab da carta com o nome '{nomeDaCarta}' no ManagerMao.");
    }

    // --- O RESTO DO SEU SCRIPT (SEM MUDANÇAS) ---
    void CompraComSucesso()
    {
        if (somCompra != null) audioSource.PlayOneShot(somCompra);
        OnCartaClicada?.Invoke();
        if (cartaHoverSuave != null) cartaHoverSuave.DesabilitarHover();

        transform.DOScale(transform.localScale * 1.3f, 0.1f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => {
                // AQUI QUE ESTAVA FALTANDO!
                if (interludioManager != null)
                {
                    interludioManager.CartaComprada(gameObject);
                }
            });
    }

    void FeedbackDinheiroInsuficiente()
    {
        if (somErro != null) audioSource.PlayOneShot(somErro);
        if (cartaHoverSuave != null) cartaHoverSuave.DesabilitarHover();
        transform.DOShakePosition(0.3f, 0.1f, 10, 90, false, true);
        if (spriteRenderer != null)
        {
            spriteRenderer.DOColor(Color.red, 0.1f).SetLoops(4, LoopType.Yoyo)
                .OnComplete(() => { if (cartaHoverSuave != null) cartaHoverSuave.HabilitarHover(); });
        }
        else
        {
            StartCoroutine(ReabilitarHoverDelay());
        }
    }

    private IEnumerator ReabilitarHoverDelay()
    {
        yield return new WaitForSeconds(0.5f);
        if (cartaHoverSuave != null) cartaHoverSuave.HabilitarHover();
    }

    IEnumerator MostrarDescricaoComDelay()
    {
        yield return new WaitForSeconds(tempoParaMostrarDescricao);

        if (mouseEmCima && textoDescricao != null && dadosCarta != null && !estaUsandoCarta)
        {
            var componenteTexto = textoDescricao.GetComponent<UnityEngine.UI.Text>();
            if (componenteTexto != null)
            {
                string textoFinal = $"<b>{dadosCarta.nomeCarta}</b>\n{dadosCarta.descricao}";
                if (ehItemDaLoja)
                {
                    textoFinal += $"\n\n<b>Preço:</b> {dadosCarta.preco} doações";
                }
                componenteTexto.text = textoFinal;
            }
            textoDescricao.SetActive(true);
        }
    }

    void OnDestroy()
    {
        transform.DOKill();
        if (corotinaHover != null) StopCoroutine(corotinaHover);
    }
}