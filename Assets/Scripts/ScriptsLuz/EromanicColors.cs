using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EromanicColors : MonoBehaviour
{
    [Header("Luzes para alterar cor")]
    [SerializeField] private Light luz1;
    [SerializeField] private Light luz2;

    [Header("Botão de mudança de cor")]
    [SerializeField] private Button botaoTrocarCor;

    [Header("Botão de voltar para tela anterior")]
    [SerializeField] private Button botaoVoltar;

    [Header("Cor alvo")]
    [SerializeField] private Color corNova = Color.red;

    [Header("Duração da transição (segundos)")]
    [SerializeField] private float duracaoTransicao = 1f;

    private Color corOriginalLuz1;
    private Color corOriginalLuz2;

    private Coroutine transicaoCoroutine1;
    private Coroutine transicaoCoroutine2;

    void Start()
    {
        if (luz1 == null || luz2 == null || botaoTrocarCor == null || botaoVoltar == null)
        {
            Debug.LogWarning("Atribua as duas luzes e os dois botões ao script EromanicColors!");
            enabled = false;
            return;
        }

        corOriginalLuz1 = luz1.color;
        corOriginalLuz2 = luz2.color;

        botaoTrocarCor.onClick.AddListener(() => TrocarCorSuave(corNova));
        botaoVoltar.onClick.AddListener(() => TrocarCorSuave(corOriginalLuz1, corOriginalLuz2));
    }

    // Transição para a mesma cor nas duas luzes
    public void TrocarCorSuave(Color corDestino)
    {
        // Se já está rodando uma transição, interrompe
        if (transicaoCoroutine1 != null) StopCoroutine(transicaoCoroutine1);
        if (transicaoCoroutine2 != null) StopCoroutine(transicaoCoroutine2);

        transicaoCoroutine1 = StartCoroutine(TransicaoCor(luz1, luz1.color, corDestino, duracaoTransicao));
        transicaoCoroutine2 = StartCoroutine(TransicaoCor(luz2, luz2.color, corDestino, duracaoTransicao));
    }

    // Transição para cores diferentes em cada luz (usado para "voltar")
    public void TrocarCorSuave(Color corDestino1, Color corDestino2)
    {
        if (transicaoCoroutine1 != null) StopCoroutine(transicaoCoroutine1);
        if (transicaoCoroutine2 != null) StopCoroutine(transicaoCoroutine2);

        transicaoCoroutine1 = StartCoroutine(TransicaoCor(luz1, luz1.color, corDestino1, duracaoTransicao));
        transicaoCoroutine2 = StartCoroutine(TransicaoCor(luz2, luz2.color, corDestino2, duracaoTransicao));
    }

    // Coroutine para interpolar a cor suavemente
    private IEnumerator TransicaoCor(Light luz, Color corInicial, Color corFinal, float duracao)
    {
        float tempo = 0f;
        while (tempo < duracao)
        {
            luz.color = Color.Lerp(corInicial, corFinal, tempo / duracao);
            tempo += Time.deltaTime;
            yield return null;
        }
        luz.color = corFinal;
    }
}