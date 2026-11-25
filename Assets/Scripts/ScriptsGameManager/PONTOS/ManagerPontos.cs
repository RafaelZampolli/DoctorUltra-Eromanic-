using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ManagerPontos : MonoBehaviour
{
    public float TempoAtual;
    public bool Contando;
    public int Rodada = 0;

    private bool tempoJaPausado = false;

    [Header("Referências")]
    public ManagerTurno PularTurno;
    public AmpulhetaManager AmpulhetaManager;
    public ManekiNeko manekiNeko;
    public OvoDourado Ovodourado;
    public Text DoacoesTexto;
    public Text PontosTexto;

    [Header("Efeitos Visuais (URP)")]
    public Volume postProcessVolume; // Arraste o seu Volume aqui
    private ColorAdjustments colorAdjustments;

    [Header("Pontuação - SOMENTE LEITURA")]
    [SerializeField] public int pontos;
    [SerializeField] public int doacoes;
    [SerializeField] private int culpa;

    [Header("Planos")]
    public GameObject GameOver;
    public GameObject Ovo;

    [Header("Game Over")]
    public GameOverManager gameOverManager;

    private bool foiTimeOut = false;
    private bool foiErroDeDiagnostico = false; // NOVA FLAG

    // Propriedades públicas
    public int Pontos
    {
        get => pontos;
        set => pontos = value;
    }
    public int Doacoes
    {
        get => doacoes;
        set => doacoes = value;
    }
    public int Culpa
    {
        get => culpa;
        set => culpa = value;
    }

    private bool gameOverJaFoiChamado = false;

    void Start()
    {
        GameOver.SetActive(false);
        culpa = 0;

        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGet(out colorAdjustments);
        }
        else
        {
            Debug.LogWarning("Volume (URP) não referenciado no ManagerPontos. O efeito preto e branco d'O Mundo não funcionará.");
        }
    }

    private void AtualizarDoacoes()
    {
        float bonusDoacoes = manekiNeko != null ? manekiNeko.ModficadorDoacoes : 0f;
        int doacoesBase = pontos / 10;
        int doacoesComBonus = doacoesBase + (int)(doacoesBase * bonusDoacoes);

        if (CartasEffectManager.RodaDaFortunaUsosRestantes > 0)
        {
            doacoesComBonus = Mathf.CeilToInt(doacoesComBonus * 1.4f);
            Debug.Log($"Roda da Fortuna ativa! Doações multiplicadas para {doacoesComBonus}");
        }

        // Nunca diminua doações automaticamente!
        if (doacoesComBonus > doacoes)
            doacoes = doacoesComBonus;
        // Se quiser permitir perder dinheiro por penalidade, faça via método específico
    }

    public void PausarTempoPorSegundos(float duracao)
    {
        if (Contando && !tempoJaPausado)
        {
            StartCoroutine(PausarTempoCoroutine(duracao));
        }
    }

    private IEnumerator PausarTempoCoroutine(float duracao)
    {
        tempoJaPausado = true;
        Contando = false;

        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value = -100f;
        }

        yield return new WaitForSeconds(duracao);

        if (!foiTimeOut)
        {
            Contando = true;
        }

        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value = 0f;
        }

        tempoJaPausado = false;
    }

    void Update()
    {
        if (Contando)
        {
            TempoAtual += Time.deltaTime;

            if (VerificarSeEstourouTempo())
            {
                ForcarSaidaPaciente();
            }
        }

        // UI ATUALIZADA
        if (DoacoesTexto != null)
            DoacoesTexto.text = "Doações: " + Doacoes.ToString() + "R$";

        if (PontosTexto != null)
            PontosTexto.text = "Score: " + Pontos.ToString() + " ULTRA!";

        // Lógica de Game Over
        if (Culpa >= 3 && Ovodourado.ovoDourado == false && !gameOverJaFoiChamado)
        {
            gameOverJaFoiChamado = true;
            Debug.Log("ENTRANDO NO GAME OVER!");
            gameOverManager.ShowGameOver();
        }
        else if (Culpa >= 3 && Ovodourado.ovoDourado == true)
        {
            Culpa = 0;
            gameOverJaFoiChamado = false;
            Debug.Log("[ManagerPontos] Ovo Dourado ativado! Culpa resetada.");
        }

        if (Input.GetKeyDown(KeyCode.F12))
        {
            if (Contando)
            {
                Debug.Log("[ManagerPontos DEBUG] F12 - Forçando timeout!");
                ForcarSaidaPaciente();
            }
            else
            {
                Debug.Log("[ManagerPontos DEBUG] F12 - Timer não está ativo!");
            }
        }
    }

    public void IniciarContador()
    {
        TempoAtual = 0f;
        Contando = true;
        foiTimeOut = false;
        foiErroDeDiagnostico = false;
        Ovo.SetActive(false);

        if (CartasEffectManager.RodaDaFortunaUsosRestantes > 0)
        {
            CartasEffectManager.RodaDaFortunaUsosRestantes--;
            Debug.Log($"Um uso da Roda da Fortuna consumido. Restam: {CartasEffectManager.RodaDaFortunaUsosRestantes}");
        }
    }

    public void PararContador()
    {
        Contando = false;
        ContarPontos();
    }

    public void ContarPontos()
    {
        // Não dar pontos se foi timeout ou erro de diagnóstico!
        if (foiTimeOut || foiErroDeDiagnostico)
        {
            foiTimeOut = false;
            foiErroDeDiagnostico = false;
            Debug.Log("[ManagerPontos] Contagem de pontos abortada por timeout ou erro de diagnóstico.");
            return;
        }

        float bonusAmpulheta = AmpulhetaManager != null ? AmpulhetaManager.ModficadorAmpulheta : 0f;
        int pontosParaDar = 0;

        if (Rodada == 1)
        {
            pontosParaDar = CalcularPontosComPontos(10f, 15f, 20f, 25f, 30f,
                                                   130, 115, 110, 90, 75, bonusAmpulheta);
        }
        else if (Rodada == 2)
        {
            pontosParaDar = CalcularPontosComPontos(10f, 15f, 20f, 25f, 30f,
                                                   130, 110, 90, 70, 70, bonusAmpulheta);
        }
        else if (Rodada == 3)
        {
            pontosParaDar = CalcularPontosComPontos(7f, 14f, 17f, 20f, 25f,
                                                   130, 105, 90, 70, 70, bonusAmpulheta);
        }
        else if (Rodada >= 4)
        {
            pontosParaDar = CalcularPontosComPontos(5f, 9f, 12f, 15f, 20f,
                                                   130, 105, 95, 70, 70, bonusAmpulheta);
        }

        if (pontosParaDar > 0)
        {
            pontos += pontosParaDar;
            AtualizarDoacoes();
            Debug.Log($"[ManagerPontos] Adicionados {pontosParaDar} pontos! Tempo: {TempoAtual:F1}s");
        }

        TempoAtual = 0f;
    }

    // CORRIGIDO: Não dá pontos fora do tempo!
    private int CalcularPontosComPontos(
     float tempo1, float tempo2, float tempo3, float tempo4, float tempo5,
     int pontos1, int pontos2, int pontos3, int pontos4, int pontos5,
     float bonusAmpulheta)
    {
        float t1 = tempo1 + (tempo1 * bonusAmpulheta);
        float t2 = tempo2 + (tempo2 * bonusAmpulheta);
        float t3 = tempo3 + (tempo3 * bonusAmpulheta);
        float t4 = tempo4 + (tempo4 * bonusAmpulheta);
        float t5 = tempo5 + (tempo5 * bonusAmpulheta);

        if (TempoAtual <= t1) return pontos1;

        if (TempoAtual <= t2)
        {
            float fator = (TempoAtual - t1) / (t2 - t1);
            float pontosCalculados = pontos1 - (fator * (pontos1 - pontos2));
            return Mathf.RoundToInt(pontosCalculados);
        }
        if (TempoAtual <= t3)
        {
            float fator = (TempoAtual - t2) / (t3 - t2);
            float pontosCalculados = pontos2 - (fator * (pontos2 - pontos3));
            return Mathf.RoundToInt(pontosCalculados);
        }
        if (TempoAtual <= t4)
        {
            float fator = (TempoAtual - t3) / (t4 - t3);
            float pontosCalculados = pontos3 - (fator * (pontos3 - pontos4));
            return Mathf.RoundToInt(pontosCalculados);
        }
        if (TempoAtual <= t5)
        {
            float fator = (TempoAtual - t4) / (t5 - t4);
            float pontosCalculados = pontos4 - (fator * (pontos4 - pontos5));
            return Mathf.RoundToInt(pontosCalculados);
        }

        // NOVO: Se passou do último tempo, não ganha pontos!
        return 0;
    }

    private bool VerificarSeEstourouTempo()
    {
        return TempoAtual > GetTempoMaximoDoTurno();
    }

    private void ForcarSaidaPaciente()
    {
        Contando = false;
        TempoAtual = 0f;
        foiTimeOut = true;

        AdicionarCulpa(1);
        Debug.Log("TEMPO ESGOTADO! Paciente foi embora frustrado. Culpa atual: " + Culpa);

        if (PularTurno != null)
            PularTurno.FimDoPaciente();
    }

    // Métodos utilitários para debug/admin
    public void AdicionarDoacoesExtras(int valor)
    {
        Doacoes += valor;
        Debug.Log($"[ManagerPontos] Doações extras adicionadas: +{valor}. Total: {Doacoes}");
    }

    public void ModificarDoacoesDiretamente(int novoValor)
    {
        Doacoes = novoValor;
        Debug.Log($"[ManagerPontos] Doações modificadas diretamente: {novoValor}");
    }

    public void AdicionarPontos(int pontosBase)
    {
        Pontos += pontosBase;
        float bonusDoacoes = manekiNeko != null ? manekiNeko.ModficadorDoacoes : 0f;
        int doacoesBase = pontosBase + (int)(pontosBase * bonusDoacoes);
        Doacoes += doacoesBase;
        Debug.Log($"Ganhou {pontosBase} pontos e {doacoesBase} doações! Tempo: {TempoAtual:F1}s");
    }

    public void AdicionarCulpa(int valor)
    {
        culpa += valor;
        Debug.Log($"[ManagerPontos] Adicionada {valor} culpa. Total: {culpa}");
    }

    public bool RemoverPontoDeCulpa()
    {
        if (culpa > 0)
        {
            culpa--;
            Debug.Log($"[ManagerPontos] Culpa removida. Agora: {culpa}");
            return true;
        }
        Debug.Log("[ManagerPontos] Nenhuma culpa para remover.");
        return false;
    }

    public void SinalizarErroDeDiagnostico()
    {
        foiErroDeDiagnostico = true;
    }

    public float GetTempoMaximoDoTurno()
    {
        float bonusAmpulheta = AmpulhetaManager != null ? AmpulhetaManager.ModficadorAmpulheta : 0f;
        float tempoMaximoBase = 0f;

        if (Rodada == 1)
            tempoMaximoBase = 60f;
        else if (Rodada == 2)
            tempoMaximoBase = 50f;
        else if (Rodada == 3)
            tempoMaximoBase = 40f;
        else if (Rodada >= 4)
            tempoMaximoBase = 30f;
        else
            tempoMaximoBase = 60f;

        return tempoMaximoBase + (tempoMaximoBase * bonusAmpulheta);
    }
}