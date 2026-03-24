using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// ScriptableObject — create one asset per burn degree
/// Right Click in Assets → Create → Training → Burn Profile
/// Create 3 assets: FirstDegreeBurn, SecondDegreeBurn, ThirdDegreeBurn
/// </summary>
[CreateAssetMenu(fileName = "BurnProfile", menuName = "Training/Burn Profile")]
public class BurnProfile : ScriptableObject
{
    [Header("Identity")]
    public string burnDegree = "Second Degree";
    public string burnCause = "Thermal Contact";

    [Header("Education Phase")]
    [TextArea(2, 4)] public string whatItLooks = "Blisters, moist red skin, wet appearance.";
    [TextArea(2, 4)] public string causes = "Scalding water, flames, chemical contact.";
    [TextArea(2, 4)] public string symptoms = "Severe pain, swelling, fluid-filled blisters.";
    [TextArea(2, 4)] public string emergency = "Call 999 if larger than the palm of your hand.";

    [Header("Annotation Points")]
    public BurnAnnotation[] annotations;

    [Header("Treatment Steps (exactly 3)")]
    public TreatmentStep[] treatmentSteps;

    [Header("Decal")]
    [Tooltip("Decal Projector material for this burn type")]
    public Material decalMaterial;

    [Header("Emergency")]
    public bool requiresImmediate999 = false;
}

[System.Serializable]
public class TreatmentStep
{
    public string title;
    [TextArea(1, 3)]
    public string instruction;
    public StepType stepType;
}

public enum StepType
{
    RemoveWatch,
    CoolWater,
    ApplyBandage,
    ApplyClingFilm,
    ApplyMoisturiser,
    Call999,
    DoNotRemoveClothing,
    CoverLoosely
}