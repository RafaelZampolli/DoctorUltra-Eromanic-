using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CloseButtonFeel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("Visual Settings")]
    public Image buttonImage;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.red;
    public Color pressedColor = new Color(0.8f, 0f, 0f);
    
    [Header("Animation Settings")]
    public float animationDuration = 0.2f;
    public float hoverScale = 1.2f;
    public float pressedScale = 0.9f;
    public float rotationAmount = 15f;
    
    [Header("Shake on Click")]
    public float shakeStrength = 5f;
    public int shakeVibrato = 10;
    
    private Vector3 originalScale;
    private Vector3 originalRotation;
    private Button button;
    
    void Start()
    {
        originalScale = transform.localScale;
        originalRotation = transform.localEulerAngles;
        
        if (buttonImage == null)
            buttonImage = GetComponent<Image>();
        
        button = GetComponent<Button>();
        buttonImage.color = normalColor;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        // Scale + rotate + color
        transform.DOScale(originalScale * hoverScale, animationDuration).SetEase(Ease.OutBack);
        transform.DORotate(originalRotation + Vector3.forward * rotationAmount, animationDuration);
        buttonImage.DOColor(hoverColor, animationDuration);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        // Volta ao normal
        transform.DOScale(originalScale, animationDuration).SetEase(Ease.OutBack);
        transform.DORotate(originalRotation, animationDuration);
        buttonImage.DOColor(normalColor, animationDuration);
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        // Aperta
        transform.DOScale(originalScale * pressedScale, 0.1f);
        buttonImage.DOColor(pressedColor, 0.1f);
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        // Volta pro hover se ainda tiver mouse em cima
        float targetScale = IsPointerOverButton() ? hoverScale : 1f;
        Color targetColor = IsPointerOverButton() ? hoverColor : normalColor;
        
        transform.DOScale(originalScale * targetScale, animationDuration);
        buttonImage.DOColor(targetColor, animationDuration);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        // Shake no clique pra feedback tátil
        transform.DOShakePosition(0.3f, shakeStrength, shakeVibrato, 90f, false, true);
    }
    
    bool IsPointerOverButton()
    {
        return RectTransformUtility.RectangleContainsScreenPoint(
            transform as RectTransform, 
            Input.mousePosition, 
            null
        );
    }
}