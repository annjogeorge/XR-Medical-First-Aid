using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using DG.Tweening;

/// <summary>
/// Attach to: each BurnZone GameObject (alongside your XRSimpleInteractable)
/// 
/// When the player ray-selects a burn zone:
///   1. XROrigin smoothly teleports to a fixed close-up ViewPoint
///   2. UIAnimator.ShowPanel() is called
/// When the player deselects / panel closes:
///   1. XROrigin returns to the original position
/// 
/// Setup:
///   - Add an empty child GameObject to each BurnZone called "ViewPoint"
///     and position it where you want the camera to land (e.g. 0.5m in front of burn)
///   - Assign xrOrigin, uiAnimator, and burnZoneData in the Inspector
/// </summary>
public class BurnZoneCameraZoom : MonoBehaviour
{
    [Header("References")]
    public Transform xrOrigin;               // Drag your XROrigin GameObject here
    public UIAnimator uiAnimator;            // Your InspectionPanel's UIAnimator
    public BurnZoneData burnZoneData;        // This burn zone's data

    [Header("View Point")]
    [Tooltip("Place an empty child GameObject here — position it where the camera should land")]
    public Transform viewPoint;              // Child empty: positioned in front of the burn

    [Header("Teleport Animation")]
    public float zoomDuration = 0.5f;
    public Ease zoomEase = Ease.OutCubic;
    public float returnDuration = 0.4f;
    public Ease returnEase = Ease.InOutCubic;

    // Internal
    private Vector3 _originStartPos;
    private Quaternion _originStartRot;
    private bool _isZoomed = false;
    private XRSimpleInteractable _interactable;

    void Awake()
    {
        _interactable = GetComponent<XRSimpleInteractable>();

        if (_interactable == null)
        {
            Debug.LogWarning($"[BurnZoneCameraZoom] No XRSimpleInteractable found on {gameObject.name}");
            return;
        }

        _interactable.selectEntered.AddListener(OnSelected);
        _interactable.selectExited.AddListener(OnDeselected);
    }

    void OnDestroy()
    {
        if (_interactable == null) return;
        _interactable.selectEntered.RemoveListener(OnSelected);
        _interactable.selectExited.RemoveListener(OnDeselected);
    }

    // ─── Interaction Callbacks ────────────────────────────────────────────────

    private void OnSelected(SelectEnterEventArgs args)
    {
        if (_isZoomed) return;

        // Cache where the player currently is so we can return later
        _originStartPos = xrOrigin.position;
        _originStartRot = xrOrigin.rotation;

        ZoomIn();
    }

    private void OnDeselected(SelectExitEventArgs args)
    {
        if (!_isZoomed) return;
        ZoomOut();
    }

    // ─── Zoom Logic ───────────────────────────────────────────────────────────

    private void ZoomIn()
    {
        if (viewPoint == null)
        {
            Debug.LogError($"[BurnZoneCameraZoom] No ViewPoint assigned on {gameObject.name}. " +
                           "Create an empty child GameObject and assign it.");
            return;
        }

        _isZoomed = true;

        // Kill any in-progress tweens on xrOrigin
        xrOrigin.DOKill();

        // Smoothly move XROrigin to the viewpoint
        xrOrigin.DOMove(viewPoint.position, zoomDuration)
                .SetEase(zoomEase);

        // Smoothly rotate to face the burn zone
        xrOrigin.DORotateQuaternion(viewPoint.rotation, zoomDuration)
                .SetEase(zoomEase)
                .OnComplete(() =>
                {
                    // Show the inspection panel once we've arrived
                    if (uiAnimator != null && burnZoneData != null)
                        uiAnimator.ShowPanel(burnZoneData);
                });
    }

    private void ZoomOut()
    {
        _isZoomed = false;

        // Hide the panel immediately
        if (uiAnimator != null)
            uiAnimator.HidePanel();

        // Kill any in-progress tweens
        xrOrigin.DOKill();

        // Return to original position
        xrOrigin.DOMove(_originStartPos, returnDuration)
                .SetEase(returnEase);

        xrOrigin.DORotateQuaternion(_originStartRot, returnDuration)
                .SetEase(returnEase);
    }

    // ─── Optional: Public trigger for external callers ────────────────────────

    /// <summary>Call this manually if you need to trigger zoom from another script.</summary>
    public void TriggerZoomIn() => ZoomIn();
    public void TriggerZoomOut() => ZoomOut();
}