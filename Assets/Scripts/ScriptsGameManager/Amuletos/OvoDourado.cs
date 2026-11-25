using UnityEngine;
using DG.Tweening; // Para animações suaves

public class OvoDourado : MonoBehaviour
{
    [Header("Modelos 3D")]
    [SerializeField] private GameObject modeloInativo;
    [SerializeField] private GameObject modeloAtivo;

    [Header("Game Feel")]
    [SerializeField] private ParticleSystem particulasAtivacao;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip somAtivacao;
    [SerializeField] private float intensidadeVibração = 0.3f;
    [SerializeField] private float duracaoVibração = 0.2f;

    public bool ovoDourado = false;

    private void Start()
    {
        AtualizarModelo();
    }

    public void OnEnable()
    {
        ovoDourado = true;
    }

    public void UsarAmuleto()
    {
        ovoDourado = !ovoDourado;

        // Animação de transição com punch scale
        transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 10, 1)
                .SetEase(Ease.OutBack);

        // Efeitos visuais e sonoros
        if (particulasAtivacao != null)
            particulasAtivacao.Play();

        if (audioSource != null && somAtivacao != null)
        {
            audioSource.pitch = ovoDourado ? 1.2f : 0.8f; // Tom diferente pra ativação/desativação
            audioSource.PlayOneShot(somAtivacao);
        }

        // Vibração no controle (se mobile ou gamepad)
        if (Application.isMobilePlatform)
            Handheld.Vibrate();

        // Atualizar modelo após um pequeno delay pra sincronizar com animação
        DOVirtual.DelayedCall(0.1f, AtualizarModelo);

        ovoDourado = false;
    }

    private void AtualizarModelo()
    {
        if (modeloInativo != null)
            modeloInativo.SetActive(!ovoDourado);

        if (modeloAtivo != null)
            modeloAtivo.SetActive(ovoDourado);
    }

    // Se quiser chamar por input
}