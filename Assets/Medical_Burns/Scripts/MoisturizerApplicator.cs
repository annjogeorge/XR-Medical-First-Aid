using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Attach to: Moisturiser/Aloe Vera bottle GameObject
/// Player grabs it and holds near burn for 3 seconds to apply
/// Completes Step 2 for First Degree burn
/// </summary>
public class MoisturiserApplicator : MonoBehaviour
{
    [Header("Settings")]
    public string burnZoneTag = "BurnArea";
    public float holdTime = 3f;

    [Header("Visual Feedback")]
    public GameObject moisturiserEffect;    // Optional particle effect
    public GameObject appliedVisual;        // e.g. shiny skin effect on arm

    [Header("Progress UI")]
    public Image progressFill;
    public TextMeshProUGUI progressText;
    public GameObject progressPanel;

    private float _holdTimer = 0f;
    private bool _isHeld = false;
    private bool _complete = false;
    private XRGrabInteractable _grab;

    void Start()
    {
        _grab = GetComponent<XRGrabInteractable>();
        if (_grab == null)
            _grab = gameObject.AddComponent<XRGrabInteractable>();

        _grab.selectEntered.AddListener(_ =>
        {
            _isHeld = true;
            Debug.Log("MOISTURISER: Picked up");
        });

        _grab.selectExited.AddListener(_ =>
        {
            _isHeld = false;
            _holdTimer = 0f;
            UpdateProgressUI(0f);
            if (progressPanel != null) progressPanel.SetActive(false);
            Debug.Log("MOISTURISER: Put down");
        });
    }

    public void OnBurnEnter(Collider other)
    {
        if (!_isHeld || _complete) return;
        if (!other.CompareTag(burnZoneTag)) return;

        // Cancel any pending hide
        if (_hideCoroutine != null)
        {
            StopCoroutine(_hideCoroutine);
            _hideCoroutine = null;
        }

        if (progressPanel != null) progressPanel.SetActive(true);
        Debug.Log("MOISTURISER: Near burn area");
    }

    public void OnBurnStay(Collider other)
    {
        if (!_isHeld || _complete) return;
        if (!other.CompareTag(burnZoneTag)) return;

        _holdTimer += Time.deltaTime;
        float progress = _holdTimer / holdTime;
        UpdateProgressUI(progress);

        Debug.Log($"MOISTURISER: Holding {_holdTimer:F1}s / {holdTime}s");

        if (_holdTimer >= holdTime)
        {
            _complete = true;
            OnApplied();
        }
    }

    private Coroutine _hideCoroutine;

    public void OnBurnExit(Collider other)
    {
        if (!other.CompareTag(burnZoneTag)) return;
        _holdTimer = 0f;
        UpdateProgressUI(0f);

        // Small delay before hiding so brief exits don't flicker
        if (_hideCoroutine != null) StopCoroutine(_hideCoroutine);
        _hideCoroutine = StartCoroutine(HidePanelDelayed());

        Debug.Log("MOISTURISER: Left burn area — timer reset");
    }

    IEnumerator HidePanelDelayed()
    {
        yield return new WaitForSeconds(0.5f); // 0.5s grace period
        if (progressPanel != null) progressPanel.SetActive(false);
    }

    void OnApplied()
    {
        Debug.Log("MOISTURISER: Applied successfully");
        if (moisturiserEffect != null) moisturiserEffect.SetActive(true);
        if (appliedVisual != null) appliedVisual.SetActive(true);
        if (progressPanel != null) progressPanel.SetActive(false);
        gameObject.SetActive(false);

        int step = GetMoisturiserStepNumber();
        Debug.Log("MOISTURISER: Completing step " + step);

        if (step == 1) TreatmentManager.Instance?.TryCompleteStep1();
        else if (step == 2) TreatmentManager.Instance?.TryCompleteStep2();
        else if (step == 3) TreatmentManager.Instance?.TryCompleteStep3();


    }

    int GetMoisturiserStepNumber()
    {
        BurnProfile burn = TreatmentManager.Instance?.currentBurnProfile;
        if (burn == null) return 2; // default fallback

        for (int i = 0; i < burn.treatmentSteps.Length; i++)
        {
            if (burn.treatmentSteps[i].stepType == StepType.ApplyMoisturiser)
                return i + 1;
        }
        return 2; // fallback
    }

    public void ResetState()
    {
        _complete = false;
        _holdTimer = 0f;
        _isHeld = false;
        if (progressPanel != null) progressPanel.SetActive(false);
    }
    void UpdateProgressUI(float progress)
    {
        if (progressFill != null)
            progressFill.fillAmount = progress;

        if (progressText != null)
        {
            if (progress <= 0f)
                progressText.text = "Hold near burn...";
            else if (progress < 1f)
                progressText.text = $"Applying... {Mathf.RoundToInt(progress * 100f)}%";
            else
                progressText.text = "Moisturiser Applied!";
        }
    }
}