using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class QuizPanel : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup canvasGroup;
    public RectTransform panelRect;

    [Header("UI Elements")]
    public TextMeshProUGUI categoryText;
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI counterText;
    public TextMeshProUGUI feedbackText;
    public Image feedbackPanel;
    public Image progressBarFill;
    public Button[] optionButtons;       // 4 buttons in Inspector
    public TextMeshProUGUI[] optionTexts;
    public Button nextButton;
    public TextMeshProUGUI nextButtonText;

    [Header("Score Screen")]
    public GameObject quizScreen;
    public GameObject scoreScreen;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI scoreMsgText;

    [Header("Colors")]
    public Color correctColor = new Color(0.24f, 0.43f, 0.07f, 1f); // green
    public Color wrongColor = new Color(0.89f, 0.29f, 0.29f, 1f); // red
    public Color defaultColor = new Color(0.16f, 0.10f, 0.10f, 1f);
    public Color correctBgLight = new Color(0.92f, 0.95f, 0.87f, 1f);
    public Color wrongBgLight = new Color(0.99f, 0.92f, 0.92f, 1f);

    private QuizQuestion[] _questions;
    private int _current = 0;
    private int _score = 0;
    private bool _answered = false;
    private Vector3 _originalScale;

    void Awake()
    {
        _originalScale = panelRect.localScale;
        panelRect.localScale = Vector3.zero;
        if (canvasGroup != null) canvasGroup.alpha = 0f;

        nextButton.onClick.AddListener(OnNext);
    }

    // ── Called by UIAnimator after treatment ──────────────────────────────────

    public void StartQuiz(QuizQuestion[] questions)
    {
        Debug.Log("StartQuiz called, question count: " + questions.Length);

        _questions = questions;
        _current = 0;
        _score = 0;
        _answered = false;

        quizScreen.SetActive(true);
        scoreScreen.SetActive(false);

        // Reuse same pop-in as your InspectionPanel
        panelRect.DOScale(_originalScale, 0.4f).SetEase(Ease.OutBack);
        canvasGroup.DOFade(1f, 0.3f);

        ShowQuestion();
    }

    public void Hide()
    {
        panelRect.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack);
        canvasGroup.DOFade(0f, 0.2f);
    }

    // ── Quiz logic ────────────────────────────────────────────────────────────

    void ShowQuestion()
    {
        if (_current >= _questions.Length) { ShowScore(); return; }

        _answered = false;
        var q = _questions[_current];

        // Progress bar
        float pct = (float)_current / _questions.Length;
        progressBarFill.DOFillAmount(pct, 0.3f);

        counterText.text = $"{_current + 1} / {_questions.Length}";
        categoryText.text = q.category.ToUpper();
        questionText.text = q.questionText;

        feedbackPanel.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);

        // Set option buttons
        for (int i = 0; i < optionButtons.Length; i++)
        {
            int idx = i; // capture for lambda
            optionTexts[i].text = q.options[i];
            optionButtons[i].interactable = true;

            // Reset colors
            optionButtons[i].GetComponent<Image>().color = defaultColor;
            optionTexts[i].color = Color.white;

            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => OnAnswer(idx));

            // Slide in with stagger — same style as your SpawnRow
            RectTransform rt = optionButtons[i].GetComponent<RectTransform>();
            rt.localScale = new Vector3(0f, 1f, 1f);
            rt.DOScaleX(1f, 0.2f)
              .SetEase(Ease.OutCubic)
              .SetDelay(i * 0.08f);
        }
    }

    void OnAnswer(int idx)
    {
        if (_answered) return;
        _answered = true;

        var q = _questions[_current];
        bool correct = idx == q.correctIndex;
        if (correct) _score++;

        // Colour the buttons
        for (int i = 0; i < optionButtons.Length; i++)
        {
            optionButtons[i].interactable = false;
            var img = optionButtons[i].GetComponent<Image>();

            if (i == q.correctIndex)
            {
                img.DOColor(correctBgLight, 0.25f);
                optionTexts[i].color = correctColor;
            }
            else if (i == idx && !correct)
            {
                img.DOColor(wrongBgLight, 0.25f);
                optionTexts[i].color = wrongColor;
            }
        }

        // Feedback panel
        feedbackPanel.gameObject.SetActive(true);
        feedbackPanel.color = correct ? correctBgLight : wrongBgLight;
        feedbackText.color = correct ? correctColor : wrongColor;
        feedbackText.text = (correct ? "Correct! " : "Not quite. ") + q.explanation;

        // Animate feedback in
        RectTransform frt = feedbackPanel.GetComponent<RectTransform>();
        frt.localScale = new Vector3(1f, 0f, 1f);
        frt.DOScaleY(1f, 0.2f).SetEase(Ease.OutCubic);

        nextButton.gameObject.SetActive(true);
        nextButtonText.text = _current + 1 < _questions.Length
            ? "Next question" : "See results";
    }

    void OnNext()
    {
        _current++;
        ShowQuestion();
    }

    void ShowScore()
    {
        quizScreen.SetActive(false);
        scoreScreen.SetActive(true);

        int pct = Mathf.RoundToInt((float)_score / _questions.Length * 100);
        scoreText.text = $"{_score}/{_questions.Length}  —  {pct}%";
        scoreMsgText.text = pct >= 80 ? "Excellent — you are ready to respond."
                          : pct >= 60 ? "Good effort. Review the missed sections."
                          : "Keep studying — burn recognition saves lives.";
    }
}