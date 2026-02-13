using UnityEngine;
using TMPro;
using System.Collections;

public class CPRScenarioManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pulsePanel;
    public GameObject symptomPanel;
    public GameObject procedurePanel;
    public GameObject infoPanel;

    [Header("Dynamic Text")]
    public TextMeshProUGUI pulseStatusText;

    [Header("Logic")]
    public CPRCompressionLogic cprLogic;

    void Start()
    {
        pulsePanel.SetActive(true);
        symptomPanel.SetActive(false);
        procedurePanel.SetActive(false);
        infoPanel.SetActive(false);
        pulseStatusText.text = "Touch the neck to check for a pulse";
        cprLogic.enabled = false; // Keep CPR logic off at start
    }

    public void OnPulseChecked() { StartCoroutine(DynamicPulseRoutine()); }

    IEnumerator DynamicPulseRoutine()
    {
        pulseStatusText.text = "Checking pulse...";
        pulseStatusText.color = Color.yellow;
        yield return new WaitForSeconds(2.0f);

        pulseStatusText.text = "<color=red>NO PULSE DETECTED!</color>";
        yield return new WaitForSeconds(2.0f);

        pulsePanel.SetActive(false);
        symptomPanel.SetActive(true);
    }

    public void OnProceedToProcedure()
    {
        symptomPanel.SetActive(false);
        procedurePanel.SetActive(true);
    }

    public void OnStartCPR()
    {
        procedurePanel.SetActive(false);
        infoPanel.SetActive(true);
        cprLogic.enabled = true; // Wake up the CPR script
    }
}