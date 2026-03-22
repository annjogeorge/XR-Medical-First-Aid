using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using static UnityEditor.PlayerSettings;

/// <summary>
/// Attach to: StartTreatmentButton GameObject (child of BurnZone)
/// Floats near the burn, appears after inspection panel opens,
/// hides inspection panel and shows TakeToSink on click
/// </summary>
public class StartTreatmentButton : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag your CloseUpBurn canvas here")]
    public GameObject inspectionCanvas;

    [Tooltip("Drag your TakeToSink GameObject here")]
    public GameObject takeToSinkPanel;

    [Tooltip("The button's own canvas")]
    public Canvas buttonCanvas;

    [Tooltip("The visual button background image")]
    public Image buttonBackground;

    [Tooltip("The button label")]
    public TextMeshProUGUI buttonText;

    [Header("Button Style")]
    public Color normalColor = new Color(0.80f, 0.10f, 0.10f, 0.95f); // Medical red
    public Color hoverColor = new Color(1.00f, 0.20f, 0.20f, 1.00f); // Brighter red on hover
    public Color textColor = Color.white;

    [Header("Float Offset")]
    [Tooltip("Where the button floats relative to BurnZone")]
    public Vector3 floatOffset = new Vector3(0.08f, 0.05f, 0.05f);

    private CanvasGroup _canvasGroup;
    private bool _isVisible = false;

    void Awake()
    {
        // Start hidden
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        transform.localScale = Vector3.zero;

        // Apply colors
        if (buttonBackground != null) buttonBackground.color = normalColor;
        if (buttonText != null)
        {
            buttonText.text = "Start Treatment";
            buttonText.color = textColor;
        }

        // Make sure TakeToSink starts hidden
        if (takeToSinkPanel != null)
            takeToSinkPanel.SetActive(false);
    }

    // Called by UIAnimator after inspection panel finishes appearing
    public void Show()
    {
        if (_isVisible) return;
        _isVisible = true;

        // Position near the burn
        transform.localPosition = floatOffset;

        // Animate in with slight delay after panel
        transform.DOScale(Vector3.one, 0.3f)
            .SetDelay(0.5f)
            .SetEase(Ease.OutBack);

        _canvasGroup.DOFade(1f, 0.3f)
            .SetDelay(0.5f)
            .OnComplete(() => {
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            });
    }

    public void Hide()
    {
        if (!_isVisible) return;
        _isVisible = false;

        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack);
        _canvasGroup.DOFade(0f, 0.2f);
    }

    // Called by the Button component's OnClick event
    public void OnStartTreatmentClicked()
    {
        // Hide inspection canvas
        if (inspectionCanvas != null)
        {
            CanvasGroup cg = inspectionCanvas.GetComponent<CanvasGroup>();
            if (cg != null)
                cg.DOFade(0f, 0.3f).OnComplete(() => inspectionCanvas.SetActive(false));
            else
                inspectionCanvas.SetActive(false);
        }

        // Hide this button
        Hide();

        // Make sure old TakeToSink stays hidden
        if (takeToSinkPanel != null)
            takeToSinkPanel.SetActive(false);

        // Show the new treatment panel with all 3 steps
        TrainingManager.Instance?.StartTreatmentPhase();
    }

    // Hover effects for VR pointer
    public void OnPointerEnter()
    {
        if (buttonBackground != null)
            buttonBackground.DOColor(hoverColor, 0.15f);

        transform.DOScale(Vector3.one * 1.1f, 0.15f);
    }

    public void OnPointerExit()
    {
        if (buttonBackground != null)
            buttonBackground.DOColor(normalColor, 0.15f);

        transform.DOScale(Vector3.one, 0.15f);
    }
}