using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class BolaDeCristalManager : MonoBehaviour
{
    // Usamos um singleton para que outros scripts possam encontrá-lo facilmente.
    public static BolaDeCristalManager Instance { get; private set; }

    private SistemaDeducaoPrancheta prancheta;
    private ManagerPacientes managerPacientes;

    void Awake()
    {
        // Configuração do Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Encontra as referências necessárias na cena.
        // Fazemos isso no Start para garantir que os outros managers já existam.
        prancheta = FindObjectOfType<SistemaDeducaoPrancheta>();
        managerPacientes = FindObjectOfType<ManagerPacientes>();

        if (prancheta == null) Debug.LogError("BolaDeCristal: SistemaDeducaoPrancheta não encontrado!");
        if (managerPacientes == null) Debug.LogError("BolaDeCristal: ManagerPacientes não encontrado!");
    }

    /// <summary>
    /// O método principal que é chamado pelo ManagerTurno.
    /// </summary>
    public void PreverEEliminarDoenca()
    {
        if (prancheta == null || managerPacientes == null || managerPacientes.doencaAtual == null)
        {
            Debug.LogWarning("Bola de Cristal não pôde agir (sem prancheta ou paciente).");
            return;
        }

        // 1. Descobrir qual é a doença correta do paciente atual.
        string doencaCorreta = prancheta.MapearNomeDoenca(managerPacientes.doencaAtual.nome);

        // 2. Criar uma lista de doenças que estão na prancheta, NÃO estão riscadas e NÃO são a doença correta.
        var candidatasAEliminar = prancheta.estadosDoencas
            .Where(kvp => 
                kvp.Key != doencaCorreta && // Não é a doença correta
                kvp.Value != 2)             // Não está riscada (estado 2)
            .Select(kvp => kvp.Key)
            .ToList();

        // 3. Se houver alguma doença que possa ser eliminada...
        if (candidatasAEliminar.Count > 0)
        {
            // 4. Escolhe uma delas aleatoriamente.
            int indiceAleatorio = Random.Range(0, candidatasAEliminar.Count);
            string doencaParaEliminar = candidatasAEliminar[indiceAleatorio];

            // 5. Risca a doença na prancheta.
            prancheta.AtualizarSpriteDoenca(doencaParaEliminar, 2); // Estado 2 = Riscado
            prancheta.estadosDoencas[doencaParaEliminar] = 2;

            Debug.Log($"🔮 Bola de Cristal previu e eliminou a suspeita de: {doencaParaEliminar}");

            // Opcional: Adicionar um feedback visual
            DynamicPopupManager.Instance?.ShowPersistentFloatingFeedback($"Suspeita de '{doencaParaEliminar}' eliminada!", Vector3.zero, true);
        }
        else
        {
            Debug.Log("🔮 Bola de Cristal não encontrou nenhuma doença incorreta para eliminar.");
        }
    }
}