using UnityEngine;

public class ControladorUIMainGame : MonoBehaviour
{
    public GameObject painelConfig;

    void Start()
    {

    }



   
    public void BotaoFechar()
    {
        painelConfig.SetActive(false);
    }

    public void FecharJogo()
    {
        Application.Quit();
    }
}
