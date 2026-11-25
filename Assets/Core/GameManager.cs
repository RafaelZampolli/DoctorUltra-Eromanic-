using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public enum EstadoJogo
{
    Menu,
    Tutorial,
    Jogo,
    Interludio,
    GameOver,
    Pausa
}

public class GameManager : MonoBehaviour
{
    [Header("=== ESTADO DO JOGO ===")]
    public EstadoJogo estadoAtual = EstadoJogo.Menu;

    [Header("=== CONFIGURAÇÕES ===")]
    [SerializeField] private bool tutorialAtivo = true;
    [SerializeField] private bool debugMode = true;

    public static GameManager Instance { get; private set; }

    public static event Action<EstadoJogo> OnEstadoChanged;
    public static event Action OnGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        Debug.Log("[GameManager] Inicializado!");
        MudarEstado(EstadoJogo.Menu);
    }

    private void OnSceneLoaded(Scene cena, LoadSceneMode modo)
    {
        // NÃO resete mais pontos ou culpa aqui!
        Debug.Log("[GameManager] Cena carregada: " + cena.name);
    }

    public void MudarEstado(EstadoJogo novoEstado)
    {
        EstadoJogo estadoAnterior = estadoAtual;
        estadoAtual = novoEstado;
        OnEstadoChanged?.Invoke(novoEstado);
        if (debugMode) Debug.Log($"[GameManager] Estado: {estadoAnterior} → {novoEstado}");
    }

    public void TriggerGameOver()
    {
        MudarEstado(EstadoJogo.GameOver);
        OnGameOver?.Invoke();
        Debug.Log($"[GameManager] GAME OVER!");
    }

    private void Update()
    {
        // Remover atalhos que mexem em pontuação/culpa se existem.
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}