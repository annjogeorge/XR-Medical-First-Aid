using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
using System.Collections.Generic;

/// <summary>
/// Attach to: BurnZone GameObject (child of Forearm_Bone)
/// Stores burn data and individual annotation anchor points on the burn surface
/// </summary>
public class BurnZoneData : MonoBehaviour
{
    [Header("Burn Classification")]
    public string burnDegree = "Second Degree";
    public string burnCause = "Thermal Contact";

    [Header("Description")]
    [TextArea(2, 5)]
    public string burnDescription = "Second-degree burns affect both the epidermis and dermis. Characterised by blistering, redness, and significant pain response.";

    [Header("Characteristics with Anchor Points")]
    [Tooltip("Each entry is a characteristic label + the world position on the burn it points to")]
    public List<BurnAnnotation> annotations;

    [Header("Decal Highlight")]
    public DecalProjector burnDecal;
    public float highlightOpacity = 1f;
    public float defaultOpacity = 0.85f;
    public float fadeDuration = 0.3f;
    void Awake()
    {
        if (annotations == null)
        {
            Debug.LogError("ANNOTATIONS IS NULL at Awake!");
        }
        else
        {
            Debug.Log("Annotations at Awake: " + annotations.Count);
        }
    }

    void Start()
    {
        if (burnDecal != null)
            burnDecal.fadeFactor = defaultOpacity;
    }

    /// <summary>
    /// Returns the world space position of an annotation point
    /// Call this in LateUpdate to account for bone animation
    /// </summary>
    public Vector3 GetAnnotationWorldPosition(int index)
    {
        if (index < 0 || index >= annotations.Count) return transform.position;
        return transform.TransformPoint(annotations[index].localOffset);
    }

    public void ActivateHighlight()
    {
        if (burnDecal == null) return;
        DOTween.To(() => burnDecal.fadeFactor, x => burnDecal.fadeFactor = x, highlightOpacity, fadeDuration)
            .SetEase(Ease.OutCubic);
    }

    public void DeactivateHighlight()
    {
        if (burnDecal == null) return;
        DOTween.To(() => burnDecal.fadeFactor, x => burnDecal.fadeFactor = x, defaultOpacity, fadeDuration)
            .SetEase(Ease.OutCubic);
    }

    void OnDrawGizmosSelected()
    {
        // Draw each annotation anchor in the editor so you can position them
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