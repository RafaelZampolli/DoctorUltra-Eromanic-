using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TyperwriterTutorial : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI mainText;
    public TextMeshProUGUI pressEnterText;
    public Image logoMSF;
    public Image flashPanel;
    
    [Header("Typewriter Settings")]
    public float typeSpeed = 0.05f;
    public float blinkSpeed = 1f;
    
    [Header("Audio")]
    public AudioSource typingSound; // Opcional: som de digitação
    
    [Header("Transition Settings")]
    public float flashInDuration = 0.1f;  // Flash rápido
    public float flashOutDuration = 0.8f; // Fade out lento
    public string gameSceneName = "GameScene"; // Nome da cena do jogo
    
    private string[] dialogues = {
        "Identifique os sintomas e salve o maximo de vidas possivel, esse é o seu objetivo.",
        "Utilize suas cartas para isso, e monte sua estrategia, mas seja rapido, cada segundo aqui vale.",
        "Assinale na sua prancheta e confirme quando tiver certeza.",
        "Porque importante, somente a vida."
    };
    
    private int currentDialogueIndex = 0;
    private bool isTyping = false;
    private bool canContinue = false;
    private bool isTransitioning = false; // Novo flag
    private Coroutine typingCoroutine;
    private Coroutine blinkCoroutine;
    
    void Start()
    {
        // Setup inicial
        mainText.text = "";
        logoMSF.gameObject.SetActive(false);
        
        // Flash panel invisível no início
        Color flashColor = flashPanel.color;
        flashColor.a = 0f;
        flashPanel.color = flashColor;
        
        // Inicia o primeiro texto
        StartNextDialogue();
        
        // Inicia o blink do "Press Enter"
        blinkCoroutine = StartCoroutine(BlinkPressEnter());
    }
    
    void Update()
    {
        // Detecta Enter
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            HandleEnterPress();
        }
    }
    
    void HandleEnterPress()
    {
        // Previne múltiplos cliques durante transição
        if (isTransitioning) return;
        
        if (isTyping)
        {
            // Se tá digitando, completa o texto imediatamente
            StopCoroutine(typingCoroutine);
            mainText.text = dialogues[currentDialogueIndex];
            
            // Cor vermelha se for o último
            if (currentDialogueIndex == dialogues.Length - 1)
            {
                mainText.color = Color.red;
            }
            
            isTyping = false;
            canContinue = true;
        }
        else if (canContinue)
        {
            // Avança pro próximo
            if (currentDialogueIndex < dialogues.Length - 1)
            {
                currentDialogueIndex++;
                StartNextDialogue();
            }
            else if (currentDialogueIndex == dialogues.Length - 1)
            {
                // Último texto - mostra logo e muda botão
                ShowFinalScreen();
            }
            else
            {
                // Vai pro jogo
                StartGame();
            }
        }
    }
    
    void StartNextDialogue()
    {
        canContinue = false;
        mainText.text = "";
        
        // Cor branca pros primeiros textos
        if (currentDialogueIndex < dialogues.Length - 1)
        {
            mainText.color = Color.white;
        }
        
        typingCoroutine = StartCoroutine(TypeText(dialogues[currentDialogueIndex]));
    }
    
    IEnumerator TypeText(string textToType)
    {
        isTyping = true;
        
        foreach (char letter in textToType.ToCharArray())
        {
            mainText.text += letter;
            
            // Som de digitação (opcional)
            if (typingSound != null)
            {
                typingSound.pitch = Random.Range(0.8f, 1.2f); // Variação no pitch
                typingSound.Play();
            }
            
            // Game feel: pequena vibração na tela ou shake sutil
            // Camera.main.transform.position += Random.insideUnitSphere * 0.001f;
            
            yield return new WaitForSeconds(typeSpeed);
        }
        
        // Se for o último texto, deixa vermelho
        if (currentDialogueIndex == dialogues.Length - 1)
        {
            mainText.color = Color.red;
        }
        
        isTyping = false;
        canContinue = true;
    }
    
    IEnumerator BlinkPressEnter()
    {
        while (true)
        {
            pressEnterText.alpha = 1f;
            yield return new WaitForSeconds(blinkSpeed);
            
            pressEnterText.alpha = 0.3f;
            yield return new WaitForSeconds(blinkSpeed);
        }
    }
    
    void ShowFinalScreen()
    {
        // Para o blink atual e muda o texto
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
        
        // Mostra logo com fade in suave
        logoMSF.gameObject.SetActive(true);
        StartCoroutine(FadeInLogo());
        
        // Muda texto do botão e cor
        pressEnterText.text = "IR PARA O PRIMEIRO TURNO";
        pressEnterText.color = Color.red;
        
        // Reinicia blink com novo texto
        blinkCoroutine = StartCoroutine(BlinkPressEnter());
        
        // Atualiza índice e permite continuar
        currentDialogueIndex = dialogues.Length;
        canContinue = false; // Vai ser ativado no FadeInLogo
    }
    
    IEnumerator FadeInLogo()
    {
        Color logoColor = logoMSF.color;
        logoColor.a = 0f;
        logoMSF.color = logoColor;
        
        float duration = 1f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            logoColor.a = Mathf.Lerp(0f, 1f, elapsed / duration);
            logoMSF.color = logoColor;
            yield return null;
        }
        
        // Permite ir pro jogo
        canContinue = true;
    }
    
    void StartGame()
    {
        // Previne múltiplas ativações
        if (isTransitioning) return;
        
        isTransitioning = true;
        
        // Para todos os efeitos visuais
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
        
        // Fade out suave antes de trocar cena
        StartCoroutine(FadeOutAndLoadGame());
    }
    
    IEnumerator FadeOutAndLoadGame()
    {
        Debug.Log("Iniciando transição...");
        
        // Flash branco rápido (fade in)
        yield return StartCoroutine(FlashIn());
        
        Debug.Log("Flash completo - carregando cena...");
        
        // Pequena pausa no branco total
        yield return new WaitForSeconds(0.15f);
        
        // Carrega a cena
        SceneManager.LoadScene(gameSceneName);
        
        // Nota: o fade out vai acontecer na nova cena se necessário
        // ou você pode fazer um GameObject que persiste entre cenas
    }
    
    IEnumerator FlashIn()
    {
        Debug.Log("Flash In iniciado");
        float elapsed = 0f;
        Color flashColor = flashPanel.color;
        flashColor.a = 0f; // Garante que começa transparente
        flashPanel.color = flashColor;
        
        while (elapsed < flashInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / flashInDuration);
            flashColor.a = alpha;
            flashPanel.color = flashColor;
            yield return null;
        }
        
        // Garante que ficou totalmente branco
        flashColor.a = 1f;
        flashPanel.color = flashColor;
        Debug.Log("Flash In completo - Alpha: " + flashPanel.color.a);
    }
    
    IEnumerator FlashOut()
    {
        float elapsed = 0f;
        Color flashColor = flashPanel.color;
        
        while (elapsed < flashOutDuration)
        {
            elapsed += Time.deltaTime;
            flashColor.a = Mathf.Lerp(1f, 0f, elapsed / flashOutDuration);
            flashPanel.color = flashColor;
            yield return null;
        }
        
        // Garante que ficou transparente
        flashColor.a = 0f;
        flashPanel.color = flashColor;
    }
}