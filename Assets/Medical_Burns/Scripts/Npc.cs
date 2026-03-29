using UnityEngine;
using System.Collections;
using UnityEngine.Animations.Rigging;
using UnityEngine.XR.Interaction.Toolkit;

public class NPCHandGuide : MonoBehaviour
{
    [Header("IK Setup")]
    public Transform ikTarget;
    public TwoBoneIKConstraint ikConstraint;

    [Header("Settings")]
    public Transform npcHand;
    public Transform tapZone;
    public float movementSpeed = 1.5f;
    public float successRadius = 0.4f;

    [Header("Spine Setup")]
    public Transform spineTarget;
    public Transform npcSpine;
    public float maxSpineLean = 0.3f;
    public float spineBendStartDistance = 2f;
    public float spineTiltAngle = 30f; // ← was missing, now exposed in Inspector

    [Header("Spine Rigging")]
    public MultiParentConstraint spineConstraint;

    [Header("References")]
    public TreatmentManager treatmentManager;

    private XRGrabInteractable grabInteractable;
    private bool isMovingToTap = false;
    public  bool isHandInPosition = false;

    private bool isCoolingActive = false;
    private float initialDistanceToTap;

    private Vector3 spineStartPosition;
    private Quaternion spineStartRotation;
    private Vector3 spineForwardAtStart;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.trackPosition = false;
        grabInteractable.trackRotation = false;
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        if (TrainingManager.Instance?.currentPhase != TrainingPhase.Treatment) return;

        var profile = TreatmentManager.Instance?.currentBurnProfile;
        if (profile == null) return;

        int coolStep = -1;
        for (int i = 0; i < profile.treatmentSteps.Length; i++)
            if (profile.treatmentSteps[i].stepType == StepType.CoolWater)
                coolStep = i + 1;

        if (TreatmentManager.Instance?.currentStep != coolStep) return;

        if (!isMovingToTap)
        {
            ikTarget.position = npcHand.position;
            ikTarget.rotation = npcHand.rotation; // lock rotation at start

            spineStartPosition = npcSpine.position;
            spineStartRotation = npcSpine.rotation;
            spineForwardAtStart = npcSpine.forward;

            initialDistanceToTap = Vector3.Distance(npcHand.position, tapZone.position);
            isMovingToTap = true;

            StartCoroutine(EngageRigSmoothly(0.5f));
            // REMOVED SmoothRotationToTarget coroutine
            Debug.Log("Fixed route started.");
        }
    }

    private IEnumerator SmoothRotationToTarget(float duration)
    {
        float elapsed = 0f;
        Quaternion startRotation = ikTarget.rotation; // use ikTarget not npcHand
        Quaternion endRotation = tapZone.rotation;

        // Force shortest path — prevents 180 flip
        if (Quaternion.Dot(startRotation, endRotation) < 0f)
        {
            endRotation = new Quaternion(
                -endRotation.x,
                -endRotation.y,
                -endRotation.z,
                -endRotation.w
            );
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            ikTarget.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }

        ikTarget.rotation = endRotation;
    }

    private IEnumerator EngageRigSmoothly(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            ikConstraint.weight = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }
        ikConstraint.weight = 1f;
    }

    void Update()
    {
        if (!isMovingToTap || tapZone == null) return;

        Vector3 overshootTarget = tapZone.position +
            (tapZone.position - npcHand.position).normalized * 0.1f;

        ikTarget.position = Vector3.MoveTowards(
            ikTarget.position,
            overshootTarget,
            movementSpeed * Time.deltaTime
        );

        if (spineConstraint != null && spineTarget != null)
        {
            float currentDist = Vector3.Distance(ikTarget.position, tapZone.position);
            float spineProgress = Mathf.Clamp01(1f - (currentDist / spineBendStartDistance));

            spineConstraint.weight = spineProgress;

            spineTarget.position = Vector3.Lerp(
                spineStartPosition,
                spineStartPosition + (spineForwardAtStart * maxSpineLean),
                spineProgress
            );

            spineTarget.rotation = Quaternion.Lerp(
                spineStartRotation,
                spineStartRotation * Quaternion.Euler(spineTiltAngle, 0f, 0f),
                spineProgress
            );
        }

        float dist = Vector3.Distance(npcHand.position, tapZone.position);

        if (dist < successRadius)
        {

            if (!isCoolingActive)
            {
                StartCooling();
            }
        }
    }

    void StartCooling()
    {
        if (treatmentManager == null)
        {
            Debug.LogError("TreatmentManager is missing!");
            return;
        }

        isCoolingActive = true;
        isHandInPosition = true;
        treatmentManager.HandInPosition = true;
        Debug.Log("Hand at tap - cooling triggered");
    }
}
