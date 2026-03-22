using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;

/// <summary>
/// Attach to: EducationPanel GameObject
/// Shows burn education info when player inspects the burn
/// </summary>
public class EducationPanel : MonoBehaviour
{
    [Header("Text Fields")]
    public TextMeshProUGUI degreeText;
    public TextMeshProUGUI causeText;
    public TextMeshProUGUI looksLikeText;
    public TextMeshProUGUI symptomsText;
    public TextMeshProUGUI emergencyText;

    [Header("Emergency Warning")]
    public GameObject emergencyWarningObject;
    public TextMeshProUGUI emergencyWarningText;

    [Header("Navigation")]
    public GameObject startTreatmentButton;

    [Header("Animation")]
    public CanvasGroup canvasGroup;
    public float revealDelay = 0.3f;

    private Coroutine _revealCoroutine;

    public void ShowForBurn(BurnProfile burn)
    {
        gameObject.SetActive(true);
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);

        if (canvasGroup != null) canvasGroup.DOFade(1f, 0.3f);

        if (startTreatmentButton != null)
            startTreatmentButton.SetActive(false);

        if (degreeText != null) degreeText.text = burn.burnDegree.ToUpper() + " BURN";
        if (causeText != null) causeText.text = "Cause: " + burn.burnCause;
        if (looksLikeText != null) looksLikeText.text = "";
        if (symptomsText != null) symptomsText.text = "";
        if (emergencyText != null) emergencyText.text = "";

        if (emergencyWarningObject != null)
            emergencyWarningObject.SetActive(burn.requiresImmediate999);
        if (emergencyWarningText != null && burn.requiresImmediate999)
            emergencyWarningText.text = "CALL 999 IMMEDIATELY";

        if (_revealCoroutine != null) StopCoroutine(_revealCoroutine);
        _revealCoroutine = StartCoroutine(RevealContent(burn));
    }

    public void Hide()
    {
        if (_revealCoroutine != null) StopCoroutine(_revealCoroutine);

        if (canvasGroup != null)
            canvasGroup.DOFade(0f, 0.2f).OnComplete(() => gameObject.SetActive(false));
        else
            gameObject.SetActive(false);
    }

    IEnumerator RevealContent(BurnProfile burn)
    {
        yield return new WaitForSeconds(0.4f);

        if (looksLikeText != null)
        {
            yield return StartCoroutine(TypeText(looksLikeText,
                "What it looks like:\n" + burn.whatItLooks));
            yield return new WaitForSeconds(revealDelay);
        }

        if (symptomsText != null)
        {
            yield return StartCoroutine(TypeText(symptomsText,
                "Symptoms:\n" + burn.symptoms));
            yield return new WaitForSeconds(revealDelay);
        }

        if (emergencyText != null)
        {
            yield return StartCoroutine(TypeText(emergencyText,
                "Emergency:\n" + burn.emergency));
            yield return new WaitForSeconds(revealDelay);
        }

        // Show start treatment button after all info revealed
        if (startTreatmentButton != null)
        {
            startTreatmentButton.SetActive(true);
            startTreatmentButton.transform.localScale = Vector3.zero;
            startTreatmentButton.transform.DOScale(Vector3.one, 0.3f)
                .SetEase(Ease.OutBack);
        }
    }

    IEnumerator TypeText(TextMeshProUGUI textField, string fullText)
    {
        textField.text = "";
        foreach (char c in fullText)
        {
            textField.text += c;
            yield return new WaitForSeconds(0.015f);
        }
    }
}