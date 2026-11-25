using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using DG.Tweening;

[CreateAssetMenu(fileName = "Nova Carta", menuName = "Cartas/Carta")]
public class CartaData : ScriptableObject
{
    [Header("Informações da Carta")]
    public string nomeCarta;
    [TextArea(3, 5)]
    public string descricao;
    public int preco;

    [Header("Visual")]
    public Sprite imagemCarta;
    public Color corCarta = Color.white;

    [Header("Audio (opcional)")]
    public AudioClip somHover;
    public AudioClip somClick;
}