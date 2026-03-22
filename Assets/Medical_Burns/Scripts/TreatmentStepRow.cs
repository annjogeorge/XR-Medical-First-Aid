using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TreatmentStepRow : MonoBehaviour
{
    public TextMeshProUGUI stepLabel;
    public TextMeshProUGUI statusText;
    public Image rowBackground;
    public Image statusIcon;

    public Color lockedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
    public Color activeColor = new Color(0.20f, 0.55f, 0.90f, 1f);
    public Color completeColor = new Color(0.20f, 0.70f, 0.30f, 1f);

    void Start()
    {
        if (statusText != null) statusText.text = "Locked";
        if (statusIcon != null) statusIcon.color = lockedColor;
    }

    public void SetLocked(bool locked)
    {
        if (statusText != null) statusText.text = locked ? "Locked" : "Ready";
        if (statusIcon != null) statusIcon.DOColor(locked ? lockedColor : activeColor, 0.3f);
        if (stepLabel != null) stepLabel.color = locked
            ? new Color(0.6f, 0.6f, 0.6f) : new Color(0.1f, 0.1f, 0.1f);
    }

    public void SetActive(bool isActive)
    {
        if (!isActive) return;
        if (statusText != null) statusText.text = "Current";
        if (statusIcon != null) statusIcon.DOColor(activeColor, 0.3f);
        if (rowBackground != null)
            rowBackground.DOColor(new Color(0.20f, 0.55f, 0.90f, 0.12f), 0.3f);
        transform.DOPunchScale(Vector3.one * 0.05f, 0.3f, 3, 0.5f);
    }
    public void UpdateLabel(string label)
    {
        if (stepLabel != null)
            stepLabel.text = label;
    }

    public void MarkComplete()
    {
        if (statusText != null) { statusText.text = "Done"; statusText.color = completeColor; }
        if (statusIcon != null) statusIcon.DOColor(completeColor, 0.4f);
        if (rowBackground != null)
            rowBackground.DOColor(new Color(0.20f, 0.70f, 0.30f, 0.12f), 0.4f);
        transform.DOPunchScale(Vector3.one * 0.08f, 0.4f, 5, 0.5f);
    }
}