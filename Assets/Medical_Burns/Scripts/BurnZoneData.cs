using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
using System.Collections.Generic;

public class BurnZoneData : MonoBehaviour
{
    [Header("Runtime Data — set by TrainingManager")]
    public string burnDegree;
    public string burnCause;
    public string burnDescription;
    public List<BurnAnnotation> annotations = new List<BurnAnnotation>();

    [Header("Decal Highlight")]
    public DecalProjector burnDecal;
    public float highlightOpacity = 1f;
    public float defaultOpacity = 0.85f;
    public float fadeDuration = 0.3f;

    void Start()
    {
        if (burnDecal != null)
            burnDecal.fadeFactor = defaultOpacity;
    }

    // Called by TrainingManager when burn is selected
    public void ApplyFromProfile(BurnProfile profile)
    {
        burnDegree = profile.burnDegree;
        burnCause = profile.burnCause;
        burnDescription = profile.whatItLooks;

        annotations.Clear();
        foreach (var a in profile.annotations)
            annotations.Add(new BurnAnnotation
            {
                label = a.label,
                localOffset = a.localOffset
            });
    }

    public Vector3 GetAnnotationWorldPosition(int index)
    {
        if (index < 0 || index >= annotations.Count) return transform.position;
        return transform.TransformPoint(annotations[index].localOffset);
    }

    public void ActivateHighlight()
    {
        if (burnDecal == null) return;
        DOTween.To(() => burnDecal.fadeFactor,
            x => burnDecal.fadeFactor = x,
            highlightOpacity, fadeDuration).SetEase(Ease.OutCubic);
    }

    public void DeactivateHighlight()
    {
        if (burnDecal == null) return;
        DOTween.To(() => burnDecal.fadeFactor,
            x => burnDecal.fadeFactor = x,
            defaultOpacity, fadeDuration).SetEase(Ease.OutCubic);
    }

    void OnDrawGizmosSelected()
    {
        if (annotations == null) return;
        for (int i = 0; i < annotations.Count; i++)
        {
            Vector3 worldPos = transform.TransformPoint(annotations[i].localOffset);
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(worldPos, 0.005f);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(worldPos + Vector3.up * 0.01f, annotations[i].label);
#endif
        }
    }
}

/// <summary>
/// One annotation: a label and where on the burn it points to
/// </summary>
[System.Serializable]
public class BurnAnnotation
{
    public string label;
    public Vector3 localOffset; // Offset from BurnZone center in local space
}