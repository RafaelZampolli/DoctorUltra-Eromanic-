using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ManagerPacientes : MonoBehaviour
{
    [Header("Banco de Dados")]
    [Tooltip("Lista de TODAS as doenças e ações possíveis que um paciente pode ter.")]
    public List<Doenca> todasAsDoencas;

    [Header("VISUAL DO PACIENTE")]
    [Tooltip("A ÚNICA LISTA DE PREFABS. Arraste para cá TODAS as aparências possíveis do paciente.")]
    public List<GameObject> todasAsAparenciasDePaciente;

    [Header("Referências de Cena")]
    [Tooltip("O local na cena onde o paciente será criado.")]
    public Transform pontoDeSpawnPaciente;

    [Header("Estado Atual (Debug)")]
    [SerializeField] private Doenca _doencaAtual;
    public GameObject pacienteAtualObject;
    public Doenca doencaAtual => _doencaAtual;
    public static ManagerPacientes Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Random.InitState((int)System.DateTime.Now.Ticks);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void DecidirDoenca()
    {
        if (todasAsDoencas.Count == 0 || todasAsAparenciasDePaciente.Count == 0)
        {
            Debug.LogError("A lista 'todasAsDoencas' ou 'todasAsAparenciasDePaciente' está vazia!", this);
            return;
        }

        _doencaAtual = EscolherDoencaPonderada();

        GameObject prefabVisualEscolhido = todasAsAparenciasDePaciente[Random.Range(0, todasAsAparenciasDePaciente.Count)];
        Vector3 spawnPosition = pontoDeSpawnPaciente != null ? pontoDeSpawnPaciente.position : Vector3.zero;
        Quaternion spawnRotation = pontoDeSpawnPaciente != null ? pontoDeSpawnPaciente.rotation : Quaternion.identity;
        pacienteAtualObject = Instantiate(prefabVisualEscolhido, spawnPosition, spawnRotation);

        Debug.Log($"DOENÇA SORTEADA: {_doencaAtual.name}. APARÊNCIA SPAWNADA: {pacienteAtualObject.name}");
    }

    private Doenca EscolherDoencaPonderada()
    {
        float pesoTotal = todasAsDoencas.Sum(d => d.chanceAparicao);
        float valorSorteado = Random.Range(0, pesoTotal);

        float pesoAcumulado = 0f;
        foreach (var doenca in todasAsDoencas)
        {
            pesoAcumulado += doenca.chanceAparicao;
            if (valorSorteado <= pesoAcumulado)
            {
                return doenca;
            }
        }
        return todasAsDoencas[todasAsDoencas.Count - 1];
    }

    public void RemoverPacienteAtual(float delay = 0f)
    {
        if (pacienteAtualObject != null)
        {
            Destroy(pacienteAtualObject, delay);
            pacienteAtualObject = null;
        }
    }
}