using UnityEngine;

/// <summary>
/// Bridge script que fica no próprio prefab da carta e chama o CartasEffectManager
/// Resolve o problema de referência perdida em prefabs
/// </summary>
public class CartaCallerBridge : MonoBehaviour
{
    [Header("Configuração da Carta")]
    [Tooltip("Tipo de efeito que esta carta executa")]
    public TipoEfeitoCarta tipoEfeito = TipoEfeitoCarta.Termometro;

    // Enum atualizado com todas as cartas novas
    public enum TipoEfeitoCarta
    {
        // Itens e Cartas Antigas
        Termometro,
        Estetoscopio,
        Anamnese,
        ExameGeral,
        Pilula,
        PrimeirosSocorros,
        Antiremetico,
        EstetoscopioAvancado,
        AForca,
        AEstrela,

        // --- Novas Cartas de Tarot ---
        ARodaDaFortuna,
        OTolo,
        OMundo,
        OSol,
        AJustica,
        ATemperanca,
        PalpiteMedico,
        ExperienciaMedica
    }

    // Este método pode ser chamado pelo UnityEvent no botão da carta
    public void ExecutarEfeito()
    {
        var effectManager = CartasEffectManager.Instance;
        if (effectManager == null)
        {
            effectManager = FindObjectOfType<CartasEffectManager>();
        }

        if (effectManager == null)
        {
            Debug.LogError("FATAL: CartasEffectManager não foi encontrado na cena! Não é possível executar o efeito.");
            return;
        }

        // Switch atualizado com as chamadas para os novos efeitos
        switch (tipoEfeito)
        {
            // --- Itens e Cartas Antigas ---
            case TipoEfeitoCarta.Termometro:
                effectManager.UsarTermometro();
                break;
            case TipoEfeitoCarta.Estetoscopio:
                effectManager.UsarEstetoscopio();
                break;
            case TipoEfeitoCarta.Anamnese:
                effectManager.UsarAnamnese();
                break;
            case TipoEfeitoCarta.ExameGeral:
                effectManager.UsarExameGeral();
                break;
            case TipoEfeitoCarta.Pilula:
                effectManager.UsarPilula();
                break;
            case TipoEfeitoCarta.PrimeirosSocorros:
                effectManager.UsarPrimeirosSocorros();
                break;
            case TipoEfeitoCarta.Antiremetico:
                effectManager.UsarAntiremetico();
                break;
            case TipoEfeitoCarta.EstetoscopioAvancado:
                effectManager.UsarEstetoscopioAvancado();
                break;
            case TipoEfeitoCarta.AForca:
                effectManager.UsarAForca();
                break;
            case TipoEfeitoCarta.AEstrela:
                effectManager.UsarAEstrela();
                break;

            // --- Novas Cartas de Tarot ---
            case TipoEfeitoCarta.ARodaDaFortuna:
                effectManager.UsarARodaDaFortuna();
                break;
            case TipoEfeitoCarta.OTolo:
                effectManager.UsarOTolo();
                break;
            case TipoEfeitoCarta.OMundo:
                effectManager.UsarOMundo();
                break;
            case TipoEfeitoCarta.OSol:
                effectManager.UsarOSol();
                break;
            case TipoEfeitoCarta.AJustica:
                effectManager.UsarAJustica();
                break;
            case TipoEfeitoCarta.ATemperanca:
                effectManager.UsarATemperanca();
                break;
            case TipoEfeitoCarta.PalpiteMedico:
                effectManager.UsarPalpiteMedico();
                break;
            case TipoEfeitoCarta.ExperienciaMedica:
                effectManager.UsarExperienciaMedica();
                break;
        }

        Debug.Log($"Efeito {tipoEfeito} executado via bridge!");
    }
}