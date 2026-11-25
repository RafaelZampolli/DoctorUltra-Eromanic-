using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class AplicadorDeSintomasVisuais : MonoBehaviour
{
    [Header("Configuração de Sprites")]
    public SpriteRenderer corpoRenderer;
    public Sprite spriteOriginal;
    public List<MapeamentoSintoma> mapeamentoSintomas;

    [Header("Chances de Aparição de Sintoma (%)")]
    [Range(0, 100)] public float chanceMagreza = 50f;
    [Range(0, 100)] public float chanceManchas = 50f;
    [Range(0, 100)] public float chanceQuedaCabelo = 50f;
    [Range(0, 100)] public float chanceSudorese = 50f;
    [Range(0, 100)] public float chanceCansaco = 50f;
    [Range(0, 100)] public float chanceDorPeito = 50f;

    [Header("Minigame de Ação")]
    public KeyCode teclaDoMinigame = KeyCode.A;
    public float decaimentoPorSegundo = 0.3f;

    [Header("Preenchimento Dinâmico")]
    public float preenchimentoNormal = 0.08f;
    public float preenchimentoCritico = 0.15f;
    [Range(0, 1)] public float limiteInferiorCritico = 0.25f;
    public float preenchimentoDePrecisao = 0.04f;
    [Range(0, 1)] public float limiteSuperiorPrecisao = 0.8f;

    [Header("Recompensas e Penalidades")]
    public int pontosPorVencer = 150;
    public int culpaPorPerder = 1;

    [Header("Visual da Barra")]
    public Vector2 tamanhoDaBarra = new Vector2(250, 25);
    public Color corDaBarra = new Color(0.8f, 0.1f, 0.1f, 0.7f);
    public Color corDoPreenchimento = Color.green;

    [Header("Teleporte Progressivo")]
    public bool teleporteAtivo = true;
    public float distanciaTeleporteInicial = 50f;
    public float distanciaTeleporteFinal = 500f;
    public int cliquesParaDistanciaMaxima = 5;

    // --- NOVAS VARIÁVEIS PÚBLICAS ---
    [HideInInspector] public List<TipoSintomaVisual> SintomasVisuaisPossiveis { get; private set; } = new List<TipoSintomaVisual>();
    [HideInInspector] public TipoSintomaVisual? SintomaVisualAplicado { get; private set; } = null;
    // ---------------------------------

    private bool minigameAtivo = false;
    private Slider qteSlider;
    private GameObject qteTooltipInstance;
    private ManagerTurno managerTurno;
    private RectTransform canvasRect;
    private int cliquesNoMinigame = 0;

    [HideInInspector] public bool facilitarProximoQTE = false;
    private bool efeitoFacilitadorAtivo = false;
    private float preenchimentoNormalOriginal;
    private float decaimentoPorSegundoOriginal;

    [System.Serializable]
    public class MapeamentoSintoma
    {
        public TipoSintomaVisual tipo;
        public Sprite spriteDoSintoma;
    }

    void Start()
    {
        preenchimentoNormalOriginal = preenchimentoNormal;
        decaimentoPorSegundoOriginal = decaimentoPorSegundo;

        if (corpoRenderer == null) corpoRenderer = GetComponent<SpriteRenderer>();
        if (corpoRenderer != null) spriteOriginal = corpoRenderer.sprite;

        if (ManagerPacientes.Instance == null) return;
        Doenca doencaAtual = ManagerPacientes.Instance.doencaAtual;
        if (doencaAtual == null) return;

        // Preenche a lista de sintomas que a doença PODE causar
        if (doencaAtual.magrezaExtrema) SintomasVisuaisPossiveis.Add(TipoSintomaVisual.MagrezaExtrema);
        if (doencaAtual.manchasNaPele) SintomasVisuaisPossiveis.Add(TipoSintomaVisual.ManchasNaPele);
        if (doencaAtual.quedaDeCabelo) SintomasVisuaisPossiveis.Add(TipoSintomaVisual.QuedaDeCabelo);
        if (doencaAtual.sudorese) SintomasVisuaisPossiveis.Add(TipoSintomaVisual.Sudorese);
        if (doencaAtual.cansaco) SintomasVisuaisPossiveis.Add(TipoSintomaVisual.Cansaco);
        if (doencaAtual.dorNoPeito) SintomasVisuaisPossiveis.Add(TipoSintomaVisual.DorNoPeito);

        if (doencaAtual.eAcao)
        {
            if (doencaAtual.fraturaExposta)
            {
                AplicarSpriteObrigatorio(TipoSintomaVisual.FraturaExposta);
                SintomaVisualAplicado = TipoSintomaVisual.FraturaExposta; // Registra o sintoma
            }
            IniciarMinigame();
            return;
        }

        // Tenta aplicar um sintoma visual baseado na chance
        var candidatos = new Dictionary<TipoSintomaVisual, float>
        {
            { TipoSintomaVisual.MagrezaExtrema, chanceMagreza },
            { TipoSintomaVisual.ManchasNaPele, chanceManchas },
            { TipoSintomaVisual.QuedaDeCabelo, chanceQuedaCabelo },
            { TipoSintomaVisual.Sudorese, chanceSudorese },
            { TipoSintomaVisual.Cansaco, chanceCansaco },
            { TipoSintomaVisual.DorNoPeito, chanceDorPeito }
        };

        var aplicaveis = SintomasVisuaisPossiveis
            .Where(s => Random.Range(0f, 100f) < candidatos[s])
            .ToList();

        if (aplicaveis.Count > 0)
        {
            // Sorteia um dos sintomas que passaram no teste de chance
            int indiceSorteado = Random.Range(0, aplicaveis.Count);
            TipoSintomaVisual sintomaEscolhido = aplicaveis[indiceSorteado];

            Sprite spriteSintoma = GetSpriteParaSintoma(sintomaEscolhido);
            if (spriteSintoma != null)
            {
                corpoRenderer.sprite = spriteSintoma;
                SintomaVisualAplicado = sintomaEscolhido; // Registra qual sintoma foi aplicado
            }
        }
    }

    // ... O resto do script permanece igual ...
    void Update()
    {
        if (!minigameAtivo) return;

        if (!teleporteAtivo && qteTooltipInstance != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1.5f);
            qteTooltipInstance.transform.position = screenPos;
        }

        qteSlider.value -= decaimentoPorSegundo * Time.deltaTime;

        if (Input.GetKeyDown(teclaDoMinigame))
        {
            cliquesNoMinigame++;

            // ===== LÓGICA DO PREENCHIMENTO DINÂMICO =====
            float preenchimentoAtual;
            float valorAtualBarra = qteSlider.value;

            if (valorAtualBarra <= limiteInferiorCritico)
            {
                // Zona Crítica (quase perdendo) -> Ajuda
                preenchimentoAtual = preenchimentoCritico;
            }
            else if (valorAtualBarra >= limiteSuperiorPrecisao)
            {
                // Zona de Precisão (quase ganhando) -> Dificulta
                preenchimentoAtual = preenchimentoDePrecisao;
            }
            else
            {
                // Zona Normal (meio da barra)
                preenchimentoAtual = preenchimentoNormal;
            }

            qteSlider.value += preenchimentoAtual;
            // ============================================

            if (teleporteAtivo)
            {
                TeleportarBarra();
            }
        }

        if (qteSlider.value >= 1f) VencerMinigame();
        else if (qteSlider.value <= 0f) PerderMinigame();
    }

    // ... (O resto do script, como IniciarMinigame, TeleportarBarra, CriarQTE_UI, etc., permanece igual à versão anterior) ...

    private void IniciarMinigame()
    {
        managerTurno = FindObjectOfType<ManagerTurno>();
        if (managerTurno == null) { Debug.LogError("ManagerTurno não encontrado!"); return; }

        cliquesNoMinigame = 0;

        // ATIVA O EFEITO SÓ SE A FLAG FOI MARCADA
        if (facilitarProximoQTE)
        {
            preenchimentoNormal *= 1.5f;
            decaimentoPorSegundo *= 0.7f;
            efeitoFacilitadorAtivo = true;
            facilitarProximoQTE = false;
        }

        CriarQTE_UI_DoZero();
        qteSlider.value = 0.5f;
        minigameAtivo = true;

        if (teleporteAtivo)
        {
            qteTooltipInstance.transform.position = new Vector2(Screen.width / 2, Screen.height / 2);
        }
    }

    private void TeleportarBarra()
    {
        if (qteTooltipInstance == null || canvasRect == null) return;

        float progresso = Mathf.Clamp01((float)cliquesNoMinigame / cliquesParaDistanciaMaxima);
        float distanciaAtual = Mathf.Lerp(distanciaTeleporteInicial, distanciaTeleporteFinal, progresso);
        Vector2 posAtual = qteTooltipInstance.transform.position;
        float angulo = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector2 novoPonto = posAtual + new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo)) * distanciaAtual;

        float paddingX = tamanhoDaBarra.x / 2;
        float paddingY = tamanhoDaBarra.y / 2 + 30;
        novoPonto.x = Mathf.Clamp(novoPonto.x, paddingX, Screen.width - paddingX);
        novoPonto.y = Mathf.Clamp(novoPonto.y, paddingY, Screen.height - paddingY);

        qteTooltipInstance.transform.position = novoPonto;
    }

    private void FinalizarMinigame()
    {
        minigameAtivo = false;
        if (qteTooltipInstance != null) Destroy(qteTooltipInstance);

        // RESTAURA VALORES SE O EFEITO FOI ATIVADO
        if (efeitoFacilitadorAtivo)
        {
            preenchimentoNormal = preenchimentoNormalOriginal;
            decaimentoPorSegundo = decaimentoPorSegundoOriginal;
            efeitoFacilitadorAtivo = false;
        }
    }

    private void CriarQTE_UI_DoZero()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) { Debug.LogError("Nenhum Canvas encontrado!"); return; }
        canvasRect = canvas.GetComponent<RectTransform>();

        qteTooltipInstance = new GameObject("QTE_Tooltip_Gerado", typeof(RectTransform));
        qteTooltipInstance.transform.SetParent(canvas.transform, false);

        GameObject sliderGO = new GameObject("BarraDeProgresso", typeof(RectTransform));
        sliderGO.transform.SetParent(qteTooltipInstance.transform, false);
        qteSlider = sliderGO.AddComponent<Slider>();
        RectTransform sliderRect = sliderGO.GetComponent<RectTransform>();
        sliderRect.sizeDelta = tamanhoDaBarra;

        GameObject backgroundGO = new GameObject("Background", typeof(Image));
        backgroundGO.transform.SetParent(sliderRect, false);
        Image backgroundImg = backgroundGO.GetComponent<Image>();
        backgroundImg.color = corDaBarra;
        RectTransform backgroundRect = backgroundGO.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.sizeDelta = Vector2.zero;

        GameObject fillAreaGO = new GameObject("Fill Area", typeof(RectTransform));
        fillAreaGO.transform.SetParent(sliderRect, false);
        RectTransform fillAreaRect = fillAreaGO.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;
        fillAreaRect.offsetMin = fillAreaRect.offsetMax = Vector2.zero;

        GameObject fillGO = new GameObject("Fill", typeof(Image));
        fillGO.transform.SetParent(fillAreaRect, false);
        Image fillImg = fillGO.GetComponent<Image>();
        fillImg.color = corDoPreenchimento;
        RectTransform fillRect = fillGO.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        fillRect.offsetMin = fillRect.offsetMax = Vector2.zero;

        qteSlider.fillRect = fillRect;
        qteSlider.direction = Slider.Direction.LeftToRight;
        qteSlider.minValue = 0;
        qteSlider.maxValue = 1;

        GameObject textGO = new GameObject("TextoTecla", typeof(Text));
        textGO.transform.SetParent(qteTooltipInstance.transform, false);
        Text textComponent = textGO.GetComponent<Text>();
        textComponent.text = $"[{teclaDoMinigame}]";
        textComponent.font = Font.CreateDynamicFontFromOSFont("Arial", 24);
        textComponent.fontSize = 20;
        textComponent.alignment = TextAnchor.MiddleCenter;
        textComponent.color = Color.white;
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchoredPosition = new Vector2(0, 30);
        textRect.sizeDelta = new Vector2(50, 30);
    }

    private void VencerMinigame()
    {
        FinalizarMinigame();
        Debug.Log($"<color=lime>MINIGAME VENCIDO! Pontos: +{pontosPorVencer}</color>");
        if (managerTurno != null && managerTurno.cronometro != null)
        {
            managerTurno.cronometro.AdicionarPontos(pontosPorVencer);
        }
        managerTurno.FimDoPaciente();
    }

    private void PerderMinigame()
    {
        FinalizarMinigame();
        Debug.Log($"<color=orange>MINIGAME PERDIDO! Culpa: +{culpaPorPerder}</color>");
        if (managerTurno != null && managerTurno.cronometro != null)
        {
            for (int i = 0; i < culpaPorPerder; i++)
            {
                managerTurno.cronometro.AdicionarCulpa(1);
            }
            managerTurno.cronometro.SinalizarErroDeDiagnostico();
        }
        managerTurno.FimDoPaciente();
    }

    public void AplicarSpriteObrigatorio(TipoSintomaVisual tipo)
    {
        Sprite spriteAcao = GetSpriteParaSintoma(tipo);
        if (spriteAcao != null && corpoRenderer != null) corpoRenderer.sprite = spriteAcao;
    }

    private Sprite GetSpriteParaSintoma(TipoSintomaVisual tipo)
    {
        foreach (var item in mapeamentoSintomas)
        {
            if (item.tipo == tipo) return item.spriteDoSintoma;
        }
        return null;
    }
}