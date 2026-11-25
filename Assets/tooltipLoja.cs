using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Cria um tooltip dinâmico para uma carta da loja ao passar o mouse.
/// Usa OnMouseEnter/OnMouseExit para detectar o mouse em objetos com Collider2D.
/// Puxa os dados de um 'CartaData' e mostra o preço com desconto se 'CaixaDeBaralho' estiver ativa.
/// </summary>
public class ShopCardTooltip : MonoBehaviour
{
    [Header("Dados da Carta")]
    [Tooltip("Arraste o ScriptableObject da carta correspondente aqui.")]
    public CartaData cartaData;

    [Header("Estilo do Tooltip")]
    [SerializeField] private Color corFundo = new Color(0.1f, 0.1f, 0.15f, 0.98f);
    [SerializeField] private Color corBorda = new Color(0.8f, 0.8f, 0.9f, 1f);
    [SerializeField] private Color corTexto = Color.white;
    [SerializeField] private Color corDesconto = new Color(0.6f, 1f, 0.6f, 1f); // Verde claro
    [SerializeField] private int tamanhoFonteTitulo = 24;
    [SerializeField] private int tamanhoFonteDesc = 20;
    [SerializeField] private int tamanhoFontePreco = 22;

    // Referências do Tooltip criado dinamicamente
    private static GameObject tooltipInstance;
    private static Camera mainCamera;

    private void Start()
    {
        // Garante que a câmera principal seja pega no início
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void OnMouseEnter()
    {
        // Se não houver dados da carta ou o jogo estiver pausado, não faz nada.
        if (cartaData == null || Time.timeScale == 0f) return;

        // Mostra o tooltip com as informações da carta.
        ShowTooltip();
    }

    private void OnMouseExit()
    {
        // Destrói o tooltip ao sair.
        HideTooltip();
    }

    private void OnDisable()
    {
        // Garante que o tooltip suma se a carta for desativada.
        HideTooltip();
    }

    private void Update()
    {
        // Move o tooltip para seguir o mouse.
        if (tooltipInstance != null)
        {
            // Posição na tela do objeto da carta
            Vector3 cardScreenPosition = mainCamera.WorldToScreenPoint(transform.position);

            // Tenta posicionar à direita da carta. Se sair da tela, posiciona à esquerda.
            float tooltipWidth = tooltipInstance.GetComponent<RectTransform>().rect.width;
            float rightEdge = cardScreenPosition.x + tooltipWidth + 30; // 30 é um offset

            if (rightEdge > Screen.width)
            {
                // Posiciona à esquerda
                tooltipInstance.transform.position = cardScreenPosition + new Vector3(-30, 0, 0);
                tooltipInstance.GetComponent<RectTransform>().pivot = new Vector2(1, 0.5f); // Pivô na direita
            }
            else
            {
                // Posiciona à direita
                tooltipInstance.transform.position = cardScreenPosition + new Vector3(30, 0, 0);
                tooltipInstance.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f); // Pivô na esquerda
            }
        }
    }

    private void ShowTooltip()
    {
        // Formata o texto principal com a descrição da carta.
        string textoDescricao = cartaData.descricao;

        // Lógica de Preço
        string textoPreco;
        bool descontoAtivo = CaixaDeBaralho.IsActive;
        int precoComDesconto = CaixaDeBaralho.CalcularPrecoComDesconto(cartaData.preco);

        if (descontoAtivo && precoComDesconto < cartaData.preco)
        {
            textoPreco = $"<s>{cartaData.preco}G</s> <color=#{ColorUtility.ToHtmlStringRGB(corDesconto)}>{precoComDesconto}G</color>";
        }
        else
        {
            textoPreco = $"{cartaData.preco}G";
        }

        CreateTooltipObject(cartaData.nomeCarta, textoDescricao, textoPreco);
    }

    private void CreateTooltipObject(string titulo, string conteudo, string preco)
    {
        HideTooltip(); // Garante que qualquer tooltip antigo seja destruído.

        // Encontra o Canvas principal da cena ou cria um novo se não existir.
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            var canvasObj = new GameObject("AutoTooltipCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999; // Ordem bem alta para ficar na frente de tudo.
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // --- Cria a estrutura do Tooltip ---
        tooltipInstance = new GameObject("ShopTooltipInstance");
        tooltipInstance.transform.SetParent(canvas.transform, false);

        var rt = tooltipInstance.AddComponent<RectTransform>();
        rt.pivot = new Vector2(0, 0.5f); // Pivô inicial na esquerda-centro

        // Fundo com cor e borda
        var fundoImg = tooltipInstance.AddComponent<Image>();
        fundoImg.color = corFundo;
        var borda = tooltipInstance.AddComponent<Outline>();
        borda.effectColor = corBorda;
        borda.effectDistance = new Vector2(2, -2);

        // Layout para organizar o conteúdo
        var layout = tooltipInstance.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(15, 15, 15, 15);
        layout.spacing = 10;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        var fitter = tooltipInstance.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // --- Adiciona os textos ---
        CreateTextElement(tooltipInstance.transform, titulo, tamanhoFonteTitulo, FontStyles.Bold, TextAlignmentOptions.Left, 350);
        if (!string.IsNullOrEmpty(conteudo))
        {
            CreateTextElement(tooltipInstance.transform, conteudo, tamanhoFonteDesc, FontStyles.Normal, TextAlignmentOptions.Left, 350);
        }
        CreateTextElement(tooltipInstance.transform, preco, tamanhoFontePreco, FontStyles.Bold, TextAlignmentOptions.Right, 350);
    }

    private TextMeshProUGUI CreateTextElement(Transform parent, string text, int fontSize, FontStyles style, TextAlignmentOptions alignment, float preferredWidth)
    {
        var textObj = new GameObject("TooltipText_" + text.Substring(0, Mathf.Min(5, text.Length)));
        textObj.transform.SetParent(parent, false);

        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = corTexto;
        tmp.fontStyle = style;
        tmp.alignment = alignment;
        tmp.enableWordWrapping = true;
        tmp.richText = true;

        var layoutElement = textObj.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = preferredWidth;

        return tmp;
    }

    private static void HideTooltip()
    {
        if (tooltipInstance != null)
        {
            Destroy(tooltipInstance);
            tooltipInstance = null;
        }
    }
}