using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class AmuletoManager : MonoBehaviour
{
    [Header("Configuração")]
    public ManagerPontos Dinheiro;
    public GameObject Totem;
    public int Preco;

    [Header("Descrição do Amuleto")]
    public string nomeAmuleto;
    [TextArea(3, 5)]
    public string descricaoAmuleto;

    private bool jaComprado = false;

    public void Comprado()
    {
        if (jaComprado || Dinheiro == null) return;

        if (Dinheiro.Doacoes >= Preco)
        {
            Dinheiro.Doacoes -= Preco;
            if (Totem != null) Totem.SetActive(true);
            jaComprado = true;
            gameObject.SetActive(false); // Opcional: some da loja após comprar
        }
        else
        {
            Debug.Log($"Dinheiro insuficiente para {nomeAmuleto}!");
        }
    }

   
}