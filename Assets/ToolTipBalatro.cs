using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class TooltipBalatro : MonoBehaviour
{
    [Header("Dados da Carta")]
    public CartaData cartaData; // ScriptableObject com os dados

    [Header("Configurações de Posição")]
    public Vector2 offsetTooltip = new Vector2(1.2f, 0.5f);
    public float duracaoAnimacao = 0.15f;
    public float escalaInicial = 0.85f;
    [Range(0f, 1f)]
    public float dampingPosicao = 0.2f; // Suavização da posição

    [Header("Configurações de Layout")]
    [Space(5)]
    [Tooltip("Largura fixa do tooltip")]
    public float larguraTooltip = 200f;

    [Tooltip("Se marcado, usa altura fixa. Se não, ajusta automaticamente ao conteúdo")]
    public bool usarAlturaFixa = false;

    [Tooltip("Altura do tooltip (só funciona se 'Usar Altura Fixa' estiver marcado)")]
    public float alturaTooltip = 120f;

    [Tooltip("Controla o ajuste automático de altura")]
    public ContentSizeFitter.FitMode ajusteVertical = ContentSizeFitter.FitMode.PreferredSize;

    [Header("Padding e Espaçamento")]
    [Space(5)]
    [Tooltip("Padding Esquerda")]
    public int paddingEsquerda = 12;
    [Tooltip("Padding Direita")]
    public int paddingDireita = 12;
    [Tooltip("Padding Topo")]
    public int paddingTopo = 10;
    [Tooltip("Padding Baixo")]
    public int paddingBaixo = 10;
    [Tooltip("Espaçamento entre elementos")]
    public float espacamentoVertical = 6f;
    [Tooltip("Alinhamento do conteúdo")]
    public TextAnchor alinhamentoConteudo = TextAnchor.UpperLeft;

    [Header("Configurações de Texto - Nome")]
    [Space(5)]
    public TMP_FontAsset fonteNome;
    public float tamanhoFonteNome = 18f;
    public FontWeight pesoFonteNome = FontWeight.Bold;
    public Color corTextoNome = Color.white;
    public TextAlignmentOptions alinhamentoNome = TextAlignmentOptions.Left;
    public float alturaNome = 24f;

    [Header("Configurações de Texto - Descrição")]
    [Space(5)]
    public TMP_FontAsset fonteDescricao;
    public float tamanhoFonteDescricao = 13f;
    public FontWeight pesoFonteDescricao = FontWeight.Regular;
    public Color corTextoDescricao = new Color(0.85f, 0.85f, 0.85f, 1f);
    public TextAlignmentOptions alinhamentoDescricao = TextAlignmentOptions.Left;
    public float espacamentoLinhaDescricao = 2f;
    public float alturaDescricao = 50f;

    [Header("Linha Divisória")]
    [Space(5)]
    public bool mostrarLinhaDivisoria = true;
    public Color corLinhaDivisoria = new Color(1f, 1f, 1f, 0.1f);
    public float alturaLinha = 1f;

    [Header("Estilo Visual")]
    [Space(5)]
    public Color corFundo = new Color(0.1f, 0.1f, 0.15f, 0.95f);
    public Color corBorda = new Color(0.3f, 0.3f, 0.4f, 1f);
    public Vector2 offsetBorda = new Vector2(2, 2);
    public bool mostrarSombra = true;
    public Color corSombra = new Color(0, 0, 0, 0.5f);
    public Vector2 offsetSombra = new Vector2(3, -3);

    // Referências UI
    private GameObject tooltipContainer;
    private Image fundoImage;
    private TextMeshProUGUI textoNome;
    private TextMeshProUGUI textoDescricao;
    private CanvasGroup canvasGroup;
    private VerticalLayoutGroup layoutGroup;
    private ContentSizeFitter sizeFitter;
    private GameObject linhaObj;
    private RectOffset padding;

    // Canvas principal
    private Canvas mainCanvas;
    private RectTransform canvasRect;

    // Estado
    private bool tooltipAtivo = false;
    private Camera mainCamera;

    // Controle de posição
    private bool ladoDireito = true; // Se está mostrando do lado direito
    private bool ladoDefinido = false; // Se já definiu o lado nesta sessão
    private Vector2 posicaoAlvo;
    private Vector2 posicaoAtual;
    private Coroutine coroutineAtualizacao;

    void Start()
    {
        // Inicializa o RectOffset aqui para evitar erro do Unity 6
        padding = new RectOffset(paddingEsquerda, paddingDireita, paddingTopo, paddingBaixo);

        mainCamera = Camera.main;

        // Busca ou cria o Canvas principal
        mainCanvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        if (mainCanvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            mainCanvas = canvasObj.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.sortingOrder = 1000;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        canvasRect = mainCanvas.GetComponent<RectTransform>();

        // Cria o tooltip (mas deixa desativado)
        CriarTooltip();

        Debug.Log($"[TooltipBalatro] Inicializado em {gameObject.name}");
    }

    void CriarTooltip()
    {
        // Container principal
        tooltipContainer = new GameObject("Tooltip_" + gameObject.name);
        tooltipContainer.transform.SetParent(mainCanvas.transform, false);

        RectTransform rectTransform = tooltipContainer.AddComponent<RectTransform>();

        // Configura tamanho baseado nas configurações
        Vector2 tamanhoTooltip = new Vector2(larguraTooltip, usarAlturaFixa ? alturaTooltip : 100f);
        rectTransform.sizeDelta = tamanhoTooltip;
        rectTransform.pivot = new Vector2(0f, 0.5f); // Pivot inicial na esquerda-centro

        canvasGroup = tooltipContainer.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // Layout vertical configurável
        layoutGroup = tooltipContainer.AddComponent<VerticalLayoutGroup>();
        AplicarConfiguracaoLayout();

        // Fundo com borda
        fundoImage = tooltipContainer.AddComponent<Image>();
        fundoImage.color = corFundo;

        // Borda (Outline component)
        Outline outline = tooltipContainer.AddComponent<Outline>();
        outline.effectColor = corBorda;
        outline.effectDistance = offsetBorda;

        // Shadow para profundidade (opcional)
        if (mostrarSombra)
        {
            Shadow shadow = tooltipContainer.AddComponent<Shadow>();
            shadow.effectColor = corSombra;
            shadow.effectDistance = offsetSombra;
        }

        // === NOME DA CARTA ===
        CriarTextoNome();

        // === LINHA DIVISÓRIA (opcional) ===
        if (mostrarLinhaDivisoria)
        {
            CriarLinhaDivisoria();
        }

        // === DESCRIÇÃO ===
        CriarTextoDescricao();

        // ContentSizeFitter configurável
        if (!usarAlturaFixa)
        {
            sizeFitter = tooltipContainer.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ajusteVertical;
        }

        // Desativa inicialmente
        tooltipContainer.SetActive(false);

        Debug.Log($"[TooltipBalatro] Tooltip criado com layout customizado!");
    }

    void AplicarConfiguracaoLayout()
    {
        if (layoutGroup == null) return;

        // Atualiza o RectOffset com os valores atuais
        if (padding == null)
            padding = new RectOffset(paddingEsquerda, paddingDireita, paddingTopo, paddingBaixo);
        else
        {
            padding.left = paddingEsquerda;
            padding.right = paddingDireita;
            padding.top = paddingTopo;
            padding.bottom = paddingBaixo;
        }

        layoutGroup.spacing = espacamentoVertical;
        layoutGroup.padding = padding;
        layoutGroup.childControlHeight = false;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childAlignment = alinhamentoConteudo;
    }

    void CriarTextoNome()
    {
        GameObject nomeObj = new GameObject("Nome");
        nomeObj.transform.SetParent(tooltipContainer.transform, false);

        textoNome = nomeObj.AddComponent<TextMeshProUGUI>();
        textoNome.text = "Nome da Carta";
        textoNome.fontSize = tamanhoFonteNome;
        textoNome.fontWeight = pesoFonteNome;
        textoNome.color = corTextoNome;
        textoNome.alignment = alinhamentoNome;

        // Aplica fonte do nome se configurada
        if (fonteNome != null)
            textoNome.font = fonteNome;

        RectTransform nomeRect = nomeObj.GetComponent<RectTransform>();
        nomeRect.sizeDelta = new Vector2(0, alturaNome);
    }

    void CriarLinhaDivisoria()
    {
        linhaObj = new GameObject("Linha");
        linhaObj.transform.SetParent(tooltipContainer.transform, false);

        Image linhaImage = linhaObj.AddComponent<Image>();
        linhaImage.color = corLinhaDivisoria;

        RectTransform linhaRect = linhaObj.GetComponent<RectTransform>();
        linhaRect.sizeDelta = new Vector2(0, alturaLinha);
    }

    void CriarTextoDescricao()
    {
        GameObject descObj = new GameObject("Descricao");
        descObj.transform.SetParent(tooltipContainer.transform, false);

        textoDescricao = descObj.AddComponent<TextMeshProUGUI>();
        textoDescricao.text = "Descrição da carta";
        textoDescricao.fontSize = tamanhoFonteDescricao;
        textoDescricao.fontWeight = pesoFonteDescricao;
        textoDescricao.color = corTextoDescricao;
        textoDescricao.alignment = alinhamentoDescricao;
        textoDescricao.lineSpacing = espacamentoLinhaDescricao;

        // Aplica fonte da descrição se configurada
        if (fonteDescricao != null)
            textoDescricao.font = fonteDescricao;

        RectTransform descRect = descObj.GetComponent<RectTransform>();
        descRect.sizeDelta = new Vector2(0, alturaDescricao);
    }

    System.Collections.IEnumerator AtualizarPosicaoSuave()
    {
        while (tooltipAtivo)
        {
            if (mainCamera != null && tooltipContainer != null)
            {
                // Calcula posição desejada
                CalcularPosicaoTooltip();

                // Suaviza o movimento
                posicaoAtual = Vector2.Lerp(posicaoAtual, posicaoAlvo, dampingPosicao);

                RectTransform tooltipRect = tooltipContainer.GetComponent<RectTransform>();
                tooltipRect.anchoredPosition = posicaoAtual;
            }

            yield return null;
        }
    }

    void CalcularPosicaoTooltip()
    {
        // Define o lado apenas uma vez por sessão
        if (!ladoDefinido)
        {
            // Calcula posição inicial
            Vector3 posInicial = transform.position + new Vector3(offsetTooltip.x, offsetTooltip.y, 0);
            Vector2 telaInicial = RectTransformUtility.WorldToScreenPoint(mainCamera, posInicial);

            // Verifica se cabe do lado direito
            float larguraEstimada = larguraTooltip + 20; // margem de segurança
            ladoDireito = (telaInicial.x + larguraEstimada) < Screen.width;
            ladoDefinido = true;

            // Define o pivot baseado no lado escolhido
            RectTransform rectTooltip = tooltipContainer.GetComponent<RectTransform>();
            rectTooltip.pivot = ladoDireito ? new Vector2(0f, 0.5f) : new Vector2(1f, 0.5f);
        }

        // Calcula posição baseada no lado definido
        float offsetX = ladoDireito ? offsetTooltip.x : -offsetTooltip.x - 0.5f;
        Vector3 posFinal = transform.position + new Vector3(offsetX, offsetTooltip.y, 0);
        Vector2 telaFinal = RectTransformUtility.WorldToScreenPoint(mainCamera, posFinal);

        // Converte para posição no Canvas
        Vector2 posicaoLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            telaFinal,
            mainCanvas.worldCamera,
            out posicaoLocal
        );

        posicaoAlvo = posicaoLocal;

        // Clamp vertical para não sair da tela
        RectTransform rectClamp = tooltipContainer.GetComponent<RectTransform>();
        float alturaTooltip = rectClamp.sizeDelta.y;
        float minY = -canvasRect.sizeDelta.y * 0.5f + alturaTooltip * 0.5f + 10;
        float maxY = canvasRect.sizeDelta.y * 0.5f - alturaTooltip * 0.5f - 10;
        posicaoAlvo.y = Mathf.Clamp(posicaoAlvo.y, minY, maxY);
    }

    // Método para atualizar layout em runtime (útil para mudanças pelo Inspector)
    public void AtualizarLayout()
    {
        if (tooltipContainer == null) return;

        // Atualiza tamanho do container
        RectTransform rectTransform = tooltipContainer.GetComponent<RectTransform>();
        Vector2 novoTamanho = new Vector2(larguraTooltip, usarAlturaFixa ? alturaTooltip : rectTransform.sizeDelta.y);
        rectTransform.sizeDelta = novoTamanho;

        // Atualiza layout group
        AplicarConfiguracaoLayout();

        // Atualiza ContentSizeFitter
        if (sizeFitter != null)
        {
            if (usarAlturaFixa)
            {
                Destroy(sizeFitter);
                sizeFitter = null;
            }
            else
            {
                sizeFitter.verticalFit = ajusteVertical;
            }
        }
        else if (!usarAlturaFixa)
        {
            sizeFitter = tooltipContainer.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ajusteVertical;
        }

        // Atualiza textos
        if (textoNome != null)
        {
            textoNome.fontSize = tamanhoFonteNome;
            textoNome.fontWeight = pesoFonteNome;
            textoNome.color = corTextoNome;
            textoNome.alignment = alinhamentoNome;
            if (fonteNome != null) textoNome.font = fonteNome;

            RectTransform nomeRect = textoNome.GetComponent<RectTransform>();
            nomeRect.sizeDelta = new Vector2(0, alturaNome);
        }

        if (textoDescricao != null)
        {
            textoDescricao.fontSize = tamanhoFonteDescricao;
            textoDescricao.fontWeight = pesoFonteDescricao;
            textoDescricao.color = corTextoDescricao;
            textoDescricao.alignment = alinhamentoDescricao;
            textoDescricao.lineSpacing = espacamentoLinhaDescricao;
            if (fonteDescricao != null) textoDescricao.font = fonteDescricao;

            RectTransform descRect = textoDescricao.GetComponent<RectTransform>();
            descRect.sizeDelta = new Vector2(0, alturaDescricao);
        }

        // Atualiza linha divisória
        if (linhaObj != null)
        {
            linhaObj.SetActive(mostrarLinhaDivisoria);
            if (mostrarLinhaDivisoria)
            {
                Image linhaImage = linhaObj.GetComponent<Image>();
                linhaImage.color = corLinhaDivisoria;

                RectTransform linhaRect = linhaObj.GetComponent<RectTransform>();
                linhaRect.sizeDelta = new Vector2(0, alturaLinha);
            }
        }

        // Atualiza estilo visual
        if (fundoImage != null)
            fundoImage.color = corFundo;

        Outline outline = tooltipContainer.GetComponent<Outline>();
        if (outline != null)
        {
            outline.effectColor = corBorda;
            outline.effectDistance = offsetBorda;
        }

        Shadow shadow = tooltipContainer.GetComponent<Shadow>();
        if (shadow != null)
        {
            if (mostrarSombra)
            {
                shadow.effectColor = corSombra;
                shadow.effectDistance = offsetSombra;
            }
            else
            {
                Destroy(shadow);
            }
        }
        else if (mostrarSombra)
        {
            shadow = tooltipContainer.AddComponent<Shadow>();
            shadow.effectColor = corSombra;
            shadow.effectDistance = offsetSombra;
        }

        Debug.Log($"[TooltipBalatro] Layout atualizado!");
    }

    private void OnMouseEnter()
    {
        if (tooltipContainer == null) return;

        Debug.Log($"[TooltipBalatro] Mouse Enter - CartaData: {(cartaData != null ? cartaData.nomeCarta : "null")}");
        MostrarTooltip();
    }

    private void OnMouseExit()
    {
        if (tooltipContainer == null) return;

        Debug.Log($"[TooltipBalatro] Mouse Exit");
        EsconderTooltip();
    }

    public void MostrarTooltip()
    {
        if (tooltipAtivo) return;

        tooltipAtivo = true;
        ladoDefinido = false; // Reset do lado para recalcular
        tooltipContainer.SetActive(true);

        // Atualiza textos com dados do ScriptableObject
        if (cartaData != null)
        {
            if (textoNome != null)
                textoNome.text = cartaData.nomeCarta;

            if (textoDescricao != null)
            {
                textoDescricao.text = cartaData.descricao;
                textoDescricao.gameObject.SetActive(!string.IsNullOrEmpty(cartaData.descricao));
            }

            // Atualiza cor da borda com a cor da carta (se não for branca)
            Outline outline = tooltipContainer.GetComponent<Outline>();
            if (outline != null && cartaData.corCarta != Color.white)
            {
                Color corBordaCustom = cartaData.corCarta;
                corBordaCustom.a = 1f;
                outline.effectColor = corBordaCustom;
            }
        }
        else
        {
            // Valores padrão se não tiver ScriptableObject
            if (textoNome != null) textoNome.text = "Carta";
            if (textoDescricao != null)
            {
                textoDescricao.text = "Sem descrição";
                textoDescricao.gameObject.SetActive(false);
            }
        }

        // Calcula posição inicial
        CalcularPosicaoTooltip();
        posicaoAtual = posicaoAlvo;
        RectTransform rect = tooltipContainer.GetComponent<RectTransform>();
        rect.anchoredPosition = posicaoAtual;

        // Inicia coroutine de atualização suave
        if (coroutineAtualizacao != null)
            StopCoroutine(coroutineAtualizacao);
        coroutineAtualizacao = StartCoroutine(AtualizarPosicaoSuave());

        // Animação de entrada com mais juice
        tooltipContainer.transform.localScale = Vector3.one * escalaInicial;

        // Fade in
        DOTween.Kill(canvasGroup);
        canvasGroup.DOFade(1f, duracaoAnimacao).SetEase(Ease.OutQuad);

        // Scale bounce com overshoot
        DOTween.Kill(tooltipContainer.transform);
        tooltipContainer.transform.DOScale(1f, duracaoAnimacao)
            .SetEase(Ease.OutBack, 1.5f); // Overshoot maior para mais bounce

        // Pequeno slide baseado no lado
        Vector2 offsetSlide = ladoDireito ? new Vector2(-15, 0) : new Vector2(15, 0);
        Vector2 posOriginal = rect.anchoredPosition;
        rect.anchoredPosition = posOriginal + offsetSlide;

        DOTween.Kill(rect);
        rect.DOAnchorPos(posOriginal, duracaoAnimacao)
            .SetEase(Ease.OutExpo); // Ease mais snappy

        // Micro-shake para impacto (game feel!)
        tooltipContainer.transform.DOShakeRotation(duracaoAnimacao * 0.5f,
            strength: 2f,
            vibrato: 10,
            randomness: 90)
            .SetDelay(duracaoAnimacao * 0.3f);

        Debug.Log($"[TooltipBalatro] Tooltip mostrado no lado {(ladoDireito ? "direito" : "esquerdo")}!");
    }

    public void EsconderTooltip()
    {
        if (!tooltipAtivo) return;

        tooltipAtivo = false;
        ladoDefinido = false; // Reset para próxima vez

        // Para a coroutine de atualização
        if (coroutineAtualizacao != null)
        {
            StopCoroutine(coroutineAtualizacao);
            coroutineAtualizacao = null;
        }

        // Kill tweens anteriores
        DOTween.Kill(canvasGroup);
        DOTween.Kill(tooltipContainer.transform);
        DOTween.Kill(tooltipContainer.GetComponent<RectTransform>());

        // Animação de saída mais rápida e snappy
        canvasGroup.DOFade(0f, duracaoAnimacao * 0.6f)
            .SetEase(Ease.InQuad)
            .OnComplete(() => {
                if (tooltipContainer != null)
                    tooltipContainer.SetActive(false);
            });

        // Scale down com ease diferente
        tooltipContainer.transform.DOScale(escalaInicial * 0.95f, duracaoAnimacao * 0.6f)
            .SetEase(Ease.InBack, 0.5f);

        // Slide de saída sutil
        RectTransform rect = tooltipContainer.GetComponent<RectTransform>();
        Vector2 offsetSaida = ladoDireito ? new Vector2(-10, 0) : new Vector2(10, 0);
        rect.DOAnchorPos(rect.anchoredPosition + offsetSaida, duracaoAnimacao * 0.6f)
            .SetEase(Ease.InQuad);

        Debug.Log($"[TooltipBalatro] Tooltip escondido!");
    }

    // Método para trocar carta em runtime
    public void TrocarCarta(CartaData novaCarta)
    {
        cartaData = novaCarta;

        // Se tooltip estiver visível, atualiza imediatamente
        if (tooltipAtivo)
        {
            EsconderTooltip();
            Invoke(nameof(MostrarTooltip), 0.1f);
        }
    }

    void OnDestroy()
    {
        if (coroutineAtualizacao != null)
        {
            StopCoroutine(coroutineAtualizacao);
        }

        if (tooltipContainer != null)
        {
            DOTween.Kill(tooltipContainer.transform);
            DOTween.Kill(canvasGroup);
            if (tooltipContainer.GetComponent<RectTransform>() != null)
                DOTween.Kill(tooltipContainer.GetComponent<RectTransform>());

            Destroy(tooltipContainer);
        }
    }

#if UNITY_EDITOR
    // Atualiza em tempo de edição quando mudar configurações no Inspector
    void OnValidate()
    {
        if (Application.isPlaying && tooltipContainer != null)
        {
            // Aplica mudanças de layout em tempo real
            AtualizarLayout();
        }
    }
#endif
}