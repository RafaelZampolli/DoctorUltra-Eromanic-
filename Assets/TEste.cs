using UnityEngine;

public class ManagerCartas : MonoBehaviour
{
    // Singleton para fácil acesso
    public static ManagerCartas Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Método principal para executar efeitos de uma carta
    public void ExecutarEfeitoCarta(CartaData cartaData)
    {
        if (cartaData == null)
        {
            Debug.LogError("CartaData é null!");
            return;
        }

        

    }
}