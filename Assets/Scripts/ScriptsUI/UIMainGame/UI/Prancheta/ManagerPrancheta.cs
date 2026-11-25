using UnityEngine;
using System.Collections.Generic; // Precisamos disso para usar List<T>

public class ManagerPrancheta : MonoBehaviour
{
    private bool clicavel = false;

    public GameObject Prancheta;
    public GameObject PranchetaPequena;

    public SubidouroUniversal ScriptMouse;

    // --- MODIFICADO: Usamos uma Lista para guardar as cartas encontradas ---
    private List<GameObject> listaDeCartas;

    private void Start()
    {
        if (ScriptMouse == null)
        {
            ScriptMouse = GetComponent<SubidouroUniversal>();
        }

        Prancheta.SetActive(false);

        // Inicializamos a lista, mas a deixamos vazia.
        listaDeCartas = new List<GameObject>();
    }

    public void Update()
    {
        MouseEmcimas();
        chamarPrancheta();
    }

    private void chamarPrancheta()
    {
        if (clicavel && Input.GetKeyDown(KeyCode.Mouse0))
        {
            AbrirPrancheta();
        }
    }

    private void MouseEmcimas()
    {
        if (ScriptMouse != null)
        {
            clicavel = ScriptMouse.mouseEmCima;
        }
    }

    public void AbrirPrancheta()
    {
        Prancheta.SetActive(true);
        PranchetaPequena.SetActive(false);

        // --- LÓGICA ATUALIZADA ---
        // 1. Limpa a lista antiga para garantir que estamos sempre com as cartas mais recentes.
        listaDeCartas.Clear();

        // 2. Encontra todas as cartas ATIVAS na cena e as adiciona à nossa lista.
        foreach (var carta in GameObject.FindGameObjectsWithTag("carta"))
        {
            listaDeCartas.Add(carta);
        }

        Debug.Log($"[DEBUG] Abrindo prancheta. Encontradas e armazenadas {listaDeCartas.Count} cartas.");

        // 3. Desativa cada carta da nossa lista recém-criada.
        foreach (var carta in listaDeCartas)
        {
            if (carta != null)
            {
                carta.SetActive(false);
            }
        }
    }

    public void FecharPrancheta()
    {
        Prancheta.SetActive(false);
        PranchetaPequena.SetActive(true);

        // --- LÓGICA ATUALIZADA ---
        // Itera pela nossa lista de cartas que guardamos e as reativa.
        Debug.Log($"[DEBUG] Fechando prancheta. Reativando {listaDeCartas.Count} cartas da memória.");

        foreach (var carta in listaDeCartas)
        {
            // Verificação de segurança, caso uma carta tenha sido destruída enquanto a prancheta estava aberta.
            if (carta != null)
            {
                carta.SetActive(true);
            }
        }
    }
}


