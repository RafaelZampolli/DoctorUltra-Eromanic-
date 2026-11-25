using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CameraFloatEffect))] // Garante que o outro script esteja presente
public class ControladorCameraInterludio : MonoBehaviour
{
    [Header("Configurações do Zoom")]
    public Transform pontoFocoMonitor;
    public float duracaoAnimacao = 1.5f;

    // Referência para o outro script de câmera
    private CameraFloatEffect cameraFloatEffect;

    private Camera mainCamera;
    private Vector3 posicaoOriginalCamera;
    private Quaternion rotacaoOriginalCamera;
    private bool estaComZoom = false;

    [Header("Posição da Câmera no Interlúdio")]
    public Transform alvoCameraInterludio; // Crie um Empty GameObject na cena onde a câmera deve ir e arraste aqui.
    public float velocidadeMovimento = 2.0f;

    void Awake()
    {
        // Pega a referência do script na inicialização
        cameraFloatEffect = GetComponent<CameraFloatEffect>();
    }

    void Start()
    {
        mainCamera = Camera.main;
    }

    public void MoverParaInterludio()
    {
        if (alvoCameraInterludio != null)
        {
            // Usando DOTween para um movimento suave (se você tiver o plugin)
            // Camera.main.transform.DOMove(alvoCameraInterludio.position, velocidadeMovimento);
            // Camera.main.transform.DORotate(alvoCameraInterludio.rotation.eulerAngles, velocidadeMovimento);

            // Alternativa sem DOTween (movimento instantâneo)
            Camera.main.transform.position = alvoCameraInterludio.position;
            Camera.main.transform.rotation = alvoCameraInterludio.rotation;

            Debug.Log("Câmera movida para a posição do interlúdio.");
        }
        else
        {
            Debug.LogWarning("Alvo da câmera do interlúdio não foi definido no Inspector!");
        }
    }


    public void MoverParaMonitor()
    {
        if (estaComZoom || pontoFocoMonitor == null) return;

        // 1. Desativa o script de flutuação para não haver conflito
        if (cameraFloatEffect != null)
        {
            cameraFloatEffect.enabled = false;
        }

        // Salva a posição original ANTES de desativar o float effect
        posicaoOriginalCamera = mainCamera.transform.position;
        rotacaoOriginalCamera = mainCamera.transform.rotation;

        estaComZoom = true;

        // 2. Anima a câmera para o ponto de foco
        mainCamera.transform.DOMove(pontoFocoMonitor.position, duracaoAnimacao)
            .SetEase(Ease.OutCubic);

        mainCamera.transform.DORotateQuaternion(pontoFocoMonitor.rotation, duracaoAnimacao)
            .SetEase(Ease.OutCubic)
            .OnComplete(() => {
                // 3. Quando a animação termina:
                if (cameraFloatEffect != null)
                {
                    // Define a nova posição como base para a flutuação
                    cameraFloatEffect.DefinirNovaPosicaoOriginal();
                    // Reativa o script de flutuação
                    cameraFloatEffect.enabled = true;
                }
            });
    }

    public void VoltarParaPosicaoOriginal()
    {
        if (!estaComZoom) return;

        // 1. Desativa o script de flutuação
        if (cameraFloatEffect != null)
        {
            cameraFloatEffect.enabled = false;
        }

        estaComZoom = false;

        // 2. Anima a câmera de volta para a posição salva
        mainCamera.transform.DOMove(posicaoOriginalCamera, duracaoAnimacao)
            .SetEase(Ease.OutCubic);

        mainCamera.transform.DORotateQuaternion(rotacaoOriginalCamera, duracaoAnimacao)
            .SetEase(Ease.OutCubic)
            .OnComplete(() => {
                // 3. Quando a animação termina:
                if (cameraFloatEffect != null)
                {
                    // Define a posição original como a base para a flutuação
                    cameraFloatEffect.DefinirNovaPosicaoOriginal();
                    // Reativa o script de flutuação
                    cameraFloatEffect.enabled = true;
                }
            });
    }
}