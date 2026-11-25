using UnityEngine;
using System.Collections;

public class CameraFloatEffect : MonoBehaviour
{
    [Header("Configurações de Flutuação")]
    [Range(0.01f, 1f)]
    public float intensidadeFlutuacao = 0.1f;

    [Range(0.1f, 3f)]
    public float velocidadeFlutuacao = 1f;

    [Header("Movimento Horizontal")]
    [Range(0f, 1f)]
    public float intensidadeHorizontal = 0.5f;

    [Range(0.1f, 3f)]
    public float velocidadeHorizontal = 0.7f;

    [Header("Rotação Suave")]
    [Range(0f, 5f)]
    public float intensidadeRotacao = 1f;

    [Range(0.1f, 3f)]
    public float velocidadeRotacao = 0.5f;

    [Header("Configurações de Zoom")]
    [Range(0f, 2f)]
    public float intensidadeZoom = 0.3f;

    [Range(0.1f, 3f)]
    public float velocidadeZoom = 0.4f;

    [Header("Opções")]
    public bool ativarFlutuacao = true;
    public bool ativarRotacao = true;
    public bool ativarZoom = false;

    [Header("Shake on Events")]
    [Range(0.1f, 2f)]
    public float intensidadeShake = 0.5f;

    [Range(0.1f, 1f)]
    public float duracaoShake = 0.3f;

    [Header("Smooth Follow (Opcional)")]
    public Transform alvoSeguir;

    [Range(1f, 10f)]
    public float suavidadeSeguir = 5f;

    // Variáveis privadas
    private Vector3 posicaoOriginal;
    private Quaternion rotacaoOriginal;
    private float fovOriginal;
    private Camera cam;

    private float tempoFlutuacao;
    private float tempoHorizontal;
    private float tempoRotacao;
    private float tempoZoom;

    private Coroutine shakeCoroutine;
    private bool estaShaking = false;

    void Start()
    {
        // Salva valores originais
        posicaoOriginal = transform.position;
        rotacaoOriginal = transform.rotation;

        // Pega referência da câmera
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            fovOriginal = cam.fieldOfView;
        }

        // Inicia com valores aleatórios para parecer mais natural
        tempoFlutuacao = Random.Range(0f, Mathf.PI * 2);
        tempoHorizontal = Random.Range(0f, Mathf.PI * 2);
        tempoRotacao = Random.Range(0f, Mathf.PI * 2);
        tempoZoom = Random.Range(0f, Mathf.PI * 2);
    }

    void LateUpdate()
    {
        if (!estaShaking)
        {
            AplicarFlutuacao();
        }

        // Smooth follow opcional
        if (alvoSeguir != null)
        {
            Vector3 posicaoAlvo = alvoSeguir.position + (posicaoOriginal - transform.position);
            transform.position = Vector3.Lerp(transform.position, posicaoAlvo, Time.deltaTime * suavidadeSeguir);
        }
    }

    void AplicarFlutuacao()
    {
        Vector3 novaPos = posicaoOriginal;
        Quaternion novaRot = rotacaoOriginal;

        // Flutuação vertical
        if (ativarFlutuacao)
        {
            tempoFlutuacao += Time.deltaTime * velocidadeFlutuacao;
            float offsetY = Mathf.Sin(tempoFlutuacao) * intensidadeFlutuacao;
            novaPos.y += offsetY;

            // Movimento horizontal suave
            tempoHorizontal += Time.deltaTime * velocidadeHorizontal;
            float offsetX = Mathf.Sin(tempoHorizontal) * intensidadeHorizontal * intensidadeFlutuacao;
            novaPos.x += offsetX;
        }

        // Rotação suave
        if (ativarRotacao)
        {
            tempoRotacao += Time.deltaTime * velocidadeRotacao;

            float rotX = Mathf.Sin(tempoRotacao * 1.1f) * intensidadeRotacao * 0.3f;
            float rotY = Mathf.Sin(tempoRotacao * 0.9f) * intensidadeRotacao * 0.5f;
            float rotZ = Mathf.Sin(tempoRotacao) * intensidadeRotacao;

            novaRot = rotacaoOriginal * Quaternion.Euler(rotX, rotY, rotZ);
        }

        // Zoom suave (FOV breathing)
        if (ativarZoom && cam != null)
        {
            tempoZoom += Time.deltaTime * velocidadeZoom;
            float zoomOffset = Mathf.Sin(tempoZoom) * intensidadeZoom;
            cam.fieldOfView = fovOriginal + zoomOffset;
        }

        // Aplica as transformações
        transform.position = novaPos;
        transform.rotation = novaRot;
    }

    // ========== MÉTODOS PÚBLICOS ==========

    /// <summary>
    /// Aplica um shake na câmera (útil para impactos, cliques importantes, etc)
    /// </summary>
    public void Shake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        shakeCoroutine = StartCoroutine(ShakeCoroutine());
    }

    /// <summary>
    /// Shake customizado com parâmetros específicos
    /// </summary>
    public void ShakeCustom(float intensidade, float duracao)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        shakeCoroutine = StartCoroutine(ShakeCoroutine(intensidade, duracao));
    }

    IEnumerator ShakeCoroutine(float? intensidadeCustom = null, float? duracaoCustom = null)
    {
        estaShaking = true;

        float intensidade = intensidadeCustom ?? intensidadeShake;
        float duracao = duracaoCustom ?? duracaoShake;
        float tempo = 0;

        Vector3 posInicial = transform.position;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float progresso = tempo / duracao;

            // Diminui intensidade ao longo do tempo
            float shakeAtual = intensidade * (1f - progresso);

            // Shake mais caótico no início, mais suave no fim
            float shakeX = (Mathf.PerlinNoise(tempo * 50, 0) - 0.5f) * 2 * shakeAtual;
            float shakeY = (Mathf.PerlinNoise(0, tempo * 50) - 0.5f) * 2 * shakeAtual;

            transform.position = posInicial + new Vector3(shakeX, shakeY, 0);

            yield return null;
        }

        transform.position = posInicial;
        estaShaking = false;
    }

    /// <summary>
    /// Pulso suave na câmera (zoom in/out rápido)
    /// </summary>
    public void Pulse(float intensidade = 2f, float duracao = 0.2f)
    {
        if (cam != null)
        {
            StartCoroutine(PulseCoroutine(intensidade, duracao));
        }
    }

    IEnumerator PulseCoroutine(float intensidade, float duracao)
    {
        if (cam == null) yield break;

        float fovInicial = cam.fieldOfView;
        float fovMaximo = fovInicial - intensidade;
        float tempo = 0;

        // Zoom in
        while (tempo < duracao * 0.3f)
        {
            tempo += Time.deltaTime;
            float progresso = tempo / (duracao * 0.3f);
            cam.fieldOfView = Mathf.Lerp(fovInicial, fovMaximo, Mathf.SmoothStep(0, 1, progresso));
            yield return null;
        }

        // Zoom out
        tempo = 0;
        while (tempo < duracao * 0.7f)
        {
            tempo += Time.deltaTime;
            float progresso = tempo / (duracao * 0.7f);
            cam.fieldOfView = Mathf.Lerp(fovMaximo, fovInicial, Mathf.SmoothStep(0, 1, progresso));
            yield return null;
        }

        cam.fieldOfView = fovInicial;
    }

    /// <summary>
    /// Reseta a câmera para posição original
    /// </summary>
    public void ResetarCamera(bool suave = true, float duracao = 1f)
    {
        if (suave)
        {
            StartCoroutine(ResetSuave(duracao));
        }
        else
        {
            transform.position = posicaoOriginal;
            transform.rotation = rotacaoOriginal;
            if (cam != null) cam.fieldOfView = fovOriginal;
        }
    }

    IEnumerator ResetSuave(float duracao)
    {
        float tempo = 0;
        Vector3 posInicial = transform.position;
        Quaternion rotInicial = transform.rotation;
        float fovInicial = cam != null ? cam.fieldOfView : 60;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float progresso = tempo / duracao;
            float t = Mathf.SmoothStep(0, 1, progresso);

            transform.position = Vector3.Lerp(posInicial, posicaoOriginal, t);
            transform.rotation = Quaternion.Slerp(rotInicial, rotacaoOriginal, t);

            if (cam != null)
            {
                cam.fieldOfView = Mathf.Lerp(fovInicial, fovOriginal, t);
            }

            yield return null;
        }

        transform.position = posicaoOriginal;
        transform.rotation = rotacaoOriginal;
        if (cam != null) cam.fieldOfView = fovOriginal;
    }

    /// <summary>
    /// Alterna o efeito de flutuação
    /// </summary>
    public void AlternarFlutuacao()
    {
        ativarFlutuacao = !ativarFlutuacao;
    }

    /// <summary>
    /// Define nova posição original (útil após mover a câmera)
    /// </summary>
    public void DefinirNovaPosicaoOriginal()
    {
        posicaoOriginal = transform.position;
        rotacaoOriginal = transform.rotation;
        if (cam != null)
        {
            fovOriginal = cam.fieldOfView;
        }
    }

    void OnDestroy()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
    }
}