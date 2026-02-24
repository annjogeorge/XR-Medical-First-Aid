using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI; // Needed for the Begin Button

public class CPRScenarioManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject symptomPanel;   // Panel 1
    public GameObject procedurePanel; // Panel 2
    public GameObject pulsePanel;     // Panel 3
    public GameObject infoPanel;      // Panel 4 (The CPR HUD)

    [Header("Pulse Check Elements")]
    public TextMeshProUGUI pulseStatusText;
    public GameObject beginCprButton; // The button that appears after pulse check

    [Header("Logic Reference")]
    public CPRCompressionLogic cprLogic;

    void Start()
    {
        // 1. Initial UI States
        symptomPanel.SetActive(true);
        procedurePanel.SetActive(false);
        pulsePanel.SetActive(false);
        infoPanel.SetActive(false);

        // 2. Hide the Begin button initially
        if (beginCprButton != null) beginCprButton.SetActive(false);

        // 3. Keep CPR logic disabled until the user is ready
        if (cprLogic != null) cprLogic.enabled = false;
    }

    // Called by Button on Symptom Panel
    public void OnProceedToProcedure()
    {
        symptomPanel.SetActive(false);
        procedurePanel.SetActive(true);
    }

    // Called by Button on Procedure Panel
    public void OnProceedToPulseCheck()
    {
        procedurePanel.SetActive(false);
        pulsePanel.SetActive(true);
        pulseStatusText.text = "Touch the neck to check for a pulse";
    }

    // Triggered by the hand touching the Neck
    public void OnPulseChecked()
    {
        StartCoroutine(DynamicPulseRoutine());
    }

    IEnumerator DynamicPulseRoutine()
    {
        pulseStatusText.text = "Checking pulse...";
        yield return new WaitForSeconds(2.0f);

        pulseStatusText.text = "<color=red>NO PULSE DETECTED!</color>";
        yield return new WaitForSeconds(1.5f);

        pulseStatusText.text = "Victim is unresponsive.\nGet ready to start CPR.";

        // Show the manual Start button
        if (beginCprButton != null) beginCprButton.SetActive(true);
    }

    // Called by clicking the "BEGIN CPR" button
    public void StartLiveCPR()
    {
        pulsePanel.SetActive(false);
        infoPanel.SetActive(true);

        // Start the timer and metronome
        if (cprLogic != null) cprLogic.enabled = true;
    }
}