using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using TMPro;


public class BandageWrap : MonoBehaviour
{
    public GameObject[] bandageStrips;
    private int currentStripIndex = 0;
    public float wrapDistanceRequirement = 0.15f; // Increased for more intentional movement
    public float timeBetweenStrips = 0.5f; // Half-second delay between strips
    private float lastActivationTime;
    private bool _stepComplete = false; // ← ADD THIS

    [Header("WrappingUI")]
    public GameObject wrappingUIPanel;
    public TextMeshProUGUI wrappingUIText;
    public Image wrapProgressFill;      

    private Vector3 lastWrapPosition;
    private bool isInitialized = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GauzeRoll"))
        {
            lastWrapPosition = other.transform.position;
            isInitialized = true;

            if (wrappingUIPanel != null)
            {
                wrappingUIPanel.SetActive(true);
                wrappingUIText.text = "Start Wrapping!";
                wrapProgressFill.fillAmount = 0f;
            }

            UpdateWrapUI();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isInitialized && other.CompareTag("GauzeRoll") && currentStripIndex < bandageStrips.Length)
        {
            float distanceMoved = Vector3.Distance(other.transform.position, lastWrapPosition);

            // Added Time check so they can't all trigger in one frame
            if (distanceMoved > wrapDistanceRequirement && Time.time > lastActivationTime + timeBetweenStrips)
            {
                bandageStrips[currentStripIndex].SetActive(true);
                currentStripIndex++;

                lastWrapPosition = other.transform.position;
                lastActivationTime = Time.time; // Reset the clock

                Debug.Log("Strip " + (currentStripIndex - 1) + " applied!");

                UpdateWrapUI();

                if (!_stepComplete && currentStripIndex >= bandageStrips.Length)
                {
                    _stepComplete = true;

                    if(wrappingUIPanel != null)
                    {
                        wrappingUIPanel.SetActive(false);
                    }
                    TreatmentManager.Instance?.TryCompleteStep3();
                    Debug.Log("BANDAGE: All strips applied - step complete!");
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("GauzeRoll"))
        {
            isInitialized = false;
        }

        if (wrappingUIPanel != null)
            wrappingUIPanel.SetActive(false);
    }

    public void UpdateWrapUI()
    {
        if(bandageStrips.Length == 0)
        {
            return;
        }

        float progress = (float)currentStripIndex / bandageStrips.Length;
        if(wrapProgressFill != null)
        {
            wrapProgressFill.fillAmount = progress;
        }

        if(wrappingUIText != null)
        {
            if (currentStripIndex < bandageStrips.Length)
            {
                wrappingUIText.text = $"Wrapping... ({currentStripIndex}/{bandageStrips.Length})";
            }
            else
            {
                wrappingUIText.text = "Wrapping Complete!";
            }
        }
    }
}