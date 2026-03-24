using UnityEngine;
using TMPro;
using DG.Tweening;

/// <summary>
/// Attach to: CompletionPanel GameObject
/// Shows when all treatment steps are done
/// </summary>
public class CompletionUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI burnTypeText;
    public TextMeshProUGUI summaryText;
    public GameObject tryAnotherButton;

    [Header("Animation")]
    public CanvasGroup canvasGroup;

    private Vector3 _originalScale;

    [Header("Quiz")]
    public QuizPanel quizPanel;
    public QuizQuestion[] allQuestions;

    public void OnTryQuizClicked()
    {
        // hide completion panel
        canvasGroup.DOFade(0f, 0.2f).OnComplete(() =>
            gameObject.SetActive(false));

        // launch quiz
        quizPanel.StartQuiz(allQuestions);
    }
    void Awake()
    {
        _originalScale = transform.localScale;
        gameObject.SetActive(false);
    }
    public void Show(BurnProfile completedBurn)
    {
        gameObject.SetActive(true);
        transform.localScale = Vector3.zero;
        transform.localScale = Vector3.zero;
        transform.DOScale(_originalScale, 0.5f).SetEase(Ease.OutBack);

        if (titleText != null) titleText.text = "Treatment Complete!";
        if (burnTypeText != null) burnTypeText.text = completedBurn.burnDegree + " Burn";
        if (summaryText != null) summaryText.text = GetSummary(completedBurn);

        // Bounce in the button after a short delay
        if (tryAnotherButton != null)
        {
            tryAnotherButton.SetActive(false);
            DOVirtual.DelayedCall(1f, () =>
            {
                tryAnotherButton.SetActive(true);
                tryAnotherButton.transform.localScale = Vector3.zero;
                tryAnotherButton.transform.DOScale(Vector3.one, 0.3f)
                                    .SetEase(Ease.OutBack);
            });
        }
    }

    string GetSummary(BurnProfile burn)
    {
        if (burn.burnDegree.Contains("First"))
            return "Well done! You correctly treated a first degree burn.\n\nFirst degree burns heal within 3-5 days with proper care.";
        else if (burn.burnDegree.Contains("Second"))
            return "Well done! You correctly treated a second degree burn.\n\nAlways cool the burn for at least 20 minutes.";
        else
            return "Well done! You correctly handled a third degree burn.\n\nAlways call 999 immediately for third degree burns.";
    }

    // Called by Try Another Burn button OnClick
    public void OnTryAnotherClicked()
    {
        gameObject.SetActive(false);
        TrainingManager.Instance?.ReturnToMenu();
    }
}