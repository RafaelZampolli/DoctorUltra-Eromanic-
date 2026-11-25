using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class TooltipAmuleto : MonoBehaviour
{
    [Header("Dados do Amuleto")]
    public string nomeAmuleto = "Amuleto Misterioso";
    [TextArea(3, 6)]
    public string descricaoAmuleto = "Um amuleto com poderes místicos.";
    public Color corAmuleto = Color.yellow;

    [Header("Configurações")]
    public float larguraTooltip = 220f;
    public float duracaoAnimacao = 0.3f;
    public InterludioManager interludioManager;

    // Referências
    private GameObject tooltipContainer;
    private TextMeshProUGUI textoNome;
    private TextMeshProUGUI textoDescricao;
    private Canvas mainCanvas;
    private bool tooltipAtivo = false;
    private bool mouseEmCima = false;

    void Start()
    {
        Debug.Log($"[TooltipAmuleto] START iniciado para {gameObject.name}");

        // Busca Canvas
        mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("[TooltipAmuleto] Nenhum Canvas encontrado na cena!");
            return;
        }

        Debug.Log($"[TooltipAmuleto] Canvas encontrado: {mainCanvas.name}");

        // Busca InterludioManager
        if (interludioManager == null)
        {
            interludioManager = FindObjectOfType<InterludioManager>();
        }

        CriarTooltipSimples();

        Debug.Log($"[TooltipAmuleto] Inicializado com sucesso para {gameObject.name}");
    }

    void Update()
    {
        // Só verifica mouse se estiver nos interlúdios
        if (!EstaNosInterludios()) return;

        // Raycast 2D manual para detectar mouse
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        bool mouseNestObjeto = hit.collider != null && hit.collider.gameObject == gameObject;

        // Mouse entrou
        if (mouseNestObjeto && !mouseEmCima)
        {
            mouseEmCima = true;
            Debug.Log($"[TooltipAmuleto] Mouse Enter - {nomeAmuleto}");
            MostrarTooltip();
        }
        // Mouse saiu
        else if (!mouseNestObjeto && mouseEmCima)
        {
            mouseEmCima = false;
            Debug.Log($"[TooltipAmuleto] Mouse Exit");
            EsconderTooltip();
        }
    }

    void CriarTooltipSimples()
    {
        Debug.Log("[TooltipAmuleto] Criando tooltip...");

        // Container principal
        tooltipContainer = new GameObject("TooltipAmuleto_" + gameObject.name);
        tooltipContainer.transform.SetParent(mainCanvas.transform, false);
        tooltipContainer.layer = 10;

        // RectTransform
        RectTransform rect = tooltipContainer.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(larguraTooltip, 100f);
        rect.anchoredPosition = Vector2.zero;

        // Fundo
        Image fundo = tooltipContainer.AddComponent<Image>();
        fundo.color = new Color(0.1f, 0.1f, 0.2f, 0.9f);

        // Layout vertical
        VerticalLayoutGroup layout = tooltipContainer.AddComponent<VerticalLayoutGroup>();
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.spacing = 5f;
        layout.padding = new RectOffset(10, 10, 8, 8);

        // Nome
        GameObject nomeObj = new GameObject("Nome");
        nomeObj.transform.SetParent(tooltipContainer.transform, false);
        textoNome = nomeObj.AddComponent<TextMeshProUGUI>();
        textoNome.text = nomeAmuleto;
        textoNome.fontSize = 16f;
        textoNome.color = Color.white;
        textoNome.fontStyle = FontStyles.Bold;

        // Descrição
        GameObject descObj = new GameObject("Descricao");
        descObj.transform.SetParent(tooltipContainer.transform, false);
        textoDescricao = descObj.AddComponent<TextMeshProUGUI>();
        textoDescricao.text = descricaoAmuleto;
        textoDescricao.fontSize = 12f;
        textoDescricao.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        // ContentSizeFitter
        ContentSizeFitter sizeFitter = tooltipContainer.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Começa invisível
        tooltipContainer.SetActive(false);

        Debug.Log("[TooltipAmuleto] Tooltip criado com sucesso!");
    }

    // Remove os métodos OnMouseEnter e OnMouseExit - não usa mais

    private bool EstaNosInterludios()
    {
        if (interludioManager == null) return false;
        return interludioManager.Interludio || interludioManager.objetosJaEscolhidos;
    }

    public void MostrarTooltip()
    {
        if (tooltipAtivo || tooltipContainer == null) return;

        Debug.Log($"[TooltipAmuleto] Mostrando tooltip para {nomeAmuleto}");

        tooltipAtivo = true;
        tooltipContainer.SetActive(true);

        // Posição simples no centro da tela
        RectTransform rect = tooltipContainer.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;

        // Atualiza textos
        textoNome.text = nomeAmuleto;
        textoDescricao.text = descricaoAmuleto;

        // Animação simples
        tooltipContainer.transform.localScale = Vector3.zero;
        tooltipContainer.transform.DOScale(1f, duracaoAnimacao).SetEase(Ease.OutBack);

        Debug.Log($"[TooltipAmuleto] Tooltip ativo: {tooltipContainer.activeSelf}");
        Debug.Log($"[TooltipAmuleto] Posição: {rect.anchoredPosition}");
    }

    public void EsconderTooltip()
    {
        if (!tooltipAtivo || tooltipContainer == null) return;

        Debug.Log($"[TooltipAmuleto] Escondendo tooltip");

        tooltipAtivo = false;

        tooltipContainer.transform.DOScale(0f, duracaoAnimacao * 0.5f)
            .SetEase(Ease.InBack)
            .OnComplete(() => {
                if (tooltipContainer != null)
                    tooltipContainer.SetActive(false);
            });
    }

    void OnDestroy()
    {
        if (tooltipContainer != null)
        {
            DOTween.Kill(tooltipContainer.transform);
            Destroy(tooltipContainer);
        }
    }
}