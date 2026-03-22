using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Attach to: MainMenuCanvas GameObject
/// Shows on startup, player picks burn type, menu fades out and training begins
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance { get; private set; }

    [Header("Panel References")]
    public CanvasGroup menuCanvasGroup;
    public RectTransform menuPanel;

    [Header("Burn Type Cards")]
    public BurnCardUI firstDegreeCard;
    public BurnCardUI secondDegreeCard;
    public BurnCardUI thirdDegreeCard;

    [Header("Burn Profiles")]
    public BurnProfile firstDegreeProfile;
    public BurnProfile secondDegreeProfile;
    public BurnProfile thirdDegreeProfile;

    [Header("Subtitle Text")]
    public TextMeshProUGUI subtitleText;

    private BurnCardUI _selectedCard;

    void Awake()
    {
        Instance = this;

        // Lock canvas scale immediately before any other script touches it
        gameObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
    }

    void Start()
    {
        // Re-enforce scale in Start too in case something resets it
        gameObject.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);

        menuPanel.localScale = Vector3.zero;
        menuCanvasGroup.alpha = 0f;

        menuPanel.DOScale(Vector3.one, 0.5f)
            .SetDelay(0.3f)
            .SetEase(Ease.OutBack);

        menuCanvasGroup.DOFade(1f, 0.4f)
            .SetDelay(0.3f);

        firstDegreeCard?.Setup(firstDegreeProfile, this);
        secondDegreeCard?.Setup(secondDegreeProfile, this);
        thirdDegreeCard?.Setup(thirdDegreeProfile, this);

        if (subtitleText != null)
            subtitleText.text = "Select a burn type to begin training";
    }

    // Called by each BurnCardUI when clicked
    public void OnBurnSelected(BurnProfile profile, BurnCardUI card)
    {
        // Deselect previous card
        _selectedCard?.SetSelected(false);
        _selectedCard = card;
        _selectedCard.SetSelected(true);

        if (subtitleText != null)
            subtitleText.text = $"{profile.burnDegree} burn selected.\nPoint at the patient to begin.";

        // Short delay then hide menu and start session
        DOVirtual.DelayedCall(1.2f, () => HideAndStart(profile));
    }

    void HideAndStart(BurnProfile profile)
    {
        // Fade menu out
        menuCanvasGroup.DOFade(0f, 0.4f).OnComplete(() =>
        {
            gameObject.SetActive(false);

            // Tell TrainingManager to start with this specific burn
            TrainingManager.Instance?.StartSessionWithBurn(profile);
        });

        menuPanel.DOScale(new Vector3(0.95f, 0.95f, 0.95f), 0.4f);
    }

    // Called by TrainingManager when session completes
    // to show menu again for next selection
    public void ShowMenu()
    {
        gameObject.SetActive(true);
        menuPanel.localScale = Vector3.zero;
        menuCanvasGroup.alpha = 0f;

        menuPanel.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        menuCanvasGroup.DOFade(1f, 0.4f);

        // Reset card selections
        firstDegreeCard?.SetSelected(false);
        secondDegreeCard?.SetSelected(false);
        thirdDegreeCard?.SetSelected(false);
        _selectedCard = null;

        if (subtitleText != null)
            subtitleText.text = "Select a burn type to begin training";
    }
}