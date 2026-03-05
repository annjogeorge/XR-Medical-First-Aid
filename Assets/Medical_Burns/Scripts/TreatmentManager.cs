// TreatmentManager.cs
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TreatmentManager : MonoBehaviour
{
    public static TreatmentManager Instance { get; private set; }

    [Header("Existing References")]
    public NPCHandGuide npcHandGuide;
    public GameObject waterParticles;
    public DecalProjector burnDecal;
    public TapTrigger tap;

    [Header("Cooling Settings")]
    public float coolingDuration = 30f;
    public float currentCoolingTime = 0f;
    public bool isCooling = false;
    public bool coolingComplete = false;

    [Header("Burn Visual")]
    public float burnFadeRate = 0.01f;
    [Range(0f, 1f)] public float maxFadeAmount = 0.8f;

    private bool _handInPosition;
    private bool _waterOn;

    public bool HandInPosition
    {
        set { _handInPosition = value; TryStartCooling(); }
        get => _handInPosition;
    }
    public bool WaterOn
    {
        set { _waterOn = value; TryStartCooling(); }
        get => _waterOn;
    }

    [Header("Step Tracking")]
    public int currentStep = 0;

    [Header("Treatment Panel")]
    public GameObject treatmentPanel;
    public CanvasGroup treatmentCanvasGroup;

    [Header("Progress Bar")]
    public Image progressBarFill;
    public TextMeshProUGUI progressLabel;

    [Header("Step Rows")]
    public TreatmentStepRow step1Row;
    public TreatmentStepRow step2Row;
    public TreatmentStepRow step3Row;

    [Header("Instruction Text")]
    public TextMeshProUGUI instructionText;

    [Header("Sink Button")]
    public GameObject takeToSinkButton;

    [Header("Completion")]
    public GameObject completionPanel;
    public TextMeshProUGUI completionText;

    [Header("Cooling Timer")]
    public TextMeshProUGUI coolingTimerText;

    public Color completeColor = new Color(0.20f, 0.70f, 0.30f, 1f);

    private string[] _instructions = new string[]
    {
        "",
        "Step 1: Remove the Watch\nPoint at the watch and click to remove it.",
        "Step 2: Cool the Burn\nGrab the right controller and click the trigger button to take the patient to the sink.\nTurn On the tap by touching the right handle of the tap.\nAim at the burn at press grab to take the patient's arm under the cool running water for 20 minutes.",
        "Step 3: Cover the Burn\nGrab the bandage and take turns around the burn area."

    };

    void Awake()
    {
        Instance = this;
        if (completionPanel != null) completionPanel.SetActive(false);
        if (treatmentPanel != null) treatmentPanel.SetActive(false);
    }

    public void BeginTreatment()
    {
        if (treatmentPanel != null)
        {
            treatmentPanel.SetActive(true);
            treatmentPanel.transform.localScale = Vector3.zero;
            treatmentPanel.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
            if (treatmentCanvasGroup != null)
                treatmentCanvasGroup.DOFade(1f, 0.3f);
        }
        step2Row?.SetLocked(true);
        step3Row?.SetLocked(true);
        AdvanceToStep(1);
    }

    public void TryCompleteStep1()
    {
        if (currentStep != 1) return;
        step1Row?.MarkComplete();
        step2Row?.SetLocked(false);
        AdvanceToStep(2);
    }

    public void TryCompleteStep3()
    {
        if (currentStep != 3) return;
        step3Row?.MarkComplete();
        AdvanceToStep(4);
    }

    // ── Your original cooling logic, unchanged except TryStartCooling checks currentStep
    void TryStartCooling()
    {
        if (currentStep != 2) return;
        if (coolingComplete || isCooling) return;
        if (_handInPosition && _waterOn) StartCooling();
    }

    void StartCooling()
    {
        Debug.Log("=== COOLING STARTED ===");
        isCooling = true;
        if (waterParticles != null) waterParticles.SetActive(true);
        StartCoroutine(CoolingProcess());
    }

    IEnumerator CoolingProcess()
    {
        float startFade = burnDecal != null ? burnDecal.fadeFactor : 0.3f;

        if (coolingTimerText != null)
            coolingTimerText.gameObject.SetActive(true);

        while (currentCoolingTime < coolingDuration)
        {
            currentCoolingTime += Time.deltaTime;
            float progress = currentCoolingTime / coolingDuration;
            if (burnDecal != null)
                burnDecal.fadeFactor = Mathf.Lerp(startFade, maxFadeAmount, progress);
            // Show cooling sub-progress in bar (occupies middle third)
            if (progressBarFill != null)
                progressBarFill.fillAmount = 0.33f + (progress * 0.33f);

            if (coolingTimerText != null)
            {
                float timeRemaining = coolingDuration - currentCoolingTime;
                int minutes = Mathf.FloorToInt(timeRemaining / 60f);
                int seconds = Mathf.FloorToInt(timeRemaining % 60f);

                // Color changes as time runs out
                if (timeRemaining > 10f)
                    coolingTimerText.color = Color.white;
                else
                    coolingTimerText.color = new Color(1f, 0.4f, 0.4f); // red when almost done

                coolingTimerText.text = $"Time Remaining: {minutes:0}:{seconds:00}";
            }

            if (currentCoolingTime % 2f < Time.deltaTime)
                Debug.Log($"Cooling: {currentCoolingTime:F1}s / {coolingDuration}s");
            yield return null;
        }

        if (coolingTimerText != null)
            coolingTimerText.gameObject.SetActive(false);

        OnCoolingComplete();
    }

    void OnCoolingComplete()
    {
        Debug.Log("=== COOLING COMPLETE ===");
        coolingComplete = true;
        isCooling = false;
        if (waterParticles != null) waterParticles.SetActive(false);
        if (burnDecal != null) burnDecal.fadeFactor = maxFadeAmount;
        step2Row?.MarkComplete();
        step3Row?.SetLocked(false);
        AdvanceToStep(3);
    }

    void AdvanceToStep(int step)
    {
        currentStep = step;
        Debug.Log("TREATMENT: Advancing to step " + step);
        Debug.Log("TREATMENT: takeToSinkButton is " + takeToSinkButton);
        if (takeToSinkButton != null)
            takeToSinkButton.SetActive(step == 2);
        if (instructionText != null && step <= 3)
            instructionText.text = _instructions[step];
        step1Row?.SetActive(step == 1);
        step2Row?.SetActive(step == 2);
        step3Row?.SetActive(step == 3);
        float targetFill = Mathf.Clamp((step - 1) / 3f, 0f, 1f);
        if (progressBarFill != null)
        {
            progressBarFill.DOFillAmount(targetFill, 0.5f).SetEase(Ease.OutCubic);
            if (step > 3) progressBarFill.DOColor(completeColor, 0.5f);
        }
        if (progressLabel != null)
            progressLabel.text = step <= 3 ? $"Step {step} of 3" : "All Steps Complete";
        if (step > 3) StartCoroutine(ShowCompletion());
    }

    IEnumerator ShowCompletion()
    {
        yield return new WaitForSeconds(0.6f);
        if (instructionText != null) instructionText.text = "";
        if (completionPanel != null)
        {
            completionPanel.SetActive(true);
            completionPanel.transform.localScale = Vector3.zero;
            completionPanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            if (completionText != null)
                completionText.text = "Treatment Complete!\nWell done. The burn has been correctly treated.";
        }
        if (treatmentCanvasGroup != null)
            treatmentCanvasGroup.DOFade(0.25f, 0.8f);
    }
}