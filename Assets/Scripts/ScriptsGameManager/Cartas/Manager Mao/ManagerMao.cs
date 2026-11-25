using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;
using System.Collections;

public class ManagerMao : MonoBehaviour
{
    [Header("Configurações da Mão")]
    public int MaxCartasMao;
    public Vector3 spawnPosition = new Vector3(0, 0, 0); // Defina no Inspector onde a carta nasce
    public Transform SpawnCarta;
    public SplineContainer splineContainer;

    [Header("Animações")]
    public float tempoMovimentoSpline = 0.4f; // Tempo pra mover pelo spline
    [Range(0.1f, 0.5f)] public float delayEntreCartas = 0.15f;

    [Header("Prefabs das Cartas")]
    public List<GameObject> PrefabsCartas;

    [Header("Nomes das cartas proibidas no spawn inicial")]
    public List<string> CartasProibidasNoStart = new() { "CartraAEstrela", "CartraForca" };

    private List<GameObject> cartasMao = new();

    private bool _spawnInicial = true;

    public void Start()
    {
        spawnPosition = SpawnCarta.transform.position;

        _spawnInicial = true;
        ChamarCarta();
        ChamarCarta();
        ChamarCarta();
        ChamarCarta();
        ChamarCarta();
        _spawnInicial = false;
    }

    public void ChamarCarta()
    {
        if (PrefabsCartas.Count == 0) return;
        int indiceAleatorio = EscolherIndiceAleatorio();
        ChamarCartaEspecifica(indiceAleatorio);
    }

    private int EscolherIndiceAleatorio()
    {
        // No spawn inicial, nunca sorteia índice das cartas proibidas
        if (_spawnInicial && CartasProibidasNoStart.Count > 0)
        {
            // Cria uma lista de índices permitidos
            List<int> permitidos = new();
            for (int i = 0; i < PrefabsCartas.Count; i++)
            {
                string nomeBase = PrefabsCartas[i].name;
                // Remove "(Clone)" se tiver no nome
                if (!CartasProibidasNoStart.Contains(nomeBase))
                    permitidos.Add(i);
            }
            if (permitidos.Count == 0)
                return 0; // fallback: retorna primeiro

            return permitidos[Random.Range(0, permitidos.Count)];
        }
        else
        {
            return Random.Range(0, PrefabsCartas.Count);
        }
    }

    public void ChamarCartaEspecifica(int indiceCarta)
    {
        if (cartasMao.Count >= MaxCartasMao) return;
        if (indiceCarta < 0 || indiceCarta >= PrefabsCartas.Count) return;

        GameObject prefabEscolhido = PrefabsCartas[indiceCarta];
        GameObject novaCarta = Instantiate(prefabEscolhido, spawnPosition, prefabEscolhido.transform.rotation);

        // ==================================================================
        // <<< ADIÇÃO PARA O TREVO FUNCIONAR >>>
        // Pega o script da carta recém-criada
        ComportamentoCarta compCarta = novaCarta.GetComponent<ComportamentoCarta>();
        if (compCarta != null)
        {
            // "Apresenta" este script (o ManagerMao) para a carta.
            compCarta.ConfigurarManager(this);
        }
        // ==================================================================

        // Garante escala 0.1 sempre
        novaCarta.transform.localScale = Vector3.one * 0.1f;

        // ... (o resto do método continua exatamente igual)
        var hoverComponent = novaCarta.GetComponent<CartaHoverSuave>();
        if (hoverComponent != null)
            hoverComponent.DesabilitarHover();

        cartasMao.Add(novaCarta);
        UpdatarPosicaoComAnimacao();
    }

    public void UpdatarPosicaoComAnimacao()
    {
        LimparCartasDestruidas();
        if (cartasMao.Count == 0) return;

        float espaco = 1f / MaxCartasMao;
        float inicio = 0.5f - (cartasMao.Count - 1) * espaco / 2f;
        Spline spline = splineContainer.Spline;

        for (int i = 0; i < cartasMao.Count; i++)
        {
            if (cartasMao[i] != null)
            {
                float p = Mathf.Clamp01(inicio + i * espaco);
                Vector3 posicaoFinal = (Vector3)spline.EvaluatePosition(p);

                // Garante escala 0.12 sempre
                cartasMao[i].transform.localScale = Vector3.one * 0.12f;

                // Apenas move, sem tocar em scale além disso
                cartasMao[i].transform.DOMove(posicaoFinal, tempoMovimentoSpline)
                    .SetEase(Ease.OutQuart);

                // Mantém rotação original do prefab
                GameObject prefabOriginal = PrefabsCartas.Find(prefab =>
                    prefab.name == cartasMao[i].name.Replace("(Clone)", ""));
                if (prefabOriginal != null)
                {
                    cartasMao[i].transform.DORotateQuaternion(prefabOriginal.transform.rotation, tempoMovimentoSpline)
                        .SetEase(Ease.OutQuart);
                }

                // Habilita hover ao final
                var hoverComponent = cartasMao[i].GetComponent<CartaHoverSuave>();
                if (hoverComponent != null)
                {
                    // Habilita hover após animação
                    cartasMao[i].transform.DOKill(false); // Cancela possíveis tweens antigos
                    cartasMao[i].transform.DOMove(posicaoFinal, tempoMovimentoSpline)
                        .OnComplete(() => hoverComponent.HabilitarHover());
                }
            }
        }
    }

    void LimparCartasDestruidas()
    {
        cartasMao.RemoveAll(carta => carta == null);
    }

    public void RemoverCarta(GameObject carta)
    {
        if (carta == null)
        {
            LimparCartasDestruidas();
            UpdatarPosicaoComAnimacao();
            return;
        }

        if (cartasMao.Contains(carta))
        {
            cartasMao.Remove(carta);

            Destroy(carta);

            UpdatarPosicaoComAnimacao();
        }
        else
        {
            LimparCartasDestruidas();
            UpdatarPosicaoComAnimacao();
        }
    }

    private void Update()
    {
        if (Time.frameCount % 60 == 0)
            LimparCartasDestruidas();

        if (Input.GetKeyDown(KeyCode.Space))
            ChamarCarta();
    }
}