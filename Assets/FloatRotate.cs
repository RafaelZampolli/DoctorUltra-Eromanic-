using UnityEngine;

public class FloatingRotation : MonoBehaviour
{
    [Header("Configurações de Rotação")]
    [Tooltip("A velocidade com que o objeto gira de um lado para o outro.")]
    [SerializeField] private float velocidadeDeRotacao = 1f;

    [Tooltip("O ângulo máximo, em graus, que o objeto irá girar em cada direção.")]
    [SerializeField] private float anguloDeRotacao = 15f;

    // Armazena a rotação inicial para evitar conflitos com a rotação do editor.
    private Quaternion rotacaoInicial;

    void Start()
    {
        // Salva a rotação com a qual o objeto começa na cena.
        rotacaoInicial = transform.rotation;
    }

    void Update()
    {
        // Define a layer como 10 sempre
        gameObject.layer = 10;

        // Calcula o ângulo de rotação atual usando uma onda senoidal.
        // Time.time faz o valor mudar suavemente ao longo do tempo.
        float anguloFlutuante = Mathf.Sin(Time.time * velocidadeDeRotacao) * anguloDeRotacao;

        // Aplica a rotação flutuante à rotação inicial do objeto.
        // Isso garante que o objeto flutue em torno de sua orientação original.
        transform.rotation = rotacaoInicial * Quaternion.Euler(0f, anguloFlutuante, 0f);
    }
}

