using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControladorUI : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject painelConfig;
    public GameObject painelCreditos;
    
    [Header("Menu Principal")]
    public GameObject menuPrincipal;
    public GameObject logo;

    public void JogarBotao()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void OpcoesBotao()
    {
        Debug.Log("Abrindo opções...");
        menuPrincipal.SetActive(false);
        logo.SetActive(false);
        painelConfig.SetActive(true);
    }

    public void BotaoFechar()
    {
        Debug.Log("Fechando opções...");
        painelConfig.SetActive(false);
        menuPrincipal.SetActive(true);
        logo.SetActive(true);
    }

    public void BotaoCreditos()
    {
        Debug.Log("Abrindo créditos...");
        menuPrincipal.SetActive(false);
        logo.SetActive(false);
        painelCreditos.SetActive(true);
    }
    
    public void BotaoFecharCreditos()
    {
        Debug.Log("Fechando créditos - INÍCIO");
        Debug.Log("PainelCreditos ativo: " + painelCreditos.activeInHierarchy);
        Debug.Log("MenuPrincipal ativo: " + menuPrincipal.activeInHierarchy);
        Debug.Log("Logo ativo: " + logo.activeInHierarchy);
        
        painelCreditos.SetActive(false);
        menuPrincipal.SetActive(true);
        logo.SetActive(true);
        
        Debug.Log("Fechando créditos - FINAL");
        Debug.Log("PainelCreditos ativo: " + painelCreditos.activeInHierarchy);
        Debug.Log("MenuPrincipal ativo: " + menuPrincipal.activeInHierarchy);
        Debug.Log("Logo ativo: " + logo.activeInHierarchy);
    }

    public void AbrirSite(string url = "https://doe.msf.org.br/")
    {
        Application.OpenURL(url);
    }

    public void FecharJogo()
    {
        Application.Quit();
    }
}