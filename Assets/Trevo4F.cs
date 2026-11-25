using UnityEngine;

public class TrevoDe4Folhas : MonoBehaviour
{
    // Propriedade global para que qualquer script possa verificar se o item está ativo
    public static bool IsActive { get; private set; }

    [Header("Configuração do Item")]
    [Tooltip("A chance (de 0 a 100) de não consumir a carta ao usá-la.")]
    [Range(0, 100)]
    public int chanceDeSalvar = 25; // Ex: 25% de chance

    void Awake()
    {
        // Ao iniciar, define o estado global com base no que foi configurado no Inspector
        // Você pode chamar AtivarTrevo() ou DesativarTrevo() de outros scripts para mudar isso durante o jogo
        if (gameObject.activeInHierarchy)
        {
            AtivarTrevo(chanceDeSalvar);
        }
        else
        {
            DesativarTrevo();
        }
    }

    /// <summary>
    /// Ativa o efeito do trevo com uma chance específica.
    /// </summary>
    public static void AtivarTrevo(int chance)
    {
        IsActive = true;
        Debug.Log($"?? Trevo de 4 Folhas ATIVADO! Chance de salvar: {chance}%.");
    }

    /// <summary>
    /// Desativa o efeito do trevo.
    /// </summary>
    public static void DesativarTrevo()
    {
        IsActive = false;
        Debug.Log("?? Trevo de 4 Folhas desativado.");
    }

    /// <summary>
    /// Rola os dados para ver se a carta será salva.
    /// </summary>
    /// <returns>True se a carta foi salva, False se ela deve ser consumida.</returns>
    public static bool TentarSalvarCarta()
    {
        if (!IsActive) return false;

        // A chance é lida do script que controla o item, aqui assumimos que é 25%
        // Você pode tornar essa chance estática também se preferir.
        var trevoInstance = FindObjectOfType<TrevoDe4Folhas>();
        int chance = trevoInstance != null ? trevoInstance.chanceDeSalvar : 25;

        int rolagem = Random.Range(0, 100);
        bool salvou = rolagem < chance;

        if (salvou)
        {
            Debug.Log($"[Trevo] SUCESSO! Rolagem {rolagem} < {chance}. A carta será reposta.");
        }
        else
        {
            Debug.Log($"[Trevo] Falha. Rolagem {rolagem} >= {chance}. A carta será consumida.");
        }

        return salvou;
    }
}