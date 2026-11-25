using UnityEngine;
using UnityEngine.SceneManagement;

public class ManagerCena : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ChamarCenaMenu()
    {
        SceneManager.LoadScene("Menu");
    }
    public void ChamarCenaJogo()
    {
        SceneManager.LoadScene("Jogo");
    }

}
