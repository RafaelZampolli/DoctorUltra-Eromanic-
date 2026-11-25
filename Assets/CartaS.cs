using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

public class TooltipCarta : MonoBehaviour
{
    [Header("Configurações do Tooltip")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private Image backgroundSprite;
    [SerializeField] private TextMeshProUGUI textoDescricao;
    [SerializeField] private TextMeshProUGUI textoNomeCarta;

    [Header("Configurações de Posicionamento")]
    [SerializeField] private Vector2 offsetPosicao = new Vector2(200f, 0f);
    [SerializeField] private bool seguirMouse = false;

    [Header("Configurações de Animação")]
    [SerializeField] private float tempoFadeIn = 0.3f;
    [SerializeField] private float delayParaMostrar = 0.5f;
    [SerializeField] private float tempoFadeOut = 0.2f;
    [SerializeField] private Ease easeIn = Ease.OutQuart;
    [SerializeField] private Ease easeOut = Ease.InQuart;

    [Header("Game Feel")]
    [SerializeField] private Vector3 escalaInicial = new Vector3(0.8f, 0.8f, 1f);
    [SerializeField] private Vector3 escalaFinal = new Vector3(1f, 1f, 1f);
    [SerializeField] private float punchScale = 0.1f;
    [SerializeField] private bool usarShake = true;
    [SerializeField] private float shakeStrength = 2f;
    [SerializeField] private int shakeVibrato = 10;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip somTooltipAparece;
    [SerializeField] private AudioClip somTooltipDesaparece;
    [Range(0f, 1f)]
    [SerializeField] private float volumeTooltip = 0.3f;

    private Coroutine showTooltipCoroutine;
    private Tween currentTween;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Camera cameraUI;

    // Singleton simples para facilitar acesso
    public static TooltipCarta Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InicializarComponentes();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InicializarComponentes()
    {
        // Configura os componentes necessários
        if (tooltipPanel == null)
            tooltipPanel = gameObject;

        canvasGroup = tooltipPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = tooltipPanel.AddComponent<CanvasGroup>();

        rectTransform = tooltipPanel.GetComponent<RectTransform>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // Busca a camera UI automaticamente
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            cameraUI = canvas.worldCamera;
        else
            cameraUI = Camera.main;

        // Inicia invisível
        EsconderImediato();
    }

    public void MostrarTooltip(CartaData cartaData, Transform cartaTransform)
    {
        if (cartaData == null) return;

        // Para qualquer animação em andamento
        PararAnimacoes();

        // Atualiza o conteúdo
        AtualizarConteudo(cartaData);

        // Calcula posição
        CalcularPosicao(cartaTransform);

        // Inicia a animação com delay
        showTooltipCoroutine = StartCoroutine(MostrarComDelay());
    }

    public void EsconderTooltip()
    {
        PararAnimacoes();
        AnimarEsconder();
    }

    void AtualizarConteudo(CartaData cartaData)
    {
        if (textoNomeCarta != null)
            textoNomeCarta.text = cartaData.nomeCarta.ToUpper();

        if (textoDescricao != null)
            textoDescricao.text = cartaData.descricao;

        // Muda a cor do fundo baseado na cor da carta
        if (backgroundSprite != null)
        {
            Color corFundo = cartaData.corCarta;
            corFundo.a = 0.9f; // Mantém um pouco de transparência
            backgroundSprite.color = corFundo;
        }
    }

    void CalcularPosicao(Transform cartaTransform)
    {
        if (cartaTransform == null) return;

        Vector3 posicaoCarta = cartaTransform.position;
        Vector3 novaPosicao;

        if (seguirMouse)
        {
            // Segue o mouse com offset
            Vector3 posicaoMouse = Input.mousePosition;
            novaPosicao = posicaoMouse + (Vector3)offsetPosicao;
        }
        else
        {
            // Posiciona ao lado da carta
            Vector3 posicaoScreen = RectTransformUtility.WorldToScreenPoint(cameraUI, posicaoCarta);
            novaPosicao = posicaoScreen + (Vector3)offsetPosicao;

            // Ajusta para não sair da tela
            AjustarParaTela(ref novaPosicao);
        }

        rectTransform.position = novaPosicao;
    }

    void AjustarParaTela(ref Vector3 posicao)
    {
        Rect rectTooltip = RectTransformUtility.PixelAdjustRect(rectTransform, GetComponentInParent<Canvas>());

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
        AnimarMostrar();
    }

    void AnimarMostrar()
    {
        tooltipPanel.SetActive(true);

        // Estado inicial
        canvasGroup.alpha = 0f;
        rectTransform.localScale = escalaInicial;

        // Som de aparição
        TocarSom(somTooltipAparece);

        // Sequência de animação com game feel
        Sequence sequencia = DOTween.Sequence();

        // Fade in + scale
        sequencia.Append(canvasGroup.DOFade(1f, tempoFadeIn).SetEase(easeIn));
        sequencia.Join(rectTransform.DOScale(escalaFinal, tempoFadeIn).SetEase(easeIn));

        // Punch scale para dar aquele "pop"
        sequencia.Append(rectTransform.DOPunchScale(Vector3.one * punchScale, 0.2f, 8, 0.5f));

        // Shake sutil opcional
        if (usarShake)
            sequencia.Join(rectTransform.DOShakePosition(0.3f, shakeStrength, shakeVibrato, 90f, false, true));

        currentTween = sequencia;
    }

    void AnimarEsconder()
    {
        if (!tooltipPanel.activeInHierarchy) return;

        // Som de desaparição
        TocarSom(somTooltipDesaparece);

        // Animação de saída rápida
        Sequence sequencia = DOTween.Sequence();

        sequencia.Append(canvasGroup.DOFade(0f, tempoFadeOut).SetEase(easeOut));
        sequencia.Join(rectTransform.DOScale(escalaInicial, tempoFadeOut).SetEase(easeOut));

        sequencia.OnComplete(() => tooltipPanel.SetActive(false));

        currentTween = sequencia;
    }

    void EsconderImediato()
    {
        tooltipPanel.SetActive(false);
        canvasGroup.alpha = 0f;
        rectTransform.localScale = escalaInicial;
    }

    void PararAnimacoes()
    {
        if (showTooltipCoroutine != null)
        {
            StopCoroutine(showTooltipCoroutine);
            showTooltipCoroutine = null;
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

    void Update()
    {
        // Se estiver seguindo o mouse, atualiza posição
        if (seguirMouse && tooltipPanel.activeInHierarchy)
        {
            Vector3 posicaoMouse = Input.mousePosition + (Vector3)offsetPosicao;
            AjustarParaTela(ref posicaoMouse);
            rectTransform.position = posicaoMouse;
        }
    }

    void OnDestroy()
    {
        PararAnimacoes();
        if (Instance == this)
            Instance = null;
    }
}