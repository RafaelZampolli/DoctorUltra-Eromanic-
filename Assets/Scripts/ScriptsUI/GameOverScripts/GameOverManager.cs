using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Referências")]
    public GameObject gameOverPanel; // Seu painel atual
    public GameObject card3D; // O modelo 3D da carta
    public GameOverCard cardScript; // Referência ao script da carta
    
    [Header("Posicionamento da Carta")]
    public float distanceFromCamera = 5f;
    public Vector3 cardOffset = Vector3.zero; // Offset adicional se necessário

    public OvoDourado ovo;

    private Camera playerCamera;
    
    void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindObjectOfType<Camera>();
        
        // Esconde a carta inicialmente
        if (card3D != null)
            card3D.SetActive(false);
    }
    
    public void ShowGameOver()
    {
        if (ovo.ovoDourado = false)
        {
            SceneManager.LoadScene("Morte");
        }
        else 
        {
            ovo.UsarAmuleto();
        }

        // Posiciona e ativa a carta 3D
        if (card3D != null && playerCamera != null)
        {
            PositionCard();
            card3D.SetActive(true);

            if (cardScript != null)
                cardScript.ShowCard();
        }
        
        // Pausa o jogo (opcional)
        // Time.timeScale = 0f;
        
        // Libera o cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    void PositionCard()
    {
        // Posiciona a carta na frente da câmera
        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cardPosition = playerCamera.transform.position + (cameraForward * distanceFromCamera);
        cardPosition += cardOffset;
        
        card3D.transform.position = cardPosition;
        
        // Opcionalmente, faz a carta olhar para a câmera inicialmente
        Vector3 lookDirection = playerCamera.transform.position - card3D.transform.position;
        card3D.transform.rotation = Quaternion.LookRotation(-lookDirection);
    }
    
    public void HideGameOver()
    {
        // Esconde tudo
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        if (card3D != null)
            card3D.SetActive(false);
        
        // Volta o tempo normal
        Time.timeScale = 1f;
    }
}