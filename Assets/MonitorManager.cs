using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class MonitorManager : MonoBehaviour
{
    [Header("Referências de UI (Arrastar do Canvas)")]
    public TextMeshProUGUI doacoesText;
    public TextMeshProUGUI culpaText;
    public TextMeshProUGUI pacientesCountText;
    public TextMeshProUGUI turnoText; // NOVO: Arraste o "TextoTurno_Monitor" aqui
    public Image setaFeedbackImage;

    [Header("Sprites de Feedback (Arrastar da pasta)")]
    public Sprite setaCimaSprite;
    public Sprite setaBaixoSprite;

    [Header("Referências dos Managers (Arrastar da Cena)")]
    public ManagerPontos managerPontos; // managerPontos contém a variável "Rodada"
    public ManagerTurno managerTurno;

    private int doacoesExibidas;
    private int culpaExibida;
    private int pacientesExibidos;
    private int turnoExibido; // NOVO: Variável para guardar o turno atual
    private Coroutine animacaoDoacoesCoroutine;

    void Start()
    {
        if (managerPontos == null) managerPontos = FindObjectOfType<ManagerPontos>();
        if (managerTurno == null) managerTurno = FindObjectOfType<ManagerTurno>();

        setaFeedbackImage.gameObject.SetActive(false);

        AtualizarUI(true);
    }

    void Update()
    {
        // Lógica de doações (sem alteração)
        if (managerPontos.Doacoes != doacoesExibidas && animacaoDoacoesCoroutine == null)
        {
            animacaoDoacoesCoroutine = StartCoroutine(AnimarDoacoes(managerPontos.Doacoes));
        }

        // Lógica de culpa (sem alteração)
        if (managerPontos.Culpa != culpaExibida)
        {
            AtualizarCulpa(managerPontos.Culpa);
        }

        // Lógica de pacientes (sem alteração)
        int pacientesRestantes = Mathf.Max(0, managerTurno.PacientesTotal - managerTurno.PacientesAtual);
        if (pacientesRestantes != pacientesExibidos)
        {
            AtualizarContadorPacientes(pacientesRestantes);
        }

        // NOVO: Lógica para atualizar o contador de turnos
        if (managerPontos.Rodada != turnoExibido)
        {
            AtualizarTurno(managerPontos.Rodada);
        }
    }

    public void AtualizarUI(bool forcar = false)
    {
        // Doações
        doacoesExibidas = managerPontos.Doacoes;
        doacoesText.text = $"{doacoesExibidas}$";

        // Culpa
        culpaExibida = managerPontos.Culpa;
        culpaText.text = $"CULPA: {culpaExibida}";

        // Pacientes
        int pacientesRestantes = Mathf.Max(0, managerTurno.PacientesTotal - managerTurno.PacientesAtual);
        pacientesExibidos = pacientesRestantes;
        pacientesCountText.text = $"x{pacientesExibidos}";

        // NOVO: Atualização inicial do turno
        turnoExibido = managerPontos.Rodada;
        turnoText.text = turnoExibido.ToString();
    }

    // ... (Método AnimarDoacoes sem alterações) ...
    private IEnumerator AnimarDoacoes(int valorFinal)
    {
        int valorInicial = doacoesExibidas;
        doacoesExibidas = valorFinal;
        float duracaoAnimacao = 0.5f;

        if (valorFinal > valorInicial)
        {
            doacoesText.color = Color.green;
            setaFeedbackImage.sprite = setaCimaSprite;
            setaFeedbackImage.color = Color.green;
            doacoesText.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0), duracaoAnimacao, 10, 1);
        }
        else
        {
            doacoesText.color = Color.red;
            setaFeedbackImage.sprite = setaBaixoSprite;
            setaFeedbackImage.color = Color.red;
            doacoesText.transform.DOPunchRotation(new Vector3(0, 0, 15), duracaoAnimacao, 10, 1);
        }

        setaFeedbackImage.gameObject.SetActive(true);
        setaFeedbackImage.color = new Color(setaFeedbackImage.color.r, setaFeedbackImage.color.g, setaFeedbackImage.color.b, 0);
        setaFeedbackImage.DOFade(1, 0.2f);

        float valorCorrendo = valorInicial;
        DOTween.To(() => valorCorrendo, x => valorCorrendo = x, valorFinal, duracaoAnimacao)
            .OnUpdate(() => {
                doacoesText.text = $"{Mathf.FloorToInt(valorCorrendo)}$";
            });

        yield return new WaitForSeconds(duracaoAnimacao);

        doacoesText.text = $"{valorFinal}$";
        doacoesText.DOColor(Color.white, 0.3f);
        setaFeedbackImage.DOFade(0, 0.3f).OnComplete(() => setaFeedbackImage.gameObject.SetActive(false));

        animacaoDoacoesCoroutine = null;
    }


    // ... (Método AtualizarCulpa sem alterações) ...
    private void AtualizarCulpa(int novoValor)
    {
        int valorAntigo = culpaExibida;
        culpaExibida = novoValor;

        culpaText.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.4f, 10, 1);

        float valorCorrendo = valorAntigo;
        DOTween.To(() => valorCorrendo, x => valorCorrendo = x, novoValor, 0.4f)
            .OnUpdate(() => {
                culpaText.text = $"CULPA: {Mathf.FloorToInt(valorCorrendo)}";
            });
    }

    // ... (Método AtualizarContadorPacientes sem alterações) ...
    private void AtualizarContadorPacientes(int valorFinal)
    {
        int valorInicial = pacientesExibidos;
        pacientesExibidos = valorFinal;

        pacientesCountText.transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0), 0.4f, 10, 1);

        float valorCorrendo = valorInicial;
        DOTween.To(() => valorCorrendo, x => valorCorrendo = x, valorFinal, 0.4f)
            .OnUpdate(() => {
                pacientesCountText.text = $"x{Mathf.FloorToInt(valorCorrendo)}";
            });
    }

    // NOVO: Método para animar a mudança do número do turno
    private void AtualizarTurno(int novoTurno)
    {
        turnoExibido = novoTurno;

        // Animação de "pulso" para dar destaque à mudança
        turnoText.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0), 0.5f, 10, 1);

        // Apenas atualiza o texto, já que é só um número
        turnoText.text = novoTurno.ToString();
    }
}