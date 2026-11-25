using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider2D))]
public class CartaHoverSuave : MonoBehaviour
{
    [Header("Configurações")]
    public float alturaHover = 1.5f; // Quanto sobe no hover
    public float velocidadeHover = 8f; // Velocidade da transição
    public float scaleHover = 1.2f;
    public int layersParaCima = 10;

    [Header("Flutuação Base")]
    public float amplitudeFloat = 0.03f;
    public float velocidadeFloat = 2f;
    public float amplitudeRotacao = 1.5f;

    [Header("Visual")]
    public Color corHover = new Color(1.2f, 1.1f, 1f, 1f);

    [Header("Audio")]
    public AudioClip somHoverEnter;
    public AudioClip somHoverExit;

    // Estados
    private Vector3 posicaoBase;
    private Vector3 escalaBase;
    private Color corBase;
    private bool hoverAtivo = false;
    private bool sistemaHabilitado = false;

    // Interpolação manual
    private float hoverProgress = 0f; // 0 = base, 1 = hover completo
    private float offsetTemporalFloat;

    // Componentes
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private TooltipCartaSistema tooltipSistema;
    private int sortingOrderOriginal;

    // --- ESCALA BASE FIXA conforme pedido ---
    public readonly Vector3 escalaBaseFixa = new Vector3(0.07f, 0.0725f, 0.1f);

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        if (spriteRenderer != null)
        {
            corBase = spriteRenderer.color;
            sortingOrderOriginal = spriteRenderer.sortingOrder;
        }

        offsetTemporalFloat = Random.Range(0f, Mathf.PI * 2f);

        // Sempre inicia com a escala base fixa
        escalaBase = escalaBaseFixa;
        transform.localScale = escalaBaseFixa;

        Debug.Log($"[{name}] CartaHover V2 inicializado!");
    }

    void Update()
    {
        if (!sistemaHabilitado) return;

        float tempo = Time.time + offsetTemporalFloat;
        float floatY = Mathf.Sin(tempo * velocidadeFloat) * amplitudeFloat;
        float floatX = Mathf.Sin(tempo * velocidadeFloat * 0.7f) * amplitudeFloat * 0.5f;
        float rotacaoZ = Mathf.Sin(tempo * velocidadeFloat * 0.8f) * amplitudeRotacao;

        float targetProgress = hoverAtivo ? 1f : 0f;
        hoverProgress = Mathf.Lerp(hoverProgress, targetProgress, Time.deltaTime * velocidadeHover);

        Vector3 offsetHover = Vector3.up * alturaHover * hoverProgress;
        Vector3 offsetFloat = new Vector3(floatX, floatY, 0f);
        transform.position = posicaoBase + offsetFloat + offsetHover;

        // ESCALA
        float escalaMultiplicador = Mathf.Lerp(1f, scaleHover, hoverProgress);
        transform.localScale = escalaBaseFixa * escalaMultiplicador;

        // ROTAÇÃO
        transform.rotation = Quaternion.Euler(0, 0, rotacaoZ);

        // COR
        if (spriteRenderer != null)
        {
            Color corAtual = Color.Lerp(corBase, corHover, hoverProgress);
            spriteRenderer.color = corAtual;
        }
    }

    private void OnMouseEnter()
    {
        if (!sistemaHabilitado || hoverAtivo) return;

        hoverAtivo = true;

        if (audioSource != null && somHoverEnter != null)
        {
            audioSource.clip = somHoverEnter;
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.Play();
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = sortingOrderOriginal + layersParaCima;
        }

        Debug.Log($"[{name}] HOVER ATIVADO - Altura: {alturaHover}");
    }

    private void OnMouseExit()
    {
        if (!sistemaHabilitado || !hoverAtivo) return;

        hoverAtivo = false;

        if (tooltipSistema != null)
        {
            tooltipSistema.EsconderTooltip();
        }

        if (audioSource != null && somHoverExit != null)
        {
            audioSource.clip = somHoverExit;
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.Play();
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = sortingOrderOriginal;
        }

        Debug.Log($"[{name}] HOVER DESATIVADO");
    }

    public void HabilitarSistema()
    {
        posicaoBase = transform.position;
        escalaBase = escalaBaseFixa;
        transform.localScale = escalaBaseFixa;
        sistemaHabilitado = true;

        Debug.Log($"[{name}] Sistema habilitado - Posição base: {posicaoBase}, Escala base: {escalaBaseFixa}");
    }

    public void DesabilitarSistema()
    {
        sistemaHabilitado = false;
        hoverAtivo = false;
        hoverProgress = 0f;

        transform.position = posicaoBase;
        transform.localScale = escalaBaseFixa;
        transform.rotation = Quaternion.identity;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = corBase;
            spriteRenderer.sortingOrder = sortingOrderOriginal;
        }
    }

    // === MÉTODOS DE COMPATIBILIDADE (pros scripts antigos) ===
    public void HabilitarHover()
    {
        HabilitarSistema();
    }

    public void DesabilitarHover()
    {
        DesabilitarSistema();
    }

    public void DefinirEscalaOriginal(Vector3 escala)
    {
        // Ignora o parâmetro e sempre define a escala base fixa
        escalaBase = escalaBaseFixa;
        transform.localScale = escalaBaseFixa;
        Debug.Log($"[{name}] Escala base definida (fixa): {escalaBaseFixa}");
    }

    public void DefinirEscalaBase(Vector3 escala)
    {
        // Ignora o parâmetro e sempre define a escala base fixa
        escalaBase = escalaBaseFixa;
        transform.localScale = escalaBaseFixa;
        Debug.Log($"[{name}] Escala base definida (fixa): {escalaBaseFixa}");
    }

    private void OnMouseDown()
    {
        if (!sistemaHabilitado) return;

        Debug.Log($"[{name}] CLICK detectado!");

        transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 6, 0.8f);
    }

    public void ExecutarSpawnJuice()
    {
        Debug.Log($"[{name}] SPAWN JUICE iniciado!");

        transform.localScale = Vector3.zero;
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(-180f, 180f));

        transform.DOScale(escalaBaseFixa, 0.6f)
            .SetEase(Ease.OutElastic, 1.4f, 0.8f);

        transform.DORotate(Vector3.zero, 0.5f)
            .SetEase(Ease.OutBack, 1.2f);

        transform.DOPunchScale(Vector3.one * 0.08f, 0.3f, 6, 0.6f)
            .SetDelay(0.4f);
    }

    public void ExecutarMoveJuice(Vector3 posicaoFinal, float duracao)
    {
        Debug.Log($"[{name}] MOVE JUICE - Indo para: {posicaoFinal}");

        sistemaHabilitado = false;

        transform.DOMove(posicaoFinal, duracao)
            .SetEase(Ease.OutQuart)
            .OnComplete(() => {
                posicaoBase = transform.position;
                sistemaHabilitado = true;
                Debug.Log($"[{name}] Move completo - Sistema reativado na posição: {posicaoBase}");
            });

        transform.DORotate(Vector3.forward * Random.Range(-15f, 15f), duracao * 0.7f)
            .SetEase(Ease.OutQuart)
            .OnComplete(() => {
                transform.DORotate(Vector3.zero, 0.2f).SetEase(Ease.OutQuart);
            });
    }

    public void EsconderCarta()
    {
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>(true))
            sr.enabled = false;
        foreach (var audio in GetComponentsInChildren<AudioSource>(true))
            audio.enabled = false;

        sistemaHabilitado = false;
    }

    public void MostrarCarta()
    {
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>(true))
            sr.enabled = true;
        foreach (var audio in GetComponentsInChildren<AudioSource>(true))
            audio.enabled = true;

        sistemaHabilitado = true;
    }
}