using UnityEngine;
using System.Collections.Generic;

// Estes enums precisam estar acessíveis
public enum TipoDeCura
{
    Malaria,
    Sarampo,
    Colera,
    Pneumonia
}

public enum TipoSintomaVisual
{
    Nenhum,
    MagrezaExtrema,
    ManchasNaPele,
    QuedaDeCabelo,
    Sudorese,
    Cansaco,
    DorNoPeito,
    FraturaExposta // Importante para a nova mecânica
}

[CreateAssetMenu(fileName = "NovaDoenca", menuName = "Doenca/DoencaBase")]
public class Doenca : ScriptableObject
{
    [Header("Informações da Doença")]
    public string nome;
    [TextArea] public string descricao;
    public TipoDeCura tipoCura;

    [Header("Chance de Aparecer")]
    [Range(0f, 100f)] public float chanceAparicao = 25f;

    [Header("Configuração de Tempo")]
    public bool temTempoLimite;
    public float tempoLimite;

    [Header("Sintomas Visuais")]
    public bool magrezaExtrema;
    public bool manchasNaPele;
    public bool quedaDeCabelo;
    public bool sudorese;
    public bool cansaco;
    public bool dorNoPeito;
    public bool fraturaExposta; // Sintoma visual específico da fratura

    [Header("Sintomas de Carta")]
    public bool dorDeCabeca;
    public bool tosse;
    public bool nauseaOuVomito;
    public bool febre;
    public bool calafrio;
    public bool ProblemasRespiratorios;

    [Header("Somente Diagnóstico")]
    [Tooltip("Marque se for uma AÇÃO com minigame (ex: fratura) e não uma doença curada pela prancheta.")]
    public bool eAcao;
}