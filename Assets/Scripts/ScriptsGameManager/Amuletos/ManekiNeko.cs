
using UnityEngine;

public class ManekiNeko : MonoBehaviour
{
    public float ModficadorDoacoes = 0f;
    public float Porcentagem;

    private void Start()
    {
        ModficadorDoacoes = 0f;
    }

    private void OnEnable()
    {
        ModficadorDoacoes = Porcentagem;
    }

    private void OnDisable()
    {
        ModficadorDoacoes = 0f;
    }
}