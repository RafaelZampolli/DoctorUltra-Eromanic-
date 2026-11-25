using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider2D))]
public class SubidouroUniversal : MonoBehaviour
{
    public float SubirQuantia = 0.6f;
    private Vector3 posicaoOriginal;
    public bool mouseEmCima = false;

    void Start()
    {
        posicaoOriginal = transform.position;
    }

    private void OnMouseEnter()
    {
        if (!mouseEmCima)
        {
            
            transform.DOMove(posicaoOriginal + Vector3.up * SubirQuantia, 0.2f);
            mouseEmCima = true;
        }
    }

    private void OnMouseExit()
    {
        if (mouseEmCima)
        {
            transform.DOMove(posicaoOriginal, 0.2f);
            mouseEmCima = false;
        }
    }
}
