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
    private Coroutine _hideUICoroutine;


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
            if (_hideUICoroutine != null) StopCoroutine(_hideUICoroutine);
            lastWrapPosition = other.transform.position;
            isInitialized = true;


        }
    }

    public void UpdateWrapUI()
    {
        if (bandageStrips.Length == 0) return;

        float progress = (float)currentStripIndex / bandageStrips.Length;

        // Show panel only after first strip is applied
        if (currentStripIndex >= 1 && currentStripIndex < bandageStrips.Length)
        {
            if (wrappingUIPanel != null) wrappingUIPanel.SetActive(true);
            if (wrapProgressFill != null) wrapProgressFill.fillAmount = progress;
            if (wrappingUIText != null)
                wrappingUIText.text = $"Wrapping... ({currentStripIndex}/{bandageStrips.Length})";
        }
        // Hide panel when all strips are done
        else if (currentStripIndex >= bandageStrips.Length)
        {
            if (wrappingUIText != null) wrappingUIText.text = "Wrapping Complete!";
            if (wrapProgressFill != null) wrapProgressFill.fillAmount = 1f;

            // Small delay then hide
            if (_hideUICoroutine != null) StopCoroutine(_hideUICoroutine);
            _hideUICoroutine = StartCoroutine(HideUIDelayed());
        }
    }

    IEnumerator HideUIDelayed()
    {
        yield return new WaitForSeconds(1.5f); // show "Complete" for 1.5s then hide
        if (wrappingUIPanel != null) wrappingUIPanel.SetActive(false);
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

                if (currentStripIndex == 1)
                    TreatmentManager.Instance?.ghostHand?.Hide(); // ← add this

                Debug.Log("Strip " + (currentStripIndex - 1) + " applied!");

                UpdateWrapUI();

                if (!_stepComplete && currentStripIndex >= bandageStrips.Length)
                {
                    _stepComplete = true;

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
            // Only hide UI if wrapping never started
            if (currentStripIndex == 0 && wrappingUIPanel != null)
                wrappingUIPanel.SetActive(false);
        }
    }


}