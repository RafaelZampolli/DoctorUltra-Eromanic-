using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CartasEffectManager : MonoBehaviour
{
    [Header("Referências")]
    public ManagerPacientes managerPacientes;
    public ManagerTurno managerTurno;

    public static int RodaDaFortunaUsosRestantes = 0;

    public static CartasEffectManager Instance;
    private DynamicPopupManager popupManager;

    private static CartaData ultimaCartaUsadaData = null;
    public ManagerMao managerMao;
    public ManagerPontos managerPontos;

    public static bool DescontoSolAtivo = false;

    private static int palpiteMedicoContador = 0;
    private static bool palpiteMedicoAtivo = true;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (managerPacientes == null) managerPacientes = FindObjectOfType<ManagerPacientes>();
        popupManager = FindObjectOfType<DynamicPopupManager>();
        if (popupManager == null) Debug.LogError("FATAL: DynamicPopupManager não foi encontrado na cena!", this);
    }

    public void RegistrarUltimaCartaUsada(CartaData dados)
    {
        if (dados != null)
        {
            ultimaCartaUsadaData = dados;
            Debug.Log($"Última carta usada registrada: {dados.nomeCarta}");
        }
    }

    // --- CARTAS COM FEEDBACK JÁ EXISTENTE ---
    public void UsarTermometro() => VerificarSintoma("febre", "Febre");
    public void UsarEstetoscopio() => VerificarSintoma("tosse", "Tosse");
    public void UsarAnamnese() => VerificarSintoma("nausea", "Náusea");
    public void UsarExameGeral() => VerificarSintoma("calafrio", "Calafrio");
    public void UsarPilula() => VerificarSintoma("dorDeCabeca", "Dor de Cabeça");

    // --- CARTAS COM FEEDBACK ADICIONADO/MELHORADO ---

    public void UsarATemperanca()
    {
        if (managerPontos == null) managerPontos = FindObjectOfType<ManagerPontos>();
        if (managerPontos != null)
        {
            if (managerPontos.Culpa > 0)
            {
                managerPontos.Culpa -= 1;
                Debug.Log("XIV A Temperança: 1 ponto de culpa foi removido.");
                popupManager?.ShowPersistentFloatingFeedback("Um ponto de culpa foi removido!", GetPatientPosition(), true);
            }
            else
            {
                Debug.Log("XIV A Temperança: Nenhum ponto de culpa para remover.");
                popupManager?.ShowPersistentFloatingFeedback("Nenhuma culpa para remover.", GetPatientPosition(), false);
            }
        }
        else
        {
            Debug.LogError("ManagerPontos não encontrado!");
        }
    }

    public void UsarPrimeirosSocorros()
    {
        var aplicador = managerPacientes?.pacienteAtualObject?.GetComponent<AplicadorDeSintomasVisuais>();
        if (aplicador != null && managerPacientes.doencaAtual.fraturaExposta)
        {
            aplicador.facilitarProximoQTE = true;
            Debug.Log("🩹 Primeiros Socorros: Próximo minigame de fratura será facilitado.");
            popupManager?.ShowPersistentFloatingFeedback("Minigame facilitado!", GetPatientPosition(), true);
        }
        else
        {
            Debug.Log("🩹 Primeiros Socorros: Paciente não tem fratura exposta.");
            popupManager?.ShowPersistentFloatingFeedback("Não aplicável neste paciente.", GetPatientPosition(), false);
        }
    }

    public void UsarARodaDaFortuna()
    {
        RodaDaFortunaUsosRestantes = 2;
        Debug.Log("Roda da Fortuna ativada! Próximos 2 pacientes darão x1.4 doações.");
        popupManager?.ShowPersistentFloatingFeedback("Doações x1.4 por 2 pacientes!", GetPatientPosition(), true);
    }

    public void UsarOTolo()
    {
        if (managerTurno == null) managerTurno = FindObjectOfType<ManagerTurno>();
        if (managerTurno != null)
        {
            Debug.Log("🃏 O Tolo pula o paciente atual.");
            popupManager?.ShowPersistentFloatingFeedback("Paciente dispensado!", GetPatientPosition(), true);
            managerTurno.FimDoPaciente();

            // --- LÓGICA DA SORTE ATUALIZADA ---
            float chance = 0.25f * MultiplicadorDeChance; // Usa a nova propriedade
            if (Random.value < chance)
            // ------------------------------------
            {
                managerTurno.ReduzirTotalDePacientes(1);
                Debug.Log($"🃏 O Tolo assustou um paciente da fila! (Chance: {chance * 100}%)");
                popupManager?.ShowPersistentFloatingFeedback("Um paciente desistiu da fila!", Vector3.zero, false);
            }
        }
        else Debug.LogError("ManagerTurno não encontrado!");
    }

    public void UsarOMundo()
    {
        if (managerPontos == null) managerPontos = FindObjectOfType<ManagerPontos>();
        if (managerPontos != null)
        {
            Debug.Log("⏳ O Mundo: Tempo parado por 5 segundos.");
            popupManager?.ShowPersistentFloatingFeedback("O TEMPO PAROU!", Vector3.zero, true, 5.0f); // Feedback grande e central
            managerPontos.PausarTempoPorSegundos(5f);
        }
        else Debug.LogError("ManagerPontos não encontrado!");
    }

    public void UsarOSol()
    {
        if (!DescontoSolAtivo)
        {
            DescontoSolAtivo = true;
            Debug.Log("☀️ Efeito do Sol ativado! Próxima compra com desconto.");
            popupManager?.ShowPersistentFloatingFeedback("Desconto na próxima compra!", Vector3.zero, true);
        }
        else
        {
            Debug.Log("☀️ Efeito do Sol já está ativo.");
            popupManager?.ShowPersistentFloatingFeedback("Desconto já está ativo!", Vector3.zero, false);
        }
    }

    public void UsarAJustica()
    {
        if (popupManager == null) { HandlePopupManagerNotFound(); return; }
        if (managerMao == null) managerMao = FindObjectOfType<ManagerMao>();
        if (managerMao == null) { Debug.LogError("ManagerMao não encontrado!"); return; }

        List<string> opcoes = new List<string> { "Nova Carta Aleatória" };
        if (ultimaCartaUsadaData != null)
        {
            opcoes.Add($"Duplicar: {ultimaCartaUsadaData.nomeCarta}");
        }

        popupManager.ShowChoicePopup("A Justiça Decide", opcoes, (escolha) =>
        {
            if (escolha == "Nova Carta Aleatória")
            {
                managerMao.ChamarCarta();
                Debug.Log("⚖️ Justiça: Nova carta aleatória escolhida.");
            }
            else if (opcoes.Count > 1 && escolha == opcoes[1])
            {
                for (int i = 0; i < managerMao.PrefabsCartas.Count; i++)
                {
                    var compPrefab = managerMao.PrefabsCartas[i].GetComponent<ComportamentoCarta>();
                    if (compPrefab != null && compPrefab.dadosCarta.nomeCarta == ultimaCartaUsadaData.nomeCarta)
                    {
                        managerMao.ChamarCartaEspecifica(i);
                        Debug.Log($"⚖️ Justiça: Duplicando '{ultimaCartaUsadaData.nomeCarta}'.");
                        return;
                    }
                }
            }
        });
    }

    public void UsarPalpiteMedico()
    {
        if (!palpiteMedicoAtivo)
        {
            popupManager?.ShowPersistentFloatingFeedback("Efeito já utilizado ao máximo.", GetPatientPosition(), false);
            return;
        }

        palpiteMedicoContador++;

        // --- LÓGICA DA SORTE ATUALIZADA ---
        float chance = 0.25f * MultiplicadorDeChance; // Usa a nova propriedade
        bool deveForcar = (Random.value < chance || palpiteMedicoContador >= 4);
        // ------------------------------------

        if (deveForcar)
        {
            AplicadorDeSintomasVisuais aplicador = managerPacientes.pacienteAtualObject?.GetComponent<AplicadorDeSintomasVisuais>();
            if (aplicador != null)
            {
                var sintomasPossiveis = new List<TipoSintomaVisual> { TipoSintomaVisual.MagrezaExtrema, TipoSintomaVisual.ManchasNaPele, TipoSintomaVisual.QuedaDeCabelo, TipoSintomaVisual.Sudorese, TipoSintomaVisual.Cansaco, TipoSintomaVisual.DorNoPeito };
                int sorteio = Random.Range(0, sintomasPossiveis.Count);
                aplicador.AplicarSpriteObrigatorio(sintomasPossiveis[sorteio]);

                string nomeSintoma = System.Text.RegularExpressions.Regex.Replace(sintomasPossiveis[sorteio].ToString(), "(\\B[A-Z])", " $1");
                popupManager?.ShowPersistentFloatingFeedback($"Sintoma Visual: {nomeSintoma}!", GetPatientPosition(), true);
                Debug.Log($"🩺 Palpite Médico forçou o sintoma: {nomeSintoma} (Chance: {chance * 100}%)");
            }
        }
        else
        {
            popupManager?.ShowPersistentFloatingFeedback("Nenhum sintoma visual extra apareceu.", GetPatientPosition(), false);
            Debug.Log("🩺 Palpite Médico não ativou desta vez.");
        }

        if (palpiteMedicoContador >= 4)
        {
            palpiteMedicoAtivo = false;
        }
    }

    private float MultiplicadorDeChance
    {
        get
        {
            // Procura por um objeto ativo com a tag específica.
            GameObject amuletoSorte = GameObject.FindWithTag("AmuletoSorteAtivo");
            if (amuletoSorte != null && amuletoSorte.activeInHierarchy)
            {
                // Se encontrou o amuleto ativo, retorna o dobro da chance.
                return 2.0f;
            }
            // Caso contrário, retorna a chance normal.
            return 1.0f;
        }
    }

    public void UsarExperienciaMedica()
    {
        // --- LÓGICA DA SORTE ATUALIZADA ---
        float chance = 0.25f * MultiplicadorDeChance; // Usa a nova propriedade
        if (Random.value < chance)
        // ------------------------------------
        {
            var prancheta = FindObjectOfType<SistemaDeducaoPrancheta>();
            if (prancheta == null) return;
            string doencaCorreta = prancheta.MapearNomeDoenca(managerPacientes.doencaAtual?.nome ?? "");
            var candidatas = prancheta.estadosDoencas.Where(kvp => kvp.Value != 2 && kvp.Key != doencaCorreta).Select(kvp => kvp.Key).ToList();

            if (candidatas.Count > 0)
            {
                string escolhida = candidatas[Random.Range(0, candidatas.Count)];
                prancheta.estadosDoencas[escolhida] = 2;
                prancheta.AtualizarSpriteDoenca(escolhida, 2);
                Debug.Log($"🧠 Experiência Médica eliminou a suspeita de: {escolhida} (Chance: {chance * 100}%)");
                popupManager?.ShowPersistentFloatingFeedback($"Doença '{escolhida}' eliminada!", Vector3.zero, true);
            }
        }
        else
        {
            Debug.Log("🧠 Experiência Médica não ativou desta vez.");
            popupManager?.ShowPersistentFloatingFeedback("Nenhuma suspeita eliminada.", Vector3.zero, false);
        }
    }

    // --- POPUPS DE ESCOLHA (FEEDBACK É O PRÓPRIO POPUP) ---
    public void UsarAntiremetico()
    {
        if (popupManager == null) { HandlePopupManagerNotFound(); return; }
        popupManager.ShowChoicePopup("Antiemético", new List<string> { "Calafrio", "Náusea" }, (escolha) =>
        {
            if (escolha == "Calafrio") VerificarSintoma("calafrio", "Calafrio");
            else if (escolha == "Náusea") VerificarSintoma("nausea", "Náusea");
        });
    }

    public void UsarEstetoscopioAvancado()
    {
        if (popupManager == null) { HandlePopupManagerNotFound(); return; }
        popupManager.ShowChoicePopup("Estetoscópio Avançado", new List<string> { "Febre", "Tosse" }, (escolha) =>
        {
            if (escolha == "Febre") VerificarSintoma("febre", "Febre");
            else if (escolha == "Tosse") VerificarSintoma("tosse", "Tosse");
        });
    }

    public void UsarAForca()
    {
        if (popupManager == null) { HandlePopupManagerNotFound(); return; }
        popupManager.ShowChoicePopup("A Força", new List<string> { "Febre", "Tosse", "Náusea", "Calafrio", "Dor de Cabeça" }, (escolha) =>
        {
            if (escolha == "Febre") VerificarSintoma("febre", "Febre");
            else if (escolha == "Tosse") VerificarSintoma("tosse", "Tosse");
            else if (escolha == "Náusea") VerificarSintoma("nausea", "Náusea");
            else if (escolha == "Calafrio") VerificarSintoma("calafrio", "Calafrio");
            else if (escolha == "Dor de Cabeça") VerificarSintoma("dorDeCabeca", "Dor de Cabeça");
        });
    }

    public void UsarAEstrela()
    {
        if (popupManager == null) { HandlePopupManagerNotFound(); return; }
        if (managerPacientes?.pacienteAtualObject == null)
        {
            popupManager.ShowPersistentFloatingFeedback("Paciente não encontrado", GetPatientPosition(), false);
            return;
        }
        AplicadorDeSintomasVisuais aplicador = managerPacientes.pacienteAtualObject.GetComponent<AplicadorDeSintomasVisuais>();
        if (aplicador == null)
        {
            popupManager.ShowPersistentFloatingFeedback("Nenhum sintoma visual aplicável", GetPatientPosition(), false);
            return;
        }

        var tabelaResultados = new Dictionary<string, bool>();
        if (aplicador.SintomasVisuaisPossiveis.Count == 0)
        {
            tabelaResultados.Add("Nenhum sintoma visual possível", false);
        }
        else
        {
            foreach (var sintomaPossivel in aplicador.SintomasVisuaisPossiveis)
            {
                string nomeFormatado = System.Text.RegularExpressions.Regex.Replace(sintomaPossivel.ToString(), "(\\B[A-Z])", " $1");
                bool apareceu = aplicador.SintomaVisualAplicado.HasValue && aplicador.SintomaVisualAplicado.Value == sintomaPossivel;
                tabelaResultados.Add(nomeFormatado, apareceu);
            }
        }
        popupManager.ShowInfoPopup("A Estrela Revela", tabelaResultados);
    }

    // --- MÉTODOS AUXILIARES ---

    void VerificarSintoma(string sintomaKey, string nomeSintoma)
    {
        Vector3 feedbackPosition = GetPatientPosition();
        if (managerPacientes?.doencaAtual == null)
        {
            popupManager?.ShowPersistentFloatingFeedback("Paciente não encontrado", feedbackPosition, false);
            return;
        }

        bool temSintoma = false;
        switch (sintomaKey.ToLower())
        {
            case "febre": temSintoma = managerPacientes.doencaAtual.febre; break;
            case "tosse": temSintoma = managerPacientes.doencaAtual.tosse; break;
            case "nausea": temSintoma = managerPacientes.doencaAtual.nauseaOuVomito; break;
            case "calafrio": temSintoma = managerPacientes.doencaAtual.calafrio; break;
            case "dordecabeca": temSintoma = managerPacientes.doencaAtual.dorDeCabeca; break;
            default:
                Debug.LogError($"Sintoma '{sintomaKey}' desconhecido!");
                return;
        }

        string mensagem = temSintoma ? $"O paciente tem {nomeSintoma}" : $"O paciente não tem {nomeSintoma}";
        Debug.Log($"🏥 Verificando {nomeSintoma.ToUpper()}: {(temSintoma ? "✅ SIM" : "❌ NÃO")}");
        popupManager?.ShowPersistentFloatingFeedback(mensagem, feedbackPosition, temSintoma);
    }

    private Vector3 GetPatientPosition()
    {
        if (managerPacientes != null && managerPacientes.pacienteAtualObject != null)
        {
            return managerPacientes.pacienteAtualObject.transform.position;
        }
        return Vector3.zero;
    }

    private void HandlePopupManagerNotFound()
    {
        Debug.LogError("DynamicPopupManager não encontrado! Não é possível mostrar o pop-up.");
    }
}