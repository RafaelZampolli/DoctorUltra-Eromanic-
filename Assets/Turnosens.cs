using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

public class TransicaoTurno : MonoBehaviour
{
    public static TransicaoTurno Instancia { get; private set; }

    [Header("Configurações da Animação")]
    public float duracaoFadeIn = 0.7f;
    public float duracaoEspera = 1.5f;
    public float duracaoFadeOut = 0.7f;

    [Header("Customização Visual")]
    [Tooltip("Arraste aqui o seu arquivo de fonte TextMeshPro (SDF) para customizar o texto.")]
    public TMP_FontAsset fonteCustomizada; // NOVO: Campo para sua fonte!

    private Canvas canvasTransicao;
    private Image painelFade;
    private TextMeshProUGUI textoTurno;
    private bool estaOcupado = false;

    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
        DontDestroyOnLoad(gameObject);

        CriarElementosUI();
    }

    private void CriarElementosUI()
    {
        GameObject canvasGO = new GameObject("Canvas_Transicao_AUTOCRIADO");
        DontDestroyOnLoad(canvasGO); // Garante que o canvas também persista
        canvasTransicao = canvasGO.AddComponent<Canvas>();
        canvasTransicao.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasTransicao.sortingOrder = 100;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject painelGO = new GameObject("Painel_Fade_AUTOCRIADO");
        painelGO.transform.SetParent(canvasGO.transform, false);
        painelFade = painelGO.AddComponent<Image>();
        painelFade.color = new Color(0, 0, 0, 0);
        painelFade.rectTransform.anchorMin = Vector2.zero;
        painelFade.rectTransform.anchorMax = Vector2.one;
        painelFade.rectTransform.sizeDelta = Vector2.zero;

        GameObject textoGO = new GameObject("Texto_Turno_AUTOCRIADO");
        textoGO.transform.SetParent(canvasGO.transform, false);
        textoTurno = textoGO.AddComponent<TextMeshProUGUI>();

        // --- LÓGICA DA FONTE E TAMANHO CORRIGIDA ---
        // 1. Usa sua fonte se você arrastou uma, senão usa a padrão.
        if (fonteCustomizada != null)
        {
            textoTurno.font = fonteCustomizada;
        }
        else
        {
            // Fallback para uma fonte padrão segura
            textoTurno.font = Resources.GetBuiltinResource<TMP_FontAsset>("Arial.ttf");
        }

        // 2. Aumenta o tamanho da caixa de texto para evitar cortes.
        textoTurno.rectTransform.sizeDelta = new Vector2(1000, 300); // Tamanho grande para o texto caber

        textoTurno.fontSize = 120;
        textoTurno.alignment = TextAlignmentOptions.Center;
        textoTurno.color = new Color(1, 1, 1, 0);
        textoTurno.text = "Turno X";

        canvasGO.SetActive(false);
    }

    public void ExecutarTransicao(int numeroDoTurno, Action onComplete = null)
    {
        if (estaOcupado) return;
        estaOcupado = true;

        canvasTransicao.gameObject.SetActive(true);
        textoTurno.text = $"Turno {numeroDoTurno}";

        painelFade.color = new Color(0, 0, 0, 0);
        textoTurno.alpha = 0f;

        Sequence sequencia = DOTween.Sequence();
        sequencia.Append(painelFade.DOFade(1f, duracaoFadeIn));
        sequencia.Join(textoTurno.DOFade(1f, duracaoFadeIn));
        sequencia.AppendInterval(duracaoEspera);
        sequencia.Append(painelFade.DOFade(0f, duracaoFadeOut));
        sequencia.Join(textoTurno.DOFade(0f, duracaoFadeOut));
        sequencia.OnComplete(() => {
            canvasTransicao.gameObject.SetActive(false);
            onComplete?.Invoke();
            estaOcupado = false;
        });
    }
}