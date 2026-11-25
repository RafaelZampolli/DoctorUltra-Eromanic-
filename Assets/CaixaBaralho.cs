using UnityEngine;

public class CaixaDeBaralho : MonoBehaviour
{
    /// <summary>
    /// Propriedade estática para que qualquer script no jogo possa verificar se o amuleto está ativo.
    /// Fica 'true' automaticamente quando este objeto é ativado na cena.
    /// </summary>
    public static bool IsActive { get; private set; }

    /// <summary>
    /// Define o percentual de desconto. 0.15f = 15% de desconto.
    /// 'readonly' significa que só pode ser definido aqui.
    /// </summary>
    private static readonly float PercentualDesconto = 0.15f;

    // Método mágico da Unity: é chamado automaticamente quando o GameObject é ativado.
    private void OnEnable()
    {
        IsActive = true;
        Debug.Log($"[Caixa de Baralho] Amuleto ATIVADO. Desconto de {PercentualDesconto * 100}% aplicado.");
    }

    // Método mágico da Unity: é chamado automaticamente quando o GameObject é desativado.
    private void OnDisable()
    {
        IsActive = false;
        Debug.Log("[Caixa de Baralho] Amuleto desativado.");
    }

    /// <summary>
    /// Uma função de ajuda (helper) que qualquer script pode chamar para calcular o preço final.
    /// </summary>
    /// <param name="precoOriginal">O preço original da carta, vindo do CartaData.</param>
    /// <returns>O novo preço com desconto, ou o preço original se o amuleto estiver inativo.</returns>
    public static int CalcularPrecoComDesconto(int precoOriginal)
    {
        // Se o amuleto não estiver ativo, retorna o preço original sem fazer nada.
        if (!IsActive)
        {
            return precoOriginal;
        }

        // Se estiver ativo, calcula o desconto.
        int valorDoDesconto = Mathf.FloorToInt(precoOriginal * PercentualDesconto);
        int precoFinal = precoOriginal - valorDoDesconto;

        // Garante que o preço nunca seja menor que 0.
        return Mathf.Max(0, precoFinal);
    }
}