using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using System.Collections;

public class TooltipCartaPrefab : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Dados da Carta")]
    [SerializeField] private CartaData cartaData;

    [Header("Referências do Tooltip")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private Image backgroundSprite;
    [SerializeField] private TextMeshProUGUI textoNomeCarta;
    [SerializeField] private TextMeshProUGUI textoDescricao;

    [Header("Configurações")]
    [SerializeField] private Vector2 offsetPosicao = new Vector2(200f, 0f);
    [SerializeField] private float delayParaMostrar = 0.5f;
    [SerializeField] private float tempoAnimacao = 0.3f;
    [SerializeField] private bool ativarTooltip = true;

    [Header("Game Feel")]
    [SerializeField] private Vector3 escalaInicial = new Vector3(0.8f, 0.8f, 1f);
    [SerializeField] private float punchScale = 0.1f;
    [SerializeField] private bool usarShake = true;
    [SerializeField] private float shakeStrength = 2f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip somTooltipAparece;
    [SerializeField] private AudioClip somTooltipDesaparece;
    [Range(0f, 1f)]
    [SerializeField] private float volumeTooltip = 0.3f;

    private CanvasGroup canvasGroup;
    private RectTransform tooltipRect;
    private Coroutine showCoroutine;
    private Tween currentTween;
    private bool mouseEstaEm = false;
    private Canvas canvas;
    private Camera cameraUI;

    void Awake()
    {
        InicializarComponentes();
    }

    void Start()
    {
        // Esconde o tooltip no início
        EsconderImediato();

        // Atualiza conteúdo se tiver dados
        if (cartaData != null)
            AtualizarConteudo();
    }

    void InicializarComponentes()
    {
        // Configura CanvasGroup se não tiver
        if (tooltipPanel != null)
        {
            canvasGroup = tooltipPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = tooltipPanel.AddComponent<CanvasGroup>();

            tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        }

        // Busca canvas pai
        canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            cameraUI = canvas.worldCamera;
        else
            cameraUI = Camera.main;

        // Configura AudioSource se não tiver
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!ativarTooltip || cartaData == null || tooltipPanel == null)
        {
            Debug.Log("Tooltip não vai mostrar - ativarTooltip: " + ativarTooltip +
                     " cartaData: " + (cartaData != null) +
                     " tooltipPanel: " + (tooltipPanel != null));
            return;
        }

        Debug.Log("MOUSE ENTROU - Mostrando tooltip para: " + cartaData.nomeCarta);

        mouseEstaEm = true;
        PararAnimacoes();

        // Atualiza conteúdo e posição
        AtualizarConteudo();
        CalcularPosicao();

        // Mostra com delay
        showCoroutine = StartCoroutine(MostrarComDelay());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!ativarTooltip) return;

        Debug.Log("MOUSE SAIU - Escondendo tooltip");

        mouseEstaEm = false;
        PararAnimacoes();
        AnimarEsconder();
    }

    void AtualizarConteudo()
    {
        if (cartaData == null) return;

        // Atualiza textos
        if (textoNomeCarta != null)
            textoNomeCarta.text = cartaData.nomeCarta.ToUpper();

        if (textoDescricao != null)
            textoDescricao.text = cartaData.descricao;

        // Atualiza cor do fundo
        if (backgroundSprite != null)
        {
            Color corFundo = cartaData.corCarta;
            corFundo.a = 0.9f;
            backgroundSprite.color = corFundo;
        }

        Debug.Log("Conteúdo atualizado: " + cartaData.nomeCarta + " - " + cartaData.descricao);
    }

    void CalcularPosicao()
    {
        if (tooltipRect == null) return;

        // Posição da carta no mundo
        Vector3 posicaoCarta = transform.position;

        // Converte para posição de tela
        Vector3 posicaoScreen = RectTransformUtility.WorldToScreenPoint(cameraUI, posicaoCarta);

        // Aplica offset
        Vector3 novaPosicao = posicaoScreen + (Vector3)offsetPosicao;

        // Ajusta para não sair da tela
        AjustarParaTela(ref novaPosicao);

        tooltipRect.position = novaPosicao;

        Debug.Log("Posição calculada: " + novaPosicao);
    }

    void AjustarParaTela(ref Vector3 posicao)
    {
        if (tooltipRect == null) return;

        Rect rectTooltip = tooltipRect.rect;

        // Verifica limite direito
        if (posicao.x + rectTooltip.width > Screen.width)
            posicao.x = Screen.width - rectTooltip.width - 20f;

        // Verifica limite esquerdo
        if (posicao.x < 0)
            posicao.x = 20f;

        // Verifica limite superior
        if (posicao.y + rectTooltip.height > Screen.height)
            posicao.y = Screen.height - rectTooltip.height - 20f;

        // Verifica limite inferior
        if (posicao.y < 0)
            posicao.y = 20f;
    }

    IEnumerator MostrarComDelay()
    {
        yield return new WaitForSeconds(delayParaMostrar);

        if (mouseEstaEm) // Só mostra se mouse ainda estiver em cima
            AnimarMostrar();
    }

    void AnimarMostrar()
    {
        if (tooltipPanel == null) return;

        Debug.Log("ANIMANDO MOSTRAR");

        tooltipPanel.SetActive(true);

        // Estado inicial
        canvasGroup.alpha = 0f;
        tooltipRect.localScale = escalaInicial;

        // Som
        TocarSom(somTooltipAparece);

        // Animação
        Sequence sequencia = DOTween.Sequence();

        sequencia.Append(canvasGroup.DOFade(1f, tempoAnimacao).SetEase(Ease.OutQuart));
        sequencia.Join(tooltipRect.DOScale(Vector3.one, tempoAnimacao).SetEase(Ease.OutQuart));

        // Punch scale
        sequencia.Append(tooltipRect.DOPunchScale(Vector3.one * punchScale, 0.2f, 8, 0.5f));

        // Shake opcional
        if (usarShake)
            sequencia.Join(tooltipRect.DOShakePosition(0.3f, shakeStrength, 10, 90f, false, true));

        currentTween = sequencia;
    }

    void AnimarEsconder()
    {
        if (tooltipPanel == null || !tooltipPanel.activeInHierarchy) return;

        Debug.Log("ANIMANDO ESCONDER");

        // Som
        TocarSom(somTooltipDesaparece);

        // Animação rápida de saída
        Sequence sequencia = DOTween.Sequence();

        sequencia.Append(canvasGroup.DOFade(0f, 0.2f).SetEase(Ease.InQuart));
        sequencia.Join(tooltipRect.DOScale(escalaInicial, 0.2f).SetEase(Ease.InQuart));

        sequencia.OnComplete(() => tooltipPanel.SetActive(false));

        currentTween = sequencia;
    }

    void EsconderImediato()
    {
        if (tooltipPanel == null) return;

        tooltipPanel.SetActive(false);
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
        if (tooltipRect != null)
            tooltipRect.localScale = escalaInicial;
    }

    void PararAnimacoes()
    {
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
            showCoroutine = null;
        }

        if (currentTween != null)
        {
            currentTween.Kill();
            currentTween = null;
        }
    }

    void TocarSom(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.volume = volumeTooltip;
            audioSource.PlayOneShot(clip);
        }
    }

    // Método público para definir dados externamente
    public void DefinirCartaData(CartaData novaCartaData)
    {
        cartaData = novaCartaData;
        if (Application.isPlaying)
            AtualizarConteudo();
    }

    // Método para ativar/desativar
    public void DefinirTooltipAtivo(bool ativo)
    {
        ativarTooltip = ativo;
        if (!ativo && mouseEstaEm)
        {
            AnimarEsconder();
        }
    }

    void OnDisable()
    {
        PararAnimacoes();
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
        mouseEstaEm = false;
    }

    void OnDestroy()
    {
        PararAnimacoes();
    }

    // Debug
    [ContextMenu("Teste Mostrar Tooltip")]
    void DebugMostrarTooltip()
    {
        if (cartaData != null)
        {
            AtualizarConteudo();
            CalcularPosicao();
            AnimarMostrar();
        }
        else
        {
            Debug.LogWarning("CartaData não configurado!");
        }
    }

    [ContextMenu("Teste Esconder Tooltip")]
    void DebugEsconderTooltip()
    {
        AnimarEsconder();
    }
}