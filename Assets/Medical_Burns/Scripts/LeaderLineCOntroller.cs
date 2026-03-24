using UnityEngine;

/// <summary>
/// Attach to: LeaderLine GameObject (child of Panel)
/// Calls UIAnimator.UpdateLeaderLines every LateUpdate
/// so lines stay attached after bone animation
/// </summary>
public class LeaderLineController : MonoBehaviour
{
    [Header("References")]
    public BurnZoneData burnZoneData;
    public UIAnimator uiAnimator;

    void LateUpdate()
    {
        if (uiAnimator == null) return;

        // Use the SAME data the UI is using
        uiAnimator.UpdateLeaderLines(uiAnimator.GetCurrentData());
    }
}