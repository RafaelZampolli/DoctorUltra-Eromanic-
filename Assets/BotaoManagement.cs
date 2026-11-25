using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;
using System.Collections;

[RequireComponent(typeof(Button))]
public class BotaoInterativo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Referências")]
    [Tooltip("Se quiser, arraste manualmente o botão aqui. Se ficar vazio, será detectado automaticamente.")]
    public Button meuBotao;

    [Tooltip("(Opcional) Se deixado em branco, um tooltip será criado automaticamente.")]
    public GameObject tooltipObject;
    private CanvasGroup tooltipCanvasGroup;
    private Tween floatTween;

    public ManagerPrancheta managerPrancheta;

    [Header("Animação de Assinatura (Opcional)")]
    [Tooltip("Arraste para cá o objeto que contém o Animator da assinatura.")]
    public Animator animatorAssinatura;

    [Header("Configurações do Tooltip (Automático)")]
    public string textoTooltip = "Clique para confirmar";
    public Vector2 offsetPosicao = new Vector2(220f, 0);
    public float duracaoFade = 0.2f;

    [Header("Estilo do Tooltip (Automático)")]
    public float tamanhoFonte = 22f;
    public Color corTexto = Color.white;
    public Color corFundoTooltip = new Color(0.2f, 0.2f, 0.25f, 1f);
    public Color corBordaTooltip = new Color(0.5f, 0.5f, 0.55f, 1f);

    [Header("Animação de Flutuação do Tooltip")]
    public float forcaFloat = 5f;
    public float duracaoFloat = 1.5f;

    [Header("Animação de Clique do Botão")]
    public float forcaAnimacao = 0.1f;
    public float duracaoAnimacao = 0.2f;

    private Vector2 initialTooltipPosition;
    [SerializeField] private bool estaInteragivelAgora = false;
    private bool tooltipVisivel = false;

    void Awake()
    {
        Debug.Log("[BotaoInterativo] Awake chamado.");

        if (meuBotao == null)
        {
            meuBotao = GetComponent<Button>();
            Debug.Log("[BotaoInterativo] meuBotao atribuído via GetComponent.");
        }

        if (meuBotao != null)
            Debug.Log("[BotaoInterativo] meuBotao NÃO é nulo.");
        else
            Debug.Log("[BotaoInterativo] meuBotao É nulo!");

        SetupTooltip();
        meuBotao.onClick.AddListener(OnBotaoClicado);
    }

    void Update()
    {
        if (meuBotao != null)
            estaInteragivelAgora = meuBotao.interactable;
        else
            estaInteragivelAgora = false;

        // DEBUG FORÇADO: Remova essa linha depois que resolver!
        // meuBotao.interactable = true;

        if (tooltipVisivel && Input.GetMouseButtonDown(0))
        {
            HideTooltip();
        }
    }

    private void SetupTooltip()
    {
        if (tooltipObject != null)
        {
            tooltipCanvasGroup = tooltipObject.GetComponent<CanvasGroup>();
            if (tooltipCanvasGroup == null)
                tooltipCanvasGroup = tooltipObject.AddComponent<CanvasGroup>();
        }
        else
        {
            GameObject panelObj = new GameObject("Tooltip Painel (Gerado)");
            panelObj.transform.SetParent(this.transform, false);

            Image bgImage = panelObj.AddComponent<Image>();
            bgImage.color = corFundoTooltip;

            Outline outline = panelObj.AddComponent<Outline>();
            outline.effectColor = corBordaTooltip;
            outline.effectDistance = new Vector2(2, -2);

            HorizontalLayoutGroup layoutGroup = panelObj.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.padding = new RectOffset(15, 15, 10, 10);
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;

            ContentSizeFitter sizeFitter = panelObj.AddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            GameObject textObj = new GameObject("Texto do Tooltip");
            textObj.transform.SetParent(panelObj.transform, false);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = textoTooltip;
            tmp.fontSize = tamanhoFonte;
            tmp.color = corTexto;
            tmp.alignment = TextAlignmentOptions.Center;

            panelObj.GetComponent<RectTransform>().anchoredPosition = offsetPosicao;
            tooltipObject = panelObj;
            tooltipCanvasGroup = panelObj.AddComponent<CanvasGroup>();
        }

        if (tooltipObject != null)
        {
            initialTooltipPosition = tooltipObject.GetComponent<RectTransform>().anchoredPosition;
            tooltipCanvasGroup.alpha = 0;
            tooltipCanvasGroup.interactable = false;
            tooltipCanvasGroup.blocksRaycasts = false;
            tooltipVisivel = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (meuBotao == null)
        {
            Debug.LogWarning("[BotaoInterativo] meuBotao está nulo em OnPointerEnter!");
            return;
        }
        if (!meuBotao.interactable)
        {
            Debug.Log("[BotaoInterativo] OnPointerEnter ignorado: botão não está interagível.");
            return;
        }
        if (tooltipCanvasGroup == null) return;

        ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipCanvasGroup == null) return;
        HideTooltip();
    }

    private void OnBotaoClicado()
    {
        if (meuBotao == null)
        {
            Debug.LogWarning("[BotaoInterativo] meuBotao está nulo em OnBotaoClicado!");
            return;
        }
        if (!meuBotao.interactable)
        {
            Debug.Log("[BotaoInterativo] Botão não está interagível. Clique ignorado.");
            return;
        }

        Debug.Log("[BotaoInterativo] Clique detectado enquanto interagível!");
        transform.DOPunchScale(new Vector3(forcaAnimacao, forcaAnimacao, forcaAnimacao), duracaoAnimacao, vibrato: 1);

        HideTooltip();

        if (animatorAssinatura == null)
        {
            Debug.LogWarning("[BotaoInterativo] Animator da assinatura não foi atribuído.");
            return;
        }

        bool triggerExists = false;
        foreach (var param in animatorAssinatura.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger && param.name == "assinatura")
                triggerExists = true;
        }

        if (!triggerExists)
        {
            Debug.LogError("[BotaoInterativo] Trigger 'assinatura' NÃO existe no Animator! Verifique os parâmetros do Animator.");
        }
        else
        {
            Debug.Log("[BotaoInterativo] Disparando trigger 'assinatura' no Animator.");
            animatorAssinatura.SetTrigger("assinatura");
            Debug.Log("[BotaoInterativo] Chamando StartCoroutine para aguardar animação e fechar prancheta.");
            StartCoroutine(AguardarAnimacaoEFecharPrancheta());
        }
    }

    private IEnumerator AguardarAnimacaoEFecharPrancheta()
    {
        Debug.Log("[BotaoInterativo] Iniciando corrotina de fechamento automático da prancheta.");
        if (animatorAssinatura == null)
        {
            Debug.LogError("[BotaoInterativo] animatorAssinatura está nulo! Não é possível aguardar animação.");
            yield break;
        }
        if (managerPrancheta == null)
        {
            Debug.LogError("[BotaoInterativo] managerPrancheta está nulo! Não é possível fechar prancheta.");
            yield break;
        }

        // (1) Espera até a animação entrar em OIN
        float tempoMaximo = 5f;
        float tempo = 0f;
        bool entrouNaAnim = false;
        while (tempo < tempoMaximo)
        {
            AnimatorStateInfo stateInfo = animatorAssinatura.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"[BotaoInterativo] Esperando entrar em 'OIN' | Ativo? {stateInfo.IsName("OIN")}, normalizedTime={stateInfo.normalizedTime:F2}");
            if (stateInfo.IsName("OIN"))
            {
                Debug.Log("[BotaoInterativo] Animação 'OIN' INICIOU!");
                entrouNaAnim = true;
                break;
            }
            tempo += Time.deltaTime;
            yield return null;
        }
        if (!entrouNaAnim)
        {
            Debug.LogWarning("[BotaoInterativo] Nunca entrou na animação 'OIN'! Timeout atingido.");
            yield break;
        }

        // (2) Agora espera a animação terminar
        while (true)
        {
            AnimatorStateInfo stateInfo = animatorAssinatura.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"[BotaoInterativo] 'OIN' rodando? {stateInfo.IsName("OIN")} | normalizedTime={stateInfo.normalizedTime:F2}");
            if (!stateInfo.IsName("OIN") || stateInfo.normalizedTime >= 1.0f)
            {
                Debug.Log("[BotaoInterativo] Animação 'OIN' TERMINOU!");
                break;
            }
            yield return null;
        }

        Debug.Log("[BotaoInterativo] Chamando managerPrancheta.FecharPrancheta()");
        managerPrancheta.FecharPrancheta();
    }

    private void ShowTooltip()
    {
        if (tooltipVisivel) return;

        tooltipCanvasGroup.blocksRaycasts = true;
        tooltipCanvasGroup.DOFade(1, duracaoFade);

        floatTween?.Kill();
        RectTransform rect = tooltipObject.GetComponent<RectTransform>();
        rect.anchoredPosition = initialTooltipPosition;
        floatTween = rect.DOAnchorPosY(initialTooltipPosition.y + forcaFloat, duracaoFloat)
                         .SetEase(Ease.InOutSine)
                         .SetLoops(-1, LoopType.Yoyo);

        tooltipVisivel = true;
    }

    private void HideTooltip()
    {
        if (!tooltipVisivel) return;

        tooltipCanvasGroup.blocksRaycasts = false;
        tooltipCanvasGroup.DOFade(0, duracaoFade);
        floatTween?.Kill();

        tooltipVisivel = false;
    }
}