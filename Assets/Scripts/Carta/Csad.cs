using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TooltipCartaSistema : MonoBehaviour
{
    [Header("Configurações Tooltip")]
    public float delayParaMostrar = 0.5f;
    public Vector2 offsetTooltip = new Vector2(2f, 1f); // Offset da carta
    public Vector2 tamanhoTooltip = new Vector2(300f, 150f);

    [Header("Estilo Visual")]
    public Color corFundo = new Color(0.1f, 0.1f, 0.15f, 0.95f);
    public Color corTexto = Color.white;
    public Color corBorda = new Color(0.8f, 0.6f, 0.3f, 1f);
    public float espessuraBorda = 2f;

    [Header("Animação")]
    public float duracaoEntrada = 0.3f;
    public float duracaoSaida = 0.2f;

    // Componentes do tooltip
    private GameObject tooltipObject;
    private Image fundoTooltip;
    private Image bordaTooltip;
    private Text textoDescricao;
    private CanvasGroup canvasGroup;

    // Controle
    private bool tooltipVisivel = false;
    private Coroutine corotinaDelay;
    private Sequence animacaoTooltip;

    void Start()
    {
        CriarTooltip();
    }

    void CriarTooltip()
    {
        // Encontra ou cria canvas
        Canvas canvasUI = FindObjectOfType<Canvas>();
        if (canvasUI == null)
        {
            Debug.LogError("Nenhum Canvas encontrado na cena para o tooltip!");
            return;
        }

        // OBJETO PRINCIPAL DO TOOLTIP
        tooltipObject = new GameObject($"Tooltip_{gameObject.name}");
        tooltipObject.transform.SetParent(canvasUI.transform, false);

        // CANVAS GROUP para fade
        canvasGroup = tooltipObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        // RECT TRANSFORM
        RectTransform tooltipRect = tooltipObject.AddComponent<RectTransform>();
        tooltipRect.sizeDelta = tamanhoTooltip;

        // FUNDO
        fundoTooltip = tooltipObject.AddComponent<Image>();
        fundoTooltip.color = corFundo;
        fundoTooltip.raycastTarget = false;

        // BORDA (outline)
        Outline outline = tooltipObject.AddComponent<Outline>();
        outline.effectColor = corBorda;
        outline.effectDistance = Vector2.one * espessuraBorda;

        // TEXTO
        GameObject textoObj = new GameObject("TextoDescricao");
        textoObj.transform.SetParent(tooltipObject.transform, false);

        textoDescricao = textoObj.AddComponent<Text>();
        textoDescricao.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoDescricao.fontSize = 14;
        textoDescricao.color = corTexto;
        textoDescricao.alignment = TextAnchor.MiddleCenter;
        textoDescricao.raycastTarget = false;

        // RECT TRANSFORM do texto (preenche o tooltip)
        RectTransform textoRect = textoObj.GetComponent<RectTransform>();
        textoRect.anchorMin = Vector2.zero;
        textoRect.anchorMax = Vector2.one;
        textoRect.offsetMin = Vector2.one * 10f; // Padding
        textoRect.offsetMax = Vector2.one * -10f;

        // Começa escondido
        tooltipObject.SetActive(false);

        Debug.Log($"[{gameObject.name}] Tooltip criado!");
    }

    public void MostrarTooltip(string textoParaMostrar)
    {
        if (tooltipObject == null || tooltipVisivel) return;

        // Para delay anterior
        if (corotinaDelay != null)
        {
            StopCoroutine(corotinaDelay);
        }

        corotinaDelay = StartCoroutine(MostrarComDelay(textoParaMostrar));
    }

    public void EsconderTooltip()
    {
        if (tooltipObject == null || !tooltipVisivel) return;

        // Para delay se tiver
        if (corotinaDelay != null)
        {
            StopCoroutine(corotinaDelay);
            corotinaDelay = null;
        }

        tooltipVisivel = false;

        // Animação de saída
        animacaoTooltip?.Kill();
        animacaoTooltip = DOTween.Sequence();

        animacaoTooltip.Append(canvasGroup.DOFade(0f, duracaoSaida)
            .SetEase(Ease.OutQuart));

        animacaoTooltip.Join(tooltipObject.transform.DOScale(0.8f, duracaoSaida)
            .SetEase(Ease.InBack));

        animacaoTooltip.OnComplete(() => {
            tooltipObject.SetActive(false);
        });
    }

    private System.Collections.IEnumerator MostrarComDelay(string texto)
    {
        yield return new WaitForSeconds(delayParaMostrar);

        if (!tooltipVisivel) // Só continua se ainda não foi cancelado
        {
            ExibirTooltip(texto);
        }
    }

    private void ExibirTooltip(string texto)
    {
        if (tooltipObject == null) return;

        tooltipVisivel = true;

        // Configura texto
        textoDescricao.text = texto;

        // Posição do tooltip
        AtualizarPosicaoTooltip();

        // Ativa objeto
        tooltipObject.SetActive(true);

        // Animação de entrada
        animacaoTooltip?.Kill();
        canvasGroup.alpha = 0f;
        tooltipObject.transform.localScale = Vector3.one * 0.7f;

        animacaoTooltip = DOTween.Sequence();

        animacaoTooltip.Append(canvasGroup.DOFade(1f, duracaoEntrada)
            .SetEase(Ease.OutQuart));

        animacaoTooltip.Join(tooltipObject.transform.DOScale(Vector3.one, duracaoEntrada)
            .SetEase(Ease.OutElastic, 1.2f, 0.8f));

        // Micro bounce de entrada
        animacaoTooltip.AppendCallback(() => {
            tooltipObject.transform.DOPunchScale(Vector3.one * 0.05f, 0.2f, 6, 0.8f);
        });

        Debug.Log($"[{gameObject.name}] Tooltip exibido: {texto}");
    }

    void Update()
    {
        // Atualiza posição do tooltip se estiver visível
        if (tooltipVisivel && tooltipObject != null && tooltipObject.activeSelf)
        {
            AtualizarPosicaoTooltip();
        }
    }

    private void AtualizarPosicaoTooltip()
    {
        if (tooltipObject == null || Camera.main == null) return;

        // Posição da carta + offset pro lado direito (como na imagem)
        Vector3 posicaoMundo = transform.position + new Vector3(offsetTooltip.x, offsetTooltip.y, 0);
        Vector2 posicaoTela = Camera.main.WorldToScreenPoint(posicaoMundo);

        // Ajusta pra coordenadas do Canvas
        Canvas canvas = tooltipObject.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                tooltipObject.transform.position = posicaoTela;
            }
            else
            {
                // Pra Canvas World Space
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    posicaoTela,
                    Camera.main,
                    out Vector2 localPoint);
                tooltipObject.transform.localPosition = localPoint;
            }

            // MANTÉM TOOLTIP NA TELA (evita sair pelas bordas)
            RectTransform tooltipRect = tooltipObject.GetComponent<RectTransform>();
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();

            Vector3 pos = tooltipObject.transform.localPosition;

            // Margem de segurança
            float margem = 20f;
            float halfWidth = tooltipRect.sizeDelta.x / 2f;
            float halfHeight = tooltipRect.sizeDelta.y / 2f;

            // Limita X (horizontal)
            pos.x = Mathf.Clamp(pos.x, -canvasRect.sizeDelta.x / 2f + halfWidth + margem,
                                       canvasRect.sizeDelta.x / 2f - halfWidth - margem);

            // Limita Y (vertical) 
            pos.y = Mathf.Clamp(pos.y, -canvasRect.sizeDelta.y / 2f + halfHeight + margem,
                                       canvasRect.sizeDelta.y / 2f - halfHeight - margem);

            tooltipObject.transform.localPosition = pos;
        }
    }

    void OnDestroy()
    {
        animacaoTooltip?.Kill();
        if (corotinaDelay != null)
            StopCoroutine(corotinaDelay);

        if (tooltipObject != null)
            Destroy(tooltipObject);
    }
}