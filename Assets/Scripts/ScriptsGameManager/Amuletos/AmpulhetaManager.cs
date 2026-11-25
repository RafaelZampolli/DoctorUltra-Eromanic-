using UnityEngine;

public class AmpulhetaManager : MonoBehaviour
{
    public float ModficadorAmpulheta = 0f;
    public float Porcentagem;

    private void Start()
    {
        ModficadorAmpulheta = 0f;
    }

    private void OnEnable()
    {
        ModficadorAmpulheta = Porcentagem;
    }

    private void OnDisable()
    {
        ModficadorAmpulheta = 0f;
    }
}