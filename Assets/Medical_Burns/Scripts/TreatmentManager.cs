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

    [Header("Teleport Visuals")]
    public GameObject sinkAnchorVisual;
    public GameObject patientAnchorVisual;
    public GameObject counterAnchorVisual;

    [Header("Current Profile")]
    public BurnProfile currentBurnProfile;

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

    [Header("Third Degree Buttons")]
    public GameObject call999Button;
    public GameObject confirmNoClothingButton;

    [Header("Cooling Timer")]
    public TextMeshProUGUI coolingTimerText;

    public Color completeColor = new Color(0.20f, 0.70f, 0.30f, 1f);

    private string[] _instructions = new string[4];

    void Awake()
    {
        Instance = this;
        if (completionPanel != null) completionPanel.SetActive(false);
        if (treatmentPanel != null) treatmentPanel.SetActive(false);
    }

    // ADD THESE METHODS to your existing TreatmentManager.cs
    // Also add this field at the top with other fields:
    // public BurnProfile currentBurnProfile;

    // Replace BeginTreatment() with this version:
    public void BeginTreatment(BurnProfile burn)
    {
        currentBurnProfile = burn;


        int stepCount = burn.treatmentSteps.Length;

        // Hide unused rows
        step3Row?.gameObject.SetActive(stepCount >= 3);

        // Only load instructions that exist
        _instructions = new string[stepCount + 1];
        _instructions[0] = "";
        for (int i = 0; i < stepCount; i++)
        {
            _instructions[i + 1] = "Step " + (i + 1) + ": "
                + burn.treatmentSteps[i].title + "\n"
                + burn.treatmentSteps[i].instruction;
        }

        if (stepCount >= 1) step1Row?.UpdateLabel(burn.treatmentSteps[0].title);
        if (stepCount >= 2) step2Row?.UpdateLabel(burn.treatmentSteps[1].title);
        if (stepCount >= 3) step3Row?.UpdateLabel(burn.treatmentSteps[2].title);

        // Reset cooling state for new session
        currentCoolingTime = 0f;
        coolingComplete = false;
        isCooling = false;

        // Set correct cooling duration per burn type
        if (burn.burnDegree.Contains("First"))
            coolingDuration = 60f;    // 1 min for testing, change to 600f for real
        else if (burn.burnDegree.Contains("Second"))
            coolingDuration = 120f;   // 2 min for testing, change to 1200f for real
        else
            coolingDuration = 0f;     // Third degree — no cooling at all

    

        if (treatmentPanel != null)
        {
            treatmentPanel.SetActive(true);
            treatmentPanel.transform.localScale = Vector3.zero;
            treatmentPanel.transform.DOScale(new Vector3(0.001f, 0.001f, 0.001f), 0.4f)
                .SetEase(Ease.OutBack);
            if (treatmentCanvasGroup != null)
                treatmentCanvasGroup.DOFade(1f, 0.3f);
        }

        step2Row?.SetLocked(true);
        step3Row?.SetLocked(true);
        AdvanceToStep(1);
    }

    // Add this new ResetTreatment method:
    public void ResetTreatment()
    {
        currentStep = 0;
        currentCoolingTime = 0f;
        isCooling = false;
        coolingComplete = false;
        _handInPosition = false;
        _waterOn = false;
        if (call999Button != null) call999Button.SetActive(false);
        if (confirmNoClothingButton != null) confirmNoClothingButton.SetActive(false);
        step1Row?.SetLocked(false);
        step2Row?.SetLocked(true);
        step3Row?.SetLocked(true);

        if (progressBarFill != null) progressBarFill.fillAmount = 0f;
        if (progressLabel != null) progressLabel.text = "Step 1 of 3";
        if (instructionText != null) instructionText.text = "";
        if (treatmentPanel != null) treatmentPanel.SetActive(false);
        if (completionPanel != null) completionPanel.SetActive(false);
        if (coolingTimerText != null) coolingTimerText.gameObject.SetActive(false);

        Debug.Log("TREATMENT: Reset complete");
    }

    // At the end of ShowCompletion coroutine, add:
    // TrainingManager.Instance?.OnTreatmentComplete();

    public void TryCompleteStep1()
    {
        if (currentStep != 1) return;
        step1Row?.MarkComplete();
        step2Row?.SetLocked(false);
        AdvanceToStep(2);
    }

    // ADD THIS — was missing entirely
    public void TryCompleteStep2()
    {
        if (currentStep != 2) return;
        step2Row?.MarkComplete();
        step3Row?.SetLocked(false);
        AdvanceToStep(3);
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
        // Find which step number is the CoolWater step for this burn
        int coolWaterStep = GetCoolWaterStepNumber();
        if (coolWaterStep == -1) return;       // No cooling step for this burn
        if (currentStep != coolWaterStep) return;
        if (coolingComplete || isCooling) return;
        if (_handInPosition && _waterOn) StartCooling();
    }

    int GetCoolWaterStepNumber()
    {
        if (currentBurnProfile == null) return -1;

        for (int i = 0; i < currentBurnProfile.treatmentSteps.Length; i++)
        {
            if (currentBurnProfile.treatmentSteps[i].stepType == StepType.CoolWater)
                return i + 1; // Steps are 1-indexed
        }
        return -1; // No CoolWater step found
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
        int totalSteps = currentBurnProfile?.treatmentSteps.Length ?? 3;
        int coolStep = GetCoolWaterStepNumber();

        if (sinkAnchorVisual != null)
            sinkAnchorVisual.SetActive(currentStep == coolStep);

        if (patientAnchorVisual != null)
            patientAnchorVisual.SetActive(currentStep == 1 || currentStep == 3);

        if (counterAnchorVisual != null)
            counterAnchorVisual.SetActive(false);

        if (coolingTimerText != null)
            coolingTimerText.gameObject.SetActive(true);

        while (currentCoolingTime < coolingDuration)
        {
            currentCoolingTime += Time.deltaTime;
            float progress = currentCoolingTime / coolingDuration;

            if (burnDecal != null)
                burnDecal.fadeFactor = Mathf.Lerp(startFade, maxFadeAmount, progress);

            // Fill bar between this step's start and end position
            if (progressBarFill != null)
            {
                float stepStart = (float)(coolStep - 1) / totalSteps;
                float stepEnd = (float)coolStep / totalSteps;
                progressBarFill.fillAmount = Mathf.Lerp(stepStart, stepEnd, progress);
            }

            if (coolingTimerText != null)
            {
                float timeRemaining = coolingDuration - currentCoolingTime;
                int minutes = Mathf.FloorToInt(timeRemaining / 60f);
                int seconds = Mathf.FloorToInt(timeRemaining % 60f);
                coolingTimerText.color = timeRemaining > 10f
                    ? Color.white
                    : new Color(1f, 0.4f, 0.4f);
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

        int coolWaterStep = GetCoolWaterStepNumber();

        // Mark whichever row the cooling was on
        if (coolWaterStep == 1) { step1Row?.MarkComplete(); step2Row?.SetLocked(false); }
        if (coolWaterStep == 2) { step2Row?.MarkComplete(); step3Row?.SetLocked(false); }

        AdvanceToStep(coolWaterStep + 1);
    }

    void AdvanceToStep(int step)
    {
        currentStep = step;
        int totalSteps = currentBurnProfile?.treatmentSteps.Length ?? 3;

        // Done when step exceeds total
        if (step > totalSteps)
        {
            StartCoroutine(ShowCompletion());
            return;
        }

        // Only show sink button for burns that need cooling
        if (takeToSinkButton != null)
        {
            int coolStep = GetCoolWaterStepNumber();
            bool needsSink = coolStep != -1; // has a cooling step
            takeToSinkButton.SetActive(step == coolStep && needsSink);
        }

        bool isThird = currentBurnProfile?.burnDegree.Contains("Third") ?? false;

        if (call999Button != null)
            call999Button.SetActive(isThird && step == 1);

        if (confirmNoClothingButton != null)
            confirmNoClothingButton.SetActive(isThird && step == 2);

        if (instructionText != null && step <= 3)
            instructionText.text = _instructions[step];

        step1Row?.SetActive(step == 1);
        step2Row?.SetActive(step == 2);
        step3Row?.SetActive(step == 3);

        float targetFill = Mathf.Clamp((float)(step - 1) / totalSteps, 0f, 1f); if (progressBarFill != null)
        {
            progressBarFill.DOFillAmount(targetFill, 0.5f).SetEase(Ease.OutCubic);
            if (step > 3) progressBarFill.DOColor(completeColor, 0.5f);
        }

        if (progressLabel != null)
            progressLabel.text = $"Step {step} of {totalSteps}";
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

        TrainingManager.Instance?.OnTreatmentComplete();

    }
}