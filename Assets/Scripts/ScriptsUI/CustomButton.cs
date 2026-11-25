using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class CustomButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Text Settings")]
    public Text buttonText;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public Color pressedColor = Color.gray;
    
    [Header("Animation Settings")]
    public float animationSpeed = 0.15f;
    public float hoverScale = 1.1f;
    public float pressedScale = 0.95f;
    
    private Vector3 originalScale;
    private Button button;
    
    void Start()
    {
        originalScale = transform.localScale;
        button = GetComponent<Button>();
        
        if (buttonText == null)
            buttonText = GetComponentInChildren<Text>();
            
        buttonText.color = normalColor;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.interactable)
        {
            StopAllCoroutines();
            StartCoroutine(AnimateButton(hoverScale, hoverColor));
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (button.interactable)
        {
            StopAllCoroutines();
            StartCoroutine(AnimateButton(1f, normalColor));
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (button.interactable)
        {
            StopAllCoroutines();
            StartCoroutine(AnimateButton(pressedScale, pressedColor));
        }
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (button.interactable)
        {
            StopAllCoroutines();
            StartCoroutine(AnimateButton(hoverScale, hoverColor));
        }
    }
    
    IEnumerator AnimateButton(float targetScale, Color targetColor)
    {
        Vector3 startScale = transform.localScale;
        Color startColor = buttonText.color;
        
        float elapsed = 0f;
        
        while (elapsed < animationSpeed)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationSpeed;
            
            // Smooth easing
            t = Mathf.SmoothStep(0f, 1f, t);
            
            transform.localScale = Vector3.Lerp(startScale, originalScale * targetScale, t);
            buttonText.color = Color.Lerp(startColor, targetColor, t);
            
            yield return null;
        }
    }
}