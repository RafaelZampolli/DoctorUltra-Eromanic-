using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SpriteChangerPorCulpa : MonoBehaviour
{
    [Header("Referências (Opcional)")]
    [Tooltip("Script 'ManagerPontos'. Se deixado em branco, será procurado na cena.")]
    public ManagerPontos managerPontos;

    [Header("Configuração da Culpa")]
    [Tooltip("O sprite será trocado quando os pontos de culpa forem >= a este valor.")]
    public int limiteDeCulpa = 3;

    [Header("Sprites")]
    [Tooltip("O novo sprite a ser exibido quando o limite de culpa for atingido.")]
    public Sprite spriteNovo;

    // --- EFEITOS LOCAIS (NO OBJETO) ---
    [Header("Efeitos Visuais Locais (no próprio objeto)")]
    public bool usarEfeitoFlashLocal = true;
    public bool usarEfeitoShakeLocal = true;
    public Color corDoFlashLocal = Color.yellow;
    public float duracaoShakeLocal = 0.5f;
    public float forcaShakeLocal = 10f;

    // --- EFEITOS GLOBAIS (TELA E CÂMERA) ---
    [Header("Efeitos Visuais Globais (Tela e Câmera)")]
    public bool usarEfeitosGlobais = true;
    [Tooltip("Painel de UI que pisca. Se deixado em branco, será criado automaticamente.")]
    private Image painelFlashGlobal; // Tornamos privado, pois será autoconfigurado
    [Tooltip("Volume de Post-processing. Se deixado em branco, será procurado na cena.")]
    private Volume postProcessVolume; // Privado também

    [Header("Configuração do Flash de Tela")]
    public Color corDoFlashGlobal = Color.red;
    [Range(0, 1)] public float opacidadeMaximaFlash = 0.6f;
    public float duracaoFadeInFlash = 0.1f;
    public float duracaoFadeOutFlash = 0.5f;

    [Header("Configuração do Tremor da Câmera")]
    public float duracaoShakeCamera = 0.4f;
    public float forcaShakeCamera = 0.5f;

    // --- Variáveis Internas ---
    private SpriteRenderer meuSpriteRenderer;
    private Image minhaImagemUI;
    private Sprite spriteOriginal;
    private Color corOriginal;
    private bool spriteTrocado = false;
    private ChromaticAberration chromaticAberration;

    void Awake()
    {
        // Configuração dos componentes locais
        meuSpriteRenderer = GetComponent<SpriteRenderer>();
        minhaImagemUI = GetComponent<Image>();

        if (meuSpriteRenderer != null)
        {
            spriteOriginal = meuSpriteRenderer.sprite;
            corOriginal = meuSpriteRenderer.color;
        }
        else if (minhaImagemUI != null)
        {
            spriteOriginal = minhaImagemUI.sprite;
            corOriginal = minhaImagemUI.color;
        }
        else
        {
            Debug.LogError("Nenhum SpriteRenderer ou Image encontrado neste objeto!", this);
            return;
        }

        if (managerPontos == null) managerPontos = FindObjectOfType<ManagerPontos>();

        // --- LÓGICA DE AUTOCONFIGURAÇÃO DOS EFEITOS GLOBAIS ---
        SetupEfeitosGlobais();
    }

    void Update()
    {
        if (managerPontos == null) return;
        if (pontosDeCulpaAtuais >= limiteDeCulpa && !spriteTrocado) TrocarSpriteEAtivarEfeitos();
        else if (pontosDeCulpaAtuais < limiteDeCulpa && spriteTrocado) RestaurarSprite();
    }

    // Propriedade para simplificar o acesso à culpa
    private int pontosDeCulpaAtuais => managerPontos.Culpa;

    private void TrocarSpriteEAtivarEfeitos()
    {
        if (spriteNovo == null) return;
        spriteTrocado = true;

        // 1. Troca o sprite
        if (meuSpriteRenderer != null) meuSpriteRenderer.sprite = spriteNovo;
        else if (minhaImagemUI != null) minhaImagemUI.sprite = spriteNovo;

        // 2. Ativa os efeitos LOCAIS
        if (usarEfeitoFlashLocal) AtivarFlashLocal();
        if (usarEfeitoShakeLocal) transform.DOShakePosition(duracaoShakeLocal, strength: forcaShakeLocal);

        // 3. Ativa os efeitos GLOBAIS
        if (usarEfeitosGlobais) AcionarEfeitosGlobais();
    }

    private void AtivarFlashLocal()
    {
        int numeroDeFlashes = 2;
        float duracaoDoFlash = 0.5f;

        if (meuSpriteRenderer != null)
        {
            meuSpriteRenderer.DOColor(corDoFlashLocal, duracaoDoFlash / (numeroDeFlashes * 2))
                             .SetLoops(numeroDeFlashes * 2, LoopType.Yoyo)
                             .OnComplete(() => meuSpriteRenderer.color = corOriginal);
        }
        else if (minhaImagemUI != null)
        {
            minhaImagemUI.DOColor(corDoFlashLocal, duracaoDoFlash / (numeroDeFlashes * 2))
                         .SetLoops(numeroDeFlashes * 2, LoopType.Yoyo)
                         .OnComplete(() => minhaImagemUI.color = corOriginal);
        }
    }

    private void AcionarEfeitosGlobais()
    {
        // Tremor da Câmera
        Camera.main?.DOShakePosition(duracaoShakeCamera, strength: forcaShakeCamera);

        // Flash Vermelho na Tela
        if (painelFlashGlobal != null)
        {
            painelFlashGlobal.DOKill();
            Sequence flashSequence = DOTween.Sequence();
            flashSequence.Append(painelFlashGlobal.DOFade(opacidadeMaximaFlash, duracaoFadeInFlash));
            flashSequence.Append(painelFlashGlobal.DOFade(0, duracaoFadeOutFlash));
        }

        // Distorção Cromática
        if (chromaticAberration != null)
        {
            DOTween.Kill(chromaticAberration);
            DOVirtual.Float(0, 1, duracaoShakeCamera / 2, value => chromaticAberration.intensity.value = value)
                .SetLoops(2, LoopType.Yoyo);
        }
    }

    private void RestaurarSprite()
    {
        if (meuSpriteRenderer != null) meuSpriteRenderer.sprite = spriteOriginal;
        else if (minhaImagemUI != null) minhaImagemUI.sprite = spriteOriginal;
        spriteTrocado = false;
    }

    #region Lógica de Autoconfiguração Global
    private void SetupEfeitosGlobais()
    {
        // --- Configura o Painel de Flash ---
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("Nenhum Canvas encontrado para o efeito de flash global.", this);
            return;
        }

        // Tenta encontrar um painel existente primeiro
        Transform existingPanel = canvas.transform.Find("PainelFlashGlobal (Gerado)");
        if (existingPanel != null)
        {
            painelFlashGlobal = existingPanel.GetComponent<Image>();
        }
        else // Se não encontrar, cria um novo
        {
            GameObject painelObj = new GameObject("PainelFlashGlobal (Gerado)");
            painelObj.transform.SetParent(canvas.transform, false);
            painelFlashGlobal = painelObj.AddComponent<Image>();
            painelFlashGlobal.color = new Color(corDoFlashGlobal.r, corDoFlashGlobal.g, corDoFlashGlobal.b, 0);
            painelFlashGlobal.raycastTarget = false;
            RectTransform rect = painelObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
        }

        // --- Configura o Post-Processing ---
        postProcessVolume = FindObjectOfType<Volume>();
        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGet(out chromaticAberration);
        }
    }
    #endregion
}