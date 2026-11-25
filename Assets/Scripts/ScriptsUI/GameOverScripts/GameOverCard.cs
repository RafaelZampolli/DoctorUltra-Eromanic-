using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverCard : MonoBehaviour
{
    [Header("Textura da Carta")]
    public Texture2D cardTexture; // A textura específica para o Game Over
    
    [Header("Animação")]
    public float rotationSpeed = 50f;
    public float floatAmplitude = 0.1f; // Diminuído de 0.3f para 0.1f
    public float floatSpeed = 1.5f; // Diminuído de 2f para 1.5f
    
    [Header("Escala de Entrada")]
    public float scaleSpeed = 2f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Configurações")]
    public string menuSceneName = "Menu"; // Nome da cena do menu
    
    private Vector3 startPosition;
    private Vector3 targetScale;
    private float scaleTimer = 0f;
    private bool isAnimatingIn = true;
    private Camera playerCamera;
    private Collider cardCollider;
    private Renderer cardRenderer;
    private Material cardMaterial;

    [Header("Escala Final da Carta")]
    public Vector3 finalScale = new Vector3(1f, 1f, 1f);

void Start()
{
    // Guarda escala alvo e começa com escala zero (animação de entrada)
    targetScale = finalScale;
    transform.localScale = Vector3.zero;

    // Pega a câmera
    playerCamera = Camera.main;
    if (playerCamera == null)
        playerCamera = FindFirstObjectByType<Camera>();

    // 1) Garante que não herde transforms estranhos
    transform.SetParent(null, true);

    // 2) Define posição inicial em frente à câmera
    if (playerCamera != null)
    {
        float distancia = 0.5f; // ajuste a distância da carta até a câmera
        startPosition = playerCamera.transform.position + playerCamera.transform.forward * distancia;
        transform.position = startPosition;

        // 3) Faz a face da carta olhar para a câmera
        transform.rotation = Quaternion.LookRotation(playerCamera.transform.forward, Vector3.up);

        // Se a carta estiver virada para trás, inverte
        if (Vector3.Dot(transform.forward, playerCamera.transform.forward) < 0f)
            transform.Rotate(0f, 180f, 0f, Space.Self);
    }
    else
    {
        // fallback: posição/rotação padrão
        startPosition = new Vector3(0f, 0f, 0f);
        transform.position = startPosition;
        transform.rotation = Quaternion.identity;
    }

    // 4) Se houver Rigidbody, trava física
    var rb = GetComponent<Rigidbody>();
    if (rb != null)
    {
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    // 5) Collider + Renderer + Material
    cardCollider = GetComponent<Collider>();
    if (cardCollider == null) cardCollider = gameObject.AddComponent<BoxCollider>();

    cardRenderer = GetComponent<Renderer>();
    if (cardRenderer == null) cardRenderer = GetComponentInChildren<Renderer>();
    if (cardRenderer != null)
    {
        cardMaterial = cardRenderer.material;
        if (cardTexture != null) cardMaterial.mainTexture = cardTexture;
    }
}


void Update()
{
    // animação de entrada (escala) == seu código
    if (isAnimatingIn)
    {
        scaleTimer += Time.deltaTime * scaleSpeed;
        float scaleProgress = scaleCurve.Evaluate(scaleTimer);
        transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, scaleProgress);

        if (scaleTimer >= 1f)
        {
            isAnimatingIn = false;
            transform.localScale = targetScale;
        }
    }

    // Rotaciona em torno do eixo local (não world)
    transform.Rotate(Vector3.up * (rotationSpeed * Time.deltaTime), Space.Self);

    // Flutuação: aplica só no Y, em cima do startPosition fixo
    float offset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
    // clamp só por segurança
    offset = Mathf.Clamp(offset, -Mathf.Abs(floatAmplitude), Mathf.Abs(floatAmplitude));
    transform.position = new Vector3(startPosition.x, startPosition.y + offset, startPosition.z);

    // clique
    if (Input.GetMouseButtonDown(0)) CheckCardClick();
}

    
    void CheckCardClick()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider == cardCollider)
            {
                OnCardClicked();
            }
        }
    }
    
    void OnCardClicked()
    {
        // Adiciona um pequeno efeito de click (opcional)
        StartCoroutine(ClickEffect());
    }
    
    System.Collections.IEnumerator ClickEffect()
    {
        // Pequena animação de "aperto" quando clica
        Vector3 originalScale = transform.localScale;
        Vector3 clickScale = originalScale * 0.9f;
        
        float timer = 0f;
        float duration = 0.1f;
        
        // Diminui
        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, clickScale, timer / duration);
            yield return null;
        }
        
        timer = 0f;
        
        // Volta ao normal
        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(clickScale, originalScale, timer / duration);
            yield return null;
        }
        
        transform.localScale = originalScale;
        
        // Aguarda um pouco e volta ao menu
        yield return new WaitForSeconds(0.2f);
        GoToMenu();
    }
    
    void GoToMenu()
    {
        // Carrega a cena do menu
        SceneManager.LoadScene(menuSceneName);
    }
    
    // Método público para ativar a carta (chame do seu GameManager)
    public void ShowCard()
    {
        gameObject.SetActive(true);
    }
    
    // Método público para esconder a carta
    public void HideCard()
    {
        gameObject.SetActive(false);
    }
    
    // Método para trocar a textura da carta (caso precise mudar dinamicamente)
    public void SetCardTexture(Texture2D newTexture)
    {
        if (cardMaterial != null && newTexture != null)
        {
            cardMaterial.mainTexture = newTexture;
        }
    }
} 