using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using System.Collections;
using DG.Tweening;

public class DynamicPopupManager : MonoBehaviour
{
    public static DynamicPopupManager Instance { get; private set; }

    [Header("Estilo")]
    public TMP_FontAsset fonteGlobal;

    [Header("Cores do Pop-up")]
    public Color corFundoPopup = new Color(0.1f, 0.1f, 0.15f, 0.98f);
    public Color corBordaPopup = new Color(0.8f, 0.8f, 0.9f, 1f);
    public Color corTitulo = Color.white;
    public Color corSubtitulo = new Color(0.7f, 0.7f, 0.8f, 1f);

    [Header("Cores dos Itens")]
    public Color corFundoItem = new Color(0.2f, 0.2f, 0.25f, 1f);
    public Color corBordaItem = new Color(0.5f, 0.5f, 0.55f, 1f);
    public Color corTextoItem = Color.white;
    public Color corMouseOverItem = new Color(0.35f, 0.35f, 0.4f, 1f);

    [Header("Cores do Feedback Flutuante")]
    public Color corSucessoFeedback = new Color(0.6f, 1f, 0.6f, 1f);
    public Color corErroFeedback = new Color(1f, 0.6f, 0.6f, 1f);

    [Header("Ícones (Opcional)")]
    public Sprite iconeAEstrela;
    public Sprite iconeAForca;

    private Canvas mainCanvas;
    private GameObject blocker;
    private Coroutine persistentFeedbackCoroutine; // Referência para a corrotina

    void Awake() { if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); SetupCanvas(); } else if (Instance != this) { Destroy(gameObject); } }

    private void SetupCanvas()
    {
        GameObject canvasObj = new GameObject("DynamicPopupCanvas");
        if (Application.isPlaying) { DontDestroyOnLoad(canvasObj); }
        mainCanvas = canvasObj.AddComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        mainCanvas.sortingOrder = 9999;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();
        blocker = new GameObject("PopupBlocker");
        blocker.transform.SetParent(mainCanvas.transform, false);
        RectTransform blockerRect = blocker.AddComponent<RectTransform>();
        blockerRect.anchorMin = Vector2.zero;
        blockerRect.anchorMax = Vector2.one;
        blockerRect.sizeDelta = Vector2.zero;
        Image blockerImage = blocker.AddComponent<Image>();
        blockerImage.color = new Color(0, 0, 0, 0.5f);
        blocker.SetActive(false);
    }

    // --- VERSÃO 1 (CHAMADA PADRÃO) ---
    // Esta é a sobrecarga que faltava. Ela simplesmente chama a outra versão com uma duração padrão.
    public void ShowPersistentFloatingFeedback(string message, Vector3 worldPosition, bool success)
    {
        ShowPersistentFloatingFeedback(message, worldPosition, success, 2.0f); // Duração padrão de 2 segundos
    }

    // --- VERSÃO 2 (CHAMADA COM DURAÇÃO) ---
    // Esta é a sua função, agora corrigida para usar a duração.
    public void ShowPersistentFloatingFeedback(string message, Vector3 worldPosition, bool success, float duration)
    {
        if (mainCanvas == null) return;

        // Interrompe a corrotina anterior se estiver ativa
        if (persistentFeedbackCoroutine != null)
        {
            StopCoroutine(persistentFeedbackCoroutine);
        }

        persistentFeedbackCoroutine = StartCoroutine(FloatingFeedbackRoutine(message, worldPosition, success, duration));
    }

    // --- ROTINA DE ANIMAÇÃO CORRIGIDA ---
    private IEnumerator FloatingFeedbackRoutine(string message, Vector3 worldPosition, bool success, float duration)
    {
        GameObject feedbackObject = new GameObject("FloatingFeedbackText");
        feedbackObject.transform.SetParent(mainCanvas.transform, false);

        ContentSizeFitter fitter = feedbackObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        TextMeshProUGUI tmp = feedbackObject.AddComponent<TextMeshProUGUI>();
        tmp.text = message;
        tmp.fontSize = 36;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        if (fonteGlobal != null) tmp.font = fonteGlobal;

        var outline = feedbackObject.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.8f);
        outline.effectDistance = new Vector2(2f, -2f);

        RectTransform rectTransform = feedbackObject.GetComponent<RectTransform>();
        if (worldPosition == Vector3.zero) { rectTransform.anchorMin = new Vector2(0.5f, 0.5f); rectTransform.anchorMax = new Vector2(0.5f, 0.5f); rectTransform.pivot = new Vector2(0.5f, 0.5f); rectTransform.anchoredPosition = Vector2.zero; }
        else { Camera mainCamera = Camera.main; if (mainCamera != null) { rectTransform.position = mainCamera.WorldToScreenPoint(worldPosition + new Vector3(0, 0.5f, 0)); } else { rectTransform.anchoredPosition = Vector2.zero; } }

        tmp.color = success ? corSucessoFeedback : corErroFeedback;
        rectTransform.localScale = Vector3.zero;

        // Animação de entrada
        rectTransform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(0.4f);

        // Espera pela duração definida, menos o tempo de fade out
        float waitTime = duration - 0.5f;
        if (waitTime > 0)
        {
            yield return new WaitForSeconds(waitTime);
        }

        // Animação de saída (fade out)
        tmp.DOFade(0, 0.5f);
        rectTransform.DOMoveY(rectTransform.position.y + 50, 0.5f).OnComplete(() => {
            if (feedbackObject != null)
            {
                Destroy(feedbackObject);
            }
        });
    }

    // O resto do seu código permanece igual
    public void DismissPersistentFeedback() { /* ...seu código aqui... */ }
    public void ShowInfoPopup(string title, Dictionary<string, bool> infoLines) { StartCoroutine(ShowPopupRoutine(title, null, infoLines, null)); }
    public void ShowChoicePopup(string title, List<string> options, Action<string> onChoiceSelected) { StartCoroutine(ShowPopupRoutine(title, options, null, onChoiceSelected)); }
    private IEnumerator ShowPopupRoutine(string title, List<string> options, Dictionary<string, bool> infoLines, Action<string> onChoiceSelected) { blocker.SetActive(true); if (persistentFeedbackCoroutine != null) StopCoroutine(persistentFeedbackCoroutine); GameObject popupContainer = CreatePopupContainer(title); List<Transform> itemsToAnimate = new List<Transform>(); if (options != null) { CreateText(popupContainer.transform, "Escolha um sintoma para checar:", 18, corSubtitulo, TextAlignmentOptions.Center); GameObject buttonContainer = CreateButtonContainer(popupContainer.transform, options.Count <= 3); foreach (string option in options) { GameObject btn = CreateButton(buttonContainer.transform, option, () => { ClosePopup(popupContainer); onChoiceSelected?.Invoke(option); }); itemsToAnimate.Add(btn.transform); } } if (infoLines != null) { foreach (var line in infoLines) { itemsToAnimate.Add(CreateTextLine(popupContainer.transform, line.Key, line.Value).transform); } GameObject spacer = new GameObject("Spacer"); spacer.transform.SetParent(popupContainer.transform, false); spacer.AddComponent<RectTransform>().sizeDelta = new Vector2(0, 10); itemsToAnimate.Add(spacer.transform); itemsToAnimate.Add(CreateButton(popupContainer.transform, "Fechar", () => ClosePopup(popupContainer)).transform); } AnimateIn(popupContainer, itemsToAnimate); yield return null; }
    private GameObject CreatePopupContainer(string title) { GameObject container = new GameObject("PopupContainer"); container.transform.SetParent(blocker.transform, false); RectTransform rect = container.AddComponent<RectTransform>(); rect.sizeDelta = new Vector2(450, 100); Image bg = container.AddComponent<Image>(); bg.color = corFundoPopup; Outline outline = container.AddComponent<Outline>(); outline.effectColor = corBordaPopup; outline.effectDistance = new Vector2(2, -2); VerticalLayoutGroup layout = container.AddComponent<VerticalLayoutGroup>(); layout.padding = new RectOffset(25, 25, 25, 25); layout.spacing = 15; layout.childControlWidth = true; layout.childForceExpandWidth = true; ContentSizeFitter fitter = container.AddComponent<ContentSizeFitter>(); fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize; CreateTitleWithIcon(container.transform, title); return container; }
    private GameObject CreateButtonContainer(Transform parent, bool horizontal) { GameObject container = new GameObject("ButtonContainer"); container.transform.SetParent(parent, false); if (horizontal) { var hlg = container.AddComponent<HorizontalLayoutGroup>(); hlg.spacing = 15; hlg.childForceExpandWidth = true; } else { var vlg = container.AddComponent<VerticalLayoutGroup>(); vlg.spacing = 15; } return container; }
    private TextMeshProUGUI CreateText(Transform parent, string content, int fontSize, Color color, TextAlignmentOptions alignment) { GameObject textObj = new GameObject("Text"); textObj.transform.SetParent(parent, false); TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>(); tmp.text = content; tmp.fontSize = fontSize; tmp.color = color; tmp.alignment = alignment; if (fonteGlobal != null) tmp.font = fonteGlobal; return tmp; }
    private GameObject CreateButton(Transform parent, string text, Action onClick) { GameObject btnContainer = new GameObject("ButtonContainer"); btnContainer.transform.SetParent(parent, false); Image btnImg = btnContainer.AddComponent<Image>(); btnImg.color = corFundoItem; Button btn = btnContainer.AddComponent<Button>(); btn.targetGraphic = btnImg; btn.onClick.AddListener(() => onClick?.Invoke()); btn.transition = Selectable.Transition.ColorTint; Outline outline = btnContainer.AddComponent<Outline>(); outline.effectColor = corBordaItem; ColorBlock cb = btn.colors; cb.normalColor = corFundoItem; cb.highlightedColor = corMouseOverItem; cb.pressedColor = Color.Lerp(corMouseOverItem, Color.black, 0.2f); cb.selectedColor = corFundoItem; btn.colors = cb; HorizontalLayoutGroup layout = btnContainer.AddComponent<HorizontalLayoutGroup>(); layout.padding = new RectOffset(15, 15, 15, 15); layout.childAlignment = TextAnchor.MiddleCenter; CreateText(btnContainer.transform, text, 22, corTextoItem, TextAlignmentOptions.Center); btnContainer.transform.localScale = Vector3.zero; return btnContainer; }
    private GameObject CreateTextLine(Transform parent, string label, bool success) { GameObject lineContainer = new GameObject("InfoLineContainer"); lineContainer.transform.SetParent(parent, false); Image bg = lineContainer.AddComponent<Image>(); bg.color = corFundoItem; Outline outline = lineContainer.AddComponent<Outline>(); outline.effectColor = corBordaItem; HorizontalLayoutGroup layout = lineContainer.AddComponent<HorizontalLayoutGroup>(); layout.padding = new RectOffset(15, 15, 10, 10); layout.spacing = 10; layout.childAlignment = TextAnchor.MiddleLeft; layout.childForceExpandWidth = false; var labelText = CreateText(lineContainer.transform, label, 20, Color.white, TextAlignmentOptions.Left); labelText.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1; string resultText = success ? "SIM" : "NÃO"; Color resultColor = success ? corSucessoFeedback : corErroFeedback; var resultTMP = CreateText(lineContainer.transform, resultText, 20, resultColor, TextAlignmentOptions.Right); resultTMP.fontStyle = FontStyles.Bold; lineContainer.transform.localScale = Vector3.zero; return lineContainer; }
    private void CreateTitleWithIcon(Transform parent, string title) { GameObject titleContainer = new GameObject("TitleContainer"); titleContainer.transform.SetParent(parent, false); HorizontalLayoutGroup layout = titleContainer.AddComponent<HorizontalLayoutGroup>(); layout.spacing = 15; layout.childAlignment = TextAnchor.MiddleCenter; Sprite icon = GetIconForTitle(title); if (icon != null) { GameObject iconObj = new GameObject("Icon"); iconObj.transform.SetParent(titleContainer.transform, false); Image iconImg = iconObj.AddComponent<Image>(); iconImg.sprite = icon; iconImg.preserveAspect = true; iconObj.AddComponent<LayoutElement>().preferredWidth = 32; } CreateText(titleContainer.transform, title, 28, corTitulo, TextAlignmentOptions.Center).fontStyle = FontStyles.Bold; }
    private void AnimateIn(GameObject popup, List<Transform> items) { var popupRect = popup.GetComponent<RectTransform>(); popupRect.localScale = Vector3.one * 0.8f; popup.GetComponent<Image>().DOFade(0.98f, 0.3f); popupRect.DOScale(1f, 0.4f).SetEase(Ease.OutBack).OnComplete(() => { StartCoroutine(AnimateItemsSequentially(items)); }); }
    private IEnumerator AnimateItemsSequentially(List<Transform> items) { foreach (var item in items) { if (item != null) { item.DOScale(1f, 0.3f).SetEase(Ease.OutBack); yield return new WaitForSeconds(0.08f); } } }
    private void ClosePopup(GameObject popup) { blocker.SetActive(false); popup.transform.DOScale(0.8f, 0.2f).SetEase(Ease.InBack).OnComplete(() => { Destroy(popup); }); }
    private Sprite GetIconForTitle(string title) { if (title.Contains("Estrela")) return iconeAEstrela; if (title.Contains("Força")) return iconeAForca; return null; }
}