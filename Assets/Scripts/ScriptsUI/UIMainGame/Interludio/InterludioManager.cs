using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class InterludioManager : MonoBehaviour
{
    [Header("Totens (Sistema Original)")]
    public List<GameObject> ListaTotem1;
    public List<GameObject> ListaTotem2;

    [Header("Cartas da Loja")]
    public List<GameObject> ListaCartas1;
    public List<GameObject> ListaCartas2;
    public List<GameObject> ListaCartas3;
    public List<GameObject> ListaCartas4;

    [Header("Slots das Cartas")]
    public Transform SlotCarta1;
    public Transform SlotCarta2;
    public Transform SlotCarta3;
    public Transform SlotCarta4;

    [Header("Referências")]
    public ManagerMao managerMao;
    public ManagerPontos managerPontos;

    public bool Interludio;

    [HideInInspector] public GameObject escolhidoTotem1;
    [HideInInspector] public GameObject escolhidoTotem2;

    [HideInInspector] public GameObject cartaAtual1;
    [HideInInspector] public GameObject cartaAtual2;
    [HideInInspector] public GameObject cartaAtual3;
    [HideInInspector] public GameObject cartaAtual4;

    public bool objetosJaEscolhidos = false;

    void Update()
    {
        if (Interludio && !objetosJaEscolhidos)
        {
            // Sistema original dos totens (sem mudanças)
            escolhidoTotem1 = EscolherUm(ListaTotem1);
            escolhidoTotem2 = EscolherUm(ListaTotem2);

            if (escolhidoTotem1 != null)
            {
                escolhidoTotem1.SetActive(true);
                Debug.Log("Totem1 escolhido: " + escolhidoTotem1.name);
            }

            if (escolhidoTotem2 != null)
            {
                escolhidoTotem2.SetActive(true);
                Debug.Log("Totem2 escolhido: " + escolhidoTotem2.name);
            }

            // Sistema novo das cartas
            SpawnarCartaNoSlot(ListaCartas1, SlotCarta1, ref cartaAtual1, 0.2f);
            SpawnarCartaNoSlot(ListaCartas2, SlotCarta2, ref cartaAtual2, 0.3f);
            SpawnarCartaNoSlot(ListaCartas3, SlotCarta3, ref cartaAtual3, 0.4f);
            SpawnarCartaNoSlot(ListaCartas4, SlotCarta4, ref cartaAtual4, 0.5f);

            objetosJaEscolhidos = true;
            Interludio = false;
        }
    }

    GameObject EscolherUm(List<GameObject> lista)
    {
        if (lista.Count == 0) return null;

        int index = Random.Range(0, lista.Count);
        return lista[index];
    }

    void SpawnarCartaNoSlot(List<GameObject> lista, Transform slot, ref GameObject cartaAtual, float delay = 0f)
    {
        if (lista.Count == 0 || slot == null) return;

        GameObject prefabEscolhido = EscolherUm(lista);
        if (prefabEscolhido == null) return;

        // Instancia o prefab da carta (que contém carta visual + botão)
        cartaAtual = Instantiate(prefabEscolhido, slot.position, slot.rotation, slot);

        // --- LINHA ADICIONADA ---
        // Move a carta para o final da hierarquia do slot, forçando-a a ser renderizada por cima.
        cartaAtual.transform.SetAsLastSibling();
        // -------------------------

        cartaAtual.SetActive(true);

        // Game juice na entrada com delay escalonado
        cartaAtual.transform.localScale = Vector3.zero;
        cartaAtual.transform.DOScale(prefabEscolhido.transform.localScale, 0.4f)
            .SetEase(Ease.OutBack)
            .SetDelay(delay);

        // Configura o ComportamentoCarta como item de loja
        ComportamentoCarta comportamento = cartaAtual.GetComponent<ComportamentoCarta>();
        if (comportamento != null)
        {
            comportamento.ConfigurarComoItemLoja(this, managerPontos, managerMao);
        }

        Debug.Log($"Carta spawnada: {prefabEscolhido.name} no slot {slot.name}");
    }

    // Método chamado quando uma carta é comprada (funciona igual ao CardShopManager)
    public void CartaComprada(GameObject cartaComprada)
    {
        Transform slotDaCarta = cartaComprada.transform.parent;

        // Game juice na saída
        Vector3 destino = managerMao != null && managerMao.SpawnCarta != null ? managerMao.SpawnCarta.position : Vector3.up * 2f;

        cartaComprada.transform.DOMove(destino, 0.3f)
            .SetEase(Ease.InQuart)
            .OnComplete(() =>
            {
                Destroy(cartaComprada);
                RespawnarCartaNoSlot(slotDaCarta);
            });
    }

    void RespawnarCartaNoSlot(Transform slot)
    {
        // Delay pra dar sensação de restock
        DOVirtual.DelayedCall(0.2f, () =>
        {
            if (slot == SlotCarta1)
                SpawnarCartaNoSlot(ListaCartas1, SlotCarta1, ref cartaAtual1);
            else if (slot == SlotCarta2)
                SpawnarCartaNoSlot(ListaCartas2, SlotCarta2, ref cartaAtual2);
            else if (slot == SlotCarta3)
                SpawnarCartaNoSlot(ListaCartas3, SlotCarta3, ref cartaAtual3);
            else if (slot == SlotCarta4)
                SpawnarCartaNoSlot(ListaCartas4, SlotCarta4, ref cartaAtual4);
        });
    }

    // Método para resetar o interlúdio (útil para testes)
    [ContextMenu("Reset Interlúdio")]
    public void ResetarInterludio()
    {
        objetosJaEscolhidos = false;

        // Desativa totens (sistema original)
        if (escolhidoTotem1 != null)
            escolhidoTotem1.SetActive(false);
        if (escolhidoTotem2 != null)
            escolhidoTotem2.SetActive(false);

        // Destroi cartas atuais
        if (cartaAtual1 != null) { Destroy(cartaAtual1); cartaAtual1 = null; }
        if (cartaAtual2 != null) { Destroy(cartaAtual2); cartaAtual2 = null; }
        if (cartaAtual3 != null) { Destroy(cartaAtual3); cartaAtual3 = null; }
        if (cartaAtual4 != null) { Destroy(cartaAtual4); cartaAtual4 = null; }
    }
}