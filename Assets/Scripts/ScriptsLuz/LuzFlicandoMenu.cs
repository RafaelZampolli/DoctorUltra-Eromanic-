using UnityEngine;

public class LuzFlicandoMenu : MonoBehaviour
{
   [Header("Luzes para simular falha")]
    [SerializeField] private Light luz1;
    [SerializeField] private Light luz2;
    [SerializeField] private Light luz3;

    [Header("Intervalo entre falhas (segundos)")]
    [SerializeField] private float minIntervalo = 2f;
    [SerializeField] private float maxIntervalo = 8f;

    [Header("Configuração do piscar")]
    [SerializeField] private int minPiscar = 2;
    [SerializeField] private int maxPiscar = 6;
    [SerializeField] private float minDuracaoPiscar = 0.05f;
    [SerializeField] private float maxDuracaoPiscar = 0.2f;

    private float tempoProximaFalhaLuz1;
    private float tempoProximaFalhaLuz2;
    private float tempoProximaFalhaLuz3;

    void Start()
    {
        if (luz1 == null || luz2 == null || luz3 == null)
        {
            Debug.LogWarning("Atribua as três luzes ao script FlickeringLights!");
            enabled = false;
            return;
        }
        tempoProximaFalhaLuz1 = Time.time + Random.Range(minIntervalo, maxIntervalo);
        tempoProximaFalhaLuz2 = Time.time + Random.Range(minIntervalo, maxIntervalo);
        tempoProximaFalhaLuz3 = Time.time + Random.Range(minIntervalo, maxIntervalo);
    }

    void Update()
    {
        if (Time.time >= tempoProximaFalhaLuz1)
        {
            StartCoroutine(FalharLuz(luz1));
            tempoProximaFalhaLuz1 = Time.time + Random.Range(minIntervalo, maxIntervalo);
        }

        if (Time.time >= tempoProximaFalhaLuz2)
        {
            StartCoroutine(FalharLuz(luz2));
            tempoProximaFalhaLuz2 = Time.time + Random.Range(minIntervalo, maxIntervalo);
        }

        if (Time.time >= tempoProximaFalhaLuz3)
        {
            StartCoroutine(FalharLuz(luz3));
            tempoProximaFalhaLuz3 = Time.time + Random.Range(minIntervalo, maxIntervalo);
        }
    }

    private System.Collections.IEnumerator FalharLuz(Light luzAlvo)
    {
        int quantidadePiscar = Random.Range(minPiscar, maxPiscar + 1);

        for (int i = 0; i < quantidadePiscar; i++)
        {
            luzAlvo.enabled = false;
            yield return new WaitForSeconds(Random.Range(minDuracaoPiscar, maxDuracaoPiscar));
            luzAlvo.enabled = true;
            yield return new WaitForSeconds(Random.Range(minDuracaoPiscar, maxDuracaoPiscar));
        }
    }
}
