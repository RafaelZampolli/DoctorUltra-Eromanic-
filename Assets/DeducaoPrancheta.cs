using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections;

public class SistemaDeducaoPrancheta : MonoBehaviour
{
    private CartaHoverSuave[] cartasNaCena;

    [Header("Configurações da Prancheta")]
    public GameObject prancheta;
    public float tempoAnimacao = 0.3f;

    [Header("Game Feel")]
    public float intensidadeFlutuacao = 0.5f;
    public float velocidadeFlutuacao = 1.5f;
    public float intensidadePulse = 0.05f;

    [Header("pranchetaFloat")]
    public float amplitudeFloat = 0.03f;
    public float velocidadeFloat = 2f;
    public float amplitudeRotacao = 1.5f;
    float offset;

    [Header("Efeitos de Clique")]
    public float intensidadePunch = 0.15f;
    public float duracaoPunch = 0.2f;
    public float rotacaoFlick = 10f;
    public float duracaoFlick = 0.15f;

    private Vector3 pranchetaPosBase;
    private Quaternion pranchetaRotBase;
    private float pranchetaFloatOffset;
    private Coroutine pranchetaFloatCoroutine;

    private float zFixo;

    [Header("ScriptableObjects das Doenças")]
    public Doenca[] todasAsDoencas;

    [Header("Botões de Sintomas de Carta")]
    public BotaoAlternadorSprites botaoFebre;
    public BotaoAlternadorSprites botaoTosse;
    public BotaoAlternadorSprites botaoNausea;
    public BotaoAlternadorSprites botaoCalafrio;
    public BotaoAlternadorSprites botaoDorCabeca;
    public BotaoAlternadorSprites botaoProblemasRespiratorios;

    // --- NOVO CABEÇALHO E BOTÕES ---
    [Header("Botões de Sintomas Visuais")]
    public BotaoAlternadorSprites botaoMagrezaExtrema;
    public BotaoAlternadorSprites botaoManchasNaPele;
    public BotaoAlternadorSprites botaoQuedaDeCabelo;
    public BotaoAlternadorSprites botaoSudorese;
    public BotaoAlternadorSprites botaoCansaco;
    public BotaoAlternadorSprites botaoDorNoPeito;
    public BotaoAlternadorSprites botaoFraturaExposta;
    // ---------------------------------

    [Header("Botões de Doenças")]
    public BotaoAlternadorSprites botaoMalaria;
    public BotaoAlternadorSprites botaoSarampo;
    public BotaoAlternadorSprites botaoPneumonia;
    public BotaoAlternadorSprites botaoColera;

    [Header("Referências")]
    public ManagerPacientes managerPacientes;
    public ManagerTurno managerTurno;
    public Animator assinaturaAnimator;
    public Button botaoConfirmar;
    public Animator animadorBotaoConfirmar;

    private Dictionary<string, int> estadosSintomas;
    public Dictionary<string, int> estadosDoencas;
    private Dictionary<string, List<string>> sintomasPorDoenca;

    private Vector3 posicaoOriginalPrancheta;
    private Coroutine flutuacaoCoroutine;
    private Dictionary<GameObject, Coroutine> pulseCoroutines = new Dictionary<GameObject, Coroutine>();
    private Dictionary<GameObject, Coroutine> punchCoroutines = new Dictionary<GameObject, Coroutine>();
    private Dictionary<GameObject, Coroutine> flickCoroutines = new Dictionary<GameObject, Coroutine>();
    private Dictionary<Transform, Vector3> escalaOriginalDosBotoes = new Dictionary<Transform, Vector3>();

    private string doencaSelecionada = null;

    void Start()
    {
        InicializarSistema();
        ConfigurarBotoes();
        ConfigurarSintomasPorDoenca();

        cartasNaCena = FindObjectsOfType<CartaHoverSuave>(true);
        offset = pranchetaFloatOffset;

        if (prancheta != null)
        {
            posicaoOriginalPrancheta = prancheta.transform.position;
            pranchetaPosBase = prancheta.transform.position;
            zFixo = prancheta.transform.position.z;
            pranchetaRotBase = prancheta.transform.rotation;
            flutuacaoCoroutine = StartCoroutine(FlutuacaoPrancheta());
            pranchetaFloatOffset = Random.Range(0f, Mathf.PI * 2f);
            pranchetaFloatCoroutine = StartCoroutine(PranchetaFloatStyleCartas());
        }
        CacheOriginalScales();

        if (botaoConfirmar != null)
        {
            botaoConfirmar.onClick.AddListener(OnConfirmarClick);
            botaoConfirmar.interactable = false;
        }
    }

    // --- MÉTODO ATUALIZADO ---
    void InicializarSistema()
    {
        estadosSintomas = new Dictionary<string, int> {
            // Sintomas de Carta
            {"febre", 0}, {"tosse", 0}, {"nausea", 0}, {"calafrio", 0}, {"dorcabeca", 0}, {"problemasrespiratorios", 0},
            // Sintomas Visuais
            {"magrezaextrema", 0}, {"manchasnapele", 0}, {"quedadecabelo", 0}, {"sudorese", 0}, {"cansaco", 0}, {"dornopeito", 0}, {"fraturaexposta", 0}
        };

        estadosDoencas = new Dictionary<string, int> {
            {"malaria", 0}, {"sarampo", 0}, {"pneumonia", 0}, {"colera", 0}
        };
    }

    // --- MÉTODO ATUALIZADO ---
    void ConfigurarBotoes()
    {
        // Sintomas de Carta
        ConfigurarBotaoSintoma(botaoFebre, "febre");
        ConfigurarBotaoSintoma(botaoTosse, "tosse");
        ConfigurarBotaoSintoma(botaoNausea, "nausea");
        ConfigurarBotaoSintoma(botaoCalafrio, "calafrio");
        ConfigurarBotaoSintoma(botaoDorCabeca, "dorcabeca");
        ConfigurarBotaoSintoma(botaoProblemasRespiratorios, "problemasrespiratorios");

        // Sintomas Visuais
        ConfigurarBotaoSintoma(botaoMagrezaExtrema, "magrezaextrema");
        ConfigurarBotaoSintoma(botaoManchasNaPele, "manchasnapele");
        ConfigurarBotaoSintoma(botaoQuedaDeCabelo, "quedadecabelo");
        ConfigurarBotaoSintoma(botaoSudorese, "sudorese");
        ConfigurarBotaoSintoma(botaoCansaco, "cansaco");
        ConfigurarBotaoSintoma(botaoDorNoPeito, "dornopeito");
        ConfigurarBotaoSintoma(botaoFraturaExposta, "fraturaexposta");

        // Doenças
        ConfigurarBotaoDoenca(botaoMalaria, "malaria");
        ConfigurarBotaoDoenca(botaoSarampo, "sarampo");
        ConfigurarBotaoDoenca(botaoPneumonia, "pneumonia");
        ConfigurarBotaoDoenca(botaoColera, "colera");
    }

    // --- MÉTODO ATUALIZADO ---
    void ConfigurarSintomasPorDoenca()
    {
        sintomasPorDoenca = new Dictionary<string, List<string>>();
        if (todasAsDoencas != null && todasAsDoencas.Length > 0)
        {
            foreach (var doenca in todasAsDoencas)
            {
                if (doenca == null) continue;
                string chaveDoenca = MapearNomeDoenca(doenca.nome);
                if (string.IsNullOrEmpty(chaveDoenca)) continue;

                List<string> sintomas = new List<string>();
                // Sintomas de Carta
                if (doenca.febre) sintomas.Add("febre");
                if (doenca.tosse) sintomas.Add("tosse");
                if (doenca.nauseaOuVomito) sintomas.Add("nausea");
                if (doenca.calafrio) sintomas.Add("calafrio");
                if (doenca.dorDeCabeca) sintomas.Add("dorcabeca");
                if (doenca.ProblemasRespiratorios) sintomas.Add("problemasrespiratorios");

                // Sintomas Visuais
                if (doenca.magrezaExtrema) sintomas.Add("magrezaextrema");
                if (doenca.manchasNaPele) sintomas.Add("manchasnapele");
                if (doenca.quedaDeCabelo) sintomas.Add("quedadecabelo");
                if (doenca.sudorese) sintomas.Add("sudorese");
                if (doenca.cansaco) sintomas.Add("cansaco");
                if (doenca.dorNoPeito) sintomas.Add("dornopeito");
                if (doenca.fraturaExposta) sintomas.Add("fraturaexposta");

                sintomasPorDoenca[chaveDoenca] = sintomas;
            }
        }
        else { ConfiguracaoPadrao(); }
    }

    // --- MÉTODO ATUALIZADO ---
    void CacheOriginalScales()
    {
        BotaoAlternadorSprites[] todosBotoes = {
            // Sintomas de Carta
            botaoFebre, botaoTosse, botaoNausea, botaoCalafrio, botaoDorCabeca, botaoProblemasRespiratorios,
            // Sintomas Visuais
            botaoMagrezaExtrema, botaoManchasNaPele, botaoQuedaDeCabelo, botaoSudorese, botaoCansaco, botaoDorNoPeito, botaoFraturaExposta,
            // Doenças
            botaoMalaria, botaoSarampo, botaoPneumonia, botaoColera
        };
        foreach (var botao in todosBotoes)
        {
            if (botao != null) escalaOriginalDosBotoes[botao.transform] = botao.transform.localScale;
        }
    }

    // --- MÉTODO ATUALIZADO ---
    BotaoAlternadorSprites ObterBotaoSintoma(string nome)
    {
        switch (nome)
        {
            // Sintomas de Carta
            case "febre": return botaoFebre;
            case "tosse": return botaoTosse;
            case "nausea": return botaoNausea;
            case "calafrio": return botaoCalafrio;
            case "dorcabeca": return botaoDorCabeca;
            case "problemasrespiratorios": return botaoProblemasRespiratorios;

            // Sintomas Visuais
            case "magrezaextrema": return botaoMagrezaExtrema;
            case "manchasnapele": return botaoManchasNaPele;
            case "quedadecabelo": return botaoQuedaDeCabelo;
            case "sudorese": return botaoSudorese;
            case "cansaco": return botaoCansaco;
            case "dornopeito": return botaoDorNoPeito;
            case "fraturaexposta": return botaoFraturaExposta;

            default: return null;
        }
    }

    #region // --- MÉTODOS SEM ALTERAÇÃO ---

    public void SelecionarDoencaParaDiagnostico(string nomeDoenca)
    {
        if (!estadosDoencas.ContainsKey(nomeDoenca) || estadosDoencas[nomeDoenca] == 2)
        {
            Debug.Log($"Não é possível selecionar '{nomeDoenca}', pois já foi descartada.");
            return;
        }

        if (doencaSelecionada == nomeDoenca)
        {
            estadosDoencas[nomeDoenca] = 0;
            AtualizarSpriteDoenca(nomeDoenca, 0);
            doencaSelecionada = null;
            if (botaoConfirmar != null)
            {
                botaoConfirmar.interactable = false;
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(doencaSelecionada))
            {
                if (estadosDoencas.ContainsKey(doencaSelecionada))
                {
                    estadosDoencas[doencaSelecionada] = 0;
                    AtualizarSpriteDoenca(doencaSelecionada, 0);
                }
            }

            doencaSelecionada = nomeDoenca;
            estadosDoencas[nomeDoenca] = 1;
            AtualizarSpriteDoenca(nomeDoenca, 1);
            if (botaoConfirmar != null)
            {
                botaoConfirmar.interactable = true;
            }
        }

        AplicarEfeitosBotao(ObterBotaoDoenca(nomeDoenca));
    }

    private void OnConfirmarClick()
    {
        if (string.IsNullOrEmpty(doencaSelecionada)) return;

        if (animadorBotaoConfirmar != null)
        {
            animadorBotaoConfirmar.SetTrigger("Confirmar");
        }

        if (botaoConfirmar != null)
        {
            botaoConfirmar.interactable = false;
        }
        StartCoroutine(ExecutarDiagnosticoAposAnimacao());
    }

    private IEnumerator ExecutarDiagnosticoAposAnimacao()
    {
        yield return new WaitForSeconds(0.5f);
        if (assinaturaAnimator != null)
        {
            assinaturaAnimator.SetTrigger("assinatura");
        }
        yield return new WaitForSeconds(0.5f);

        if (managerTurno == null)
        {
            Debug.LogError("ERRO CRÍTICO: A referência para o ManagerTurno está NULA na prancheta!");
            yield break;
        }

        Debug.Log($"Diagnosticando: {doencaSelecionada}");
        bool diagnosticoCorreto = VerificarDiagnostico(doencaSelecionada);

        if (diagnosticoCorreto)
        {
            Debug.Log("✅ DIAGNÓSTICO CORRETO! Sinalizando cura...");
            switch (doencaSelecionada)
            {
                case "malaria":
                    managerTurno.CuraMalarias();
                    break;
                case "sarampo":
                    managerTurno.CuraSarampos();
                    break;
                case "pneumonia":
                    managerTurno.CuraPneumonias();
                    break;
                case "colera":
                    managerTurno.CuraColeras();
                    break;
            }
        }
        else
        {
            if (managerTurno.cronometro != null)
            {
                managerTurno.cronometro.AdicionarCulpa(1);
                managerTurno.cronometro.SinalizarErroDeDiagnostico();
            }
            managerTurno.FimDoPaciente();
        }

        doencaSelecionada = null;
    }

    void ConfigurarBotaoSintoma(BotaoAlternadorSprites botao, string nomeSintoma)
    {
        if (botao == null) return;
        var eventTrigger = botao.GetComponent<EventTrigger>() ?? botao.gameObject.AddComponent<EventTrigger>();
        eventTrigger.triggers.Clear();
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener((data) =>
        {
            PointerEventData pointerData = (PointerEventData)data;
            if (pointerData.button == PointerEventData.InputButton.Left) AlternarEstadoSintoma(nomeSintoma);
            else if (pointerData.button == PointerEventData.InputButton.Right) RiscarSintoma(nomeSintoma);
        });
        eventTrigger.triggers.Add(entry);
    }

    void ConfigurarBotaoDoenca(BotaoAlternadorSprites botao, string nomeDoenca)
    {
        if (botao == null) return;
        var eventTrigger = botao.GetComponent<EventTrigger>() ?? botao.gameObject.AddComponent<EventTrigger>();
        eventTrigger.triggers.Clear();
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener((data) =>
        {
            PointerEventData pointerData = (PointerEventData)data;
            if (pointerData.button == PointerEventData.InputButton.Left)
            {
                SelecionarDoencaParaDiagnostico(nomeDoenca);
            }
        });
        eventTrigger.triggers.Add(entry);
    }

    public string MapearNomeDoenca(string nomeOriginal)
    {
        string nome = nomeOriginal.ToLower().Replace(" ", "");
        if (nome.Contains("malaria")) return "malaria";
        if (nome.Contains("sarampo")) return "sarampo";
        if (nome.Contains("pneumonia")) return "pneumonia";
        if (nome.Contains("colera") || nome.Contains("cólera")) return "colera";
        return null;
    }

    void ConfiguracaoPadrao()
    {
        sintomasPorDoenca["malaria"] = new List<string> { "febre", "nausea", "tosse" };
        sintomasPorDoenca["sarampo"] = new List<string> { "febre", "calafrio", "dorcabeca" };
        sintomasPorDoenca["pneumonia"] = new List<string> { "tosse", "febre", "problemasrespiratorios", "calafrio" };
        sintomasPorDoenca["colera"] = new List<string> { "nausea", "dorcabeca", "calafrio" };
    }

    public void AlternarEstadoSintoma(string nomeSintoma)
    {
        if (!estadosSintomas.ContainsKey(nomeSintoma)) return;
        estadosSintomas[nomeSintoma] = (estadosSintomas[nomeSintoma] == 0) ? 1 : 0;
        AtualizarSpriteSintoma(nomeSintoma, estadosSintomas[nomeSintoma]);
        AplicarEfeitosBotao(ObterBotaoSintoma(nomeSintoma));
        RecalcularTodasDeducoes();
    }

    public void RiscarSintoma(string nomeSintoma)
    {
        if (!estadosSintomas.ContainsKey(nomeSintoma)) return;
        estadosSintomas[nomeSintoma] = (estadosSintomas[nomeSintoma] == 2) ? 0 : 2;
        AtualizarSpriteSintoma(nomeSintoma, estadosSintomas[nomeSintoma]);
        AplicarEfeitosBotao(ObterBotaoSintoma(nomeSintoma));
        RecalcularTodasDeducoes();
    }

    void RecalcularTodasDeducoes()
    {
        foreach (var kvp in estadosDoencas.Keys.ToList())
        {
            if (kvp != doencaSelecionada)
            {
                estadosDoencas[kvp] = 0;
                AtualizarSpriteDoenca(kvp, 0);
            }
        }

        foreach (var sintoma in estadosSintomas)
        {
            if (sintoma.Value == 0) continue;
            bool sintomaPresente = (sintoma.Value == 1);

            foreach (var doenca in sintomasPorDoenca)
            {
                if (sintomaPresente)
                {
                    if (!doenca.Value.Contains(sintoma.Key)) MarcarDoencaComo(doenca.Key, 2);
                }
                else if (sintoma.Value == 2)
                {
                    if (doenca.Value.Contains(sintoma.Key)) MarcarDoencaComo(doenca.Key, 2);
                }
            }
        }
    }

    void MarcarDoencaComo(string nomeDoenca, int estado)
    {
        if (estadosDoencas[nomeDoenca] != 1 || estado == 2)
        {
            estadosDoencas[nomeDoenca] = estado;
            AtualizarSpriteDoenca(nomeDoenca, estado);
        }
    }

    bool VerificarDiagnostico(string doencaDiagnosticada)
    {
        if (managerPacientes?.doencaAtual == null) return false;
        string doencaCorreta = MapearNomeDoenca(managerPacientes.doencaAtual.nome);
        return doencaDiagnosticada == doencaCorreta;
    }

    public void LimparPrancheta()
    {
        foreach (var key in estadosSintomas.Keys.ToList()) { estadosSintomas[key] = 0; AtualizarSpriteSintoma(key, 0); }
        foreach (var key in estadosDoencas.Keys.ToList()) { estadosDoencas[key] = 0; AtualizarSpriteDoenca(key, 0); }
        doencaSelecionada = null;
        if (botaoConfirmar != null) botaoConfirmar.interactable = false;
        Debug.Log("Prancheta limpa e desbloqueada!");
    }

    IEnumerator PranchetaFloatStyleCartas()
    {
        while (true)
        {
            float tempo = Time.time + offset;
            float floatY = Mathf.Sin(tempo * velocidadeFloat) * amplitudeFloat;
            float floatX = Mathf.Sin(tempo * velocidadeFloat * 0.7f) * amplitudeFloat * 0.5f;
            float rotacaoZ = Mathf.Sin(tempo * velocidadeFloat * 0.8f) * amplitudeRotacao;

            if (prancheta != null && prancheta.activeSelf)
            {
                prancheta.transform.position = new Vector3(
                    pranchetaPosBase.x + floatX,
                    pranchetaPosBase.y + floatY,
                    zFixo
                );
                prancheta.transform.rotation = pranchetaRotBase * Quaternion.Euler(0f, 0f, rotacaoZ);
            }
            yield return null;
        }
    }

    IEnumerator FlutuacaoPrancheta()
    {
        float tempo = 0;
        while (true)
        {
            tempo += Time.deltaTime * velocidadeFlutuacao;
            float offsetY = Mathf.Sin(tempo) * intensidadeFlutuacao;
            float offsetX = Mathf.Cos(tempo * 0.8f) * intensidadeFlutuacao * 0.8f;
            if (prancheta != null && prancheta.activeSelf)
            {
                prancheta.transform.position = new Vector3(
                    posicaoOriginalPrancheta.x + offsetX,
                    posicaoOriginalPrancheta.y + offsetY,
                    zFixo
                );
            }
            yield return null;
        }
    }

    void AplicarEfeitosBotao(BotaoAlternadorSprites botao)
    {
        if (botao == null) return;
        AplicarPulseBotao(botao);
        AplicarPunchEffect(botao);
        AplicarFlickEffect(botao);
    }

    void AplicarPulseBotao(BotaoAlternadorSprites botao)
    {
        GameObject obj = botao.gameObject;
        if (pulseCoroutines.ContainsKey(obj) && pulseCoroutines[obj] != null) StopCoroutine(pulseCoroutines[obj]);
        pulseCoroutines[obj] = StartCoroutine(PulseEffect(obj.transform));
    }

    void AplicarPunchEffect(BotaoAlternadorSprites botao)
    {
        GameObject obj = botao.gameObject;
        if (punchCoroutines.ContainsKey(obj) && punchCoroutines[obj] != null) StopCoroutine(punchCoroutines[obj]);
        punchCoroutines[obj] = StartCoroutine(PunchEffect(obj.transform));
    }

    void AplicarFlickEffect(BotaoAlternadorSprites botao)
    {
        GameObject obj = botao.gameObject;
        if (flickCoroutines.ContainsKey(obj) && flickCoroutines[obj] != null) StopCoroutine(flickCoroutines[obj]);
        flickCoroutines[obj] = StartCoroutine(FlickEffect(obj.transform));
    }

    IEnumerator PulseEffect(Transform target)
    {
        if (!escalaOriginalDosBotoes.ContainsKey(target)) yield break;
        Vector3 escalaBase = escalaOriginalDosBotoes[target];
        Vector3 escalaMaxima = escalaBase * (1f + intensidadePulse);
        float t = 0;
        target.localScale = escalaBase;
        while (t < tempoAnimacao * 0.5f) { t += Time.deltaTime; target.localScale = Vector3.Lerp(escalaBase, escalaMaxima, Mathf.SmoothStep(0, 1, t / (tempoAnimacao * 0.5f))); yield return null; }
        t = 0;
        while (t < tempoAnimacao * 0.5f) { t += Time.deltaTime; target.localScale = Vector3.Lerp(escalaMaxima, escalaBase, Mathf.SmoothStep(0, 1, t / (tempoAnimacao * 0.5f))); yield return null; }
        target.localScale = escalaBase;
    }

    IEnumerator PunchEffect(Transform target)
    {
        Vector3 posicaoOriginal = target.localPosition;
        float tempo = 0;
        while (tempo < duracaoPunch)
        {
            tempo += Time.deltaTime;
            float progresso = tempo / duracaoPunch;
            float shake = Mathf.Sin(progresso * Mathf.PI * 8) * intensidadePunch * (1f - progresso);
            target.localPosition = posicaoOriginal + new Vector3(Random.Range(-shake, shake), Random.Range(-shake, shake), 0);
            yield return null;
        }
        target.localPosition = posicaoOriginal;
    }

    IEnumerator FlickEffect(Transform target)
    {
        Quaternion rotacaoOriginal = target.localRotation;
        float tempo = 0;
        float anguloInicial = Random.Range(-rotacaoFlick, rotacaoFlick);
        while (tempo < duracaoFlick)
        {
            tempo += Time.deltaTime;
            float progresso = tempo / duracaoFlick;
            float elasticOut = Mathf.Pow(2, -10 * progresso) * Mathf.Sin((progresso - 0.075f) * (2 * Mathf.PI) / 0.3f) + 1;
            float anguloAtual = Mathf.Lerp(anguloInicial, 0, elasticOut);
            target.localRotation = rotacaoOriginal * Quaternion.Euler(0, 0, anguloAtual);
            yield return null;
        }
        target.localRotation = rotacaoOriginal;
    }

    void AtualizarSpriteSintoma(string nomeSintoma, int estado)
    {
        BotaoAlternadorSprites botao = ObterBotaoSintoma(nomeSintoma);
        if (botao != null) botao.DefinirSprite(estado);
    }

    public void AtualizarSpriteDoenca(string nomeDoenca, int estado)
    {
        BotaoAlternadorSprites botao = ObterBotaoDoenca(nomeDoenca);
        if (botao != null) botao.DefinirSprite(estado);
    }

    BotaoAlternadorSprites ObterBotaoDoenca(string nome)
    {
        switch (nome)
        {
            case "malaria": return botaoMalaria;
            case "sarampo": return botaoSarampo;
            case "pneumonia": return botaoPneumonia;
            case "colera": return botaoColera;
            default: return null;
        }
    }

    public void AbrirPrancheta()
    {
        if (prancheta != null)
        {
            prancheta.SetActive(true);
            if (flutuacaoCoroutine != null) StopCoroutine(flutuacaoCoroutine);
            flutuacaoCoroutine = StartCoroutine(FlutuacaoPrancheta());
        }
        if (cartasNaCena != null) foreach (var carta in cartasNaCena) if (carta != null) carta.EsconderCarta();
    }

    public void FecharPrancheta()
    {
        if (prancheta != null)
        {
            prancheta.SetActive(false);
            if (flutuacaoCoroutine != null) { StopCoroutine(flutuacaoCoroutine); flutuacaoCoroutine = null; }
        }
        if (cartasNaCena != null) foreach (var carta in cartasNaCena) if (carta != null) carta.MostrarCarta();
    }

    public void NovoPaciente() => LimparPrancheta();
    public void PacienteCurado() => Invoke(nameof(LimparPrancheta), 1.5f);

    public void SintomaDetectadoPorCarta(string nomeSintoma, bool presente)
    {
        if (!estadosSintomas.ContainsKey(nomeSintoma)) return;
        estadosSintomas[nomeSintoma] = presente ? 1 : 2;
        AtualizarSpriteSintoma(nomeSintoma, estadosSintomas[nomeSintoma]);
        AplicarEfeitosBotao(ObterBotaoSintoma(nomeSintoma));
        RecalcularTodasDeducoes();
    }

    void OnDestroy()
    {
        if (flutuacaoCoroutine != null) StopCoroutine(flutuacaoCoroutine);
        if (pranchetaFloatCoroutine != null) StopCoroutine(pranchetaFloatCoroutine);
        foreach (var coroutine in pulseCoroutines.Values) if (coroutine != null) StopCoroutine(coroutine);
        foreach (var coroutine in punchCoroutines.Values) if (coroutine != null) StopCoroutine(coroutine);
        foreach (var coroutine in flickCoroutines.Values) if (coroutine != null) StopCoroutine(coroutine);
    }
    #endregion
}