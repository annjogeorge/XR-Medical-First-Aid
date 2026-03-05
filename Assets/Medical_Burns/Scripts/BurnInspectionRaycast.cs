using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using DG.Tweening;

/// <summary>
/// Attach to: Main Camera (under XR Origin > Camera Offset > Main Camera)
/// Handles controller raycast detection of burn zones and triggers zoom + UI
/// </summary>
public class BurnInspectionRaycast : MonoBehaviour
{
    [Header("XR Input")]
    public XRRayInteractor rightHandRay; // Drag your Right Hand Ray Interactor here

    [Header("Reticle")]
    public UnityEngine.UI.Image reticleFill; // The circular progress ring UI Image
    public float lockTime = 2f;             // Seconds to hold on burn before triggering

    [Header("Camera Zoom")]
    public Camera vrCamera;                 // Drag Main Camera here
    public float zoomedFOV = 55f;           // Target FOV when zoomed (don't go below 50)
    public float defaultFOV = 90f;
    public float zoomDuration = 0.25f;

    [Header("References")]
    public UIAnimator uiAnimator;           // Drag InspectionPanel here

    private bool _isZoomed = false;
    private BurnZoneData _currentBurnData = null;

    void Start()
    {
        if (vrCamera == null)
            vrCamera = Camera.main;

        if (reticleFill != null)
            reticleFill.fillAmount = 0f;
    }

    void Update()
    {
        if (rightHandRay == null)
        {
            Debug.LogError("BURN SYSTEM: Right Hand Ray is NULL - drag Right Controller into the slot");
            return;
        }

        if (rightHandRay.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            Debug.Log("BURN SYSTEM: Ray hitting -> " + hit.collider.gameObject.name);

            BurnZoneData burnData = hit.collider.GetComponent<BurnZoneData>();

            if (burnData != null)
            {
                Debug.Log("BURN SYSTEM: BurnZoneData FOUND - filling reticle");
                _currentBurnData = burnData;

                if (reticleFill != null)
                    reticleFill.fillAmount += Time.deltaTime / lockTime;
                else
                    Debug.LogWarning("BURN SYSTEM: Reticle Fill is NULL - assign the Image in inspector");

                if (reticleFill != null && reticleFill.fillAmount >= 1f)
                {
                    TriggerInspection();
                }
                return;
            }
            else
            {
                Debug.LogWarning("BURN SYSTEM: Hit " + hit.collider.gameObject.name + " but NO BurnZoneData on it");
            }
        }
        else
        {
            Debug.Log("BURN SYSTEM: Ray not hitting anything");
        }

        ResetReticle();
    }

    void TriggerInspection()
    {
        if (_isZoomed) return;
        _isZoomed = true;

        // Zoom camera in
        DOTween.To(
            () => vrCamera.fieldOfView,
            x => vrCamera.fieldOfView = x,
            zoomedFOV,
            zoomDuration
        ).SetEase(Ease.OutCubic);

        // Show the UI panel with burn data
        if (uiAnimator != null && _currentBurnData != null)
            uiAnimator.ShowPanel(_currentBurnData);

        ResetReticle();
    }

    public void ResetInspection()
    {
        _isZoomed = false;
        _currentBurnData = null;

        // Zoom back out
        DOTween.To(
            () => vrCamera.fieldOfView,
            x => vrCamera.fieldOfView = x,
            defaultFOV,
            zoomDuration
        ).SetEase(Ease.OutCubic);

        if (uiAnimator != null)
            uiAnimator.HidePanel();
    }

    void ResetReticle()
    {
        if (reticleFill != null)
            reticleFill.fillAmount = 0f;
    }
}