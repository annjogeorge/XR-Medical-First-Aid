using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.XR.Interaction.Toolkit; // Required

public class PatientArmGuide : MonoBehaviour
{
    public TwoBoneIKConstraint armConstraint;
    public MultiParentConstraint bodyConstraint;
    public XRSimpleInteractable simpleInteractable; // Drag the hand interactable here

    public float followSpeed = 20f;
    private Rigidbody rb;
    private Transform playerHandTransform;
    private bool isAttached = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate; // Important for smooth VR movement

        // Force weights to 0 on start to prevent the "folding" glitch
        if (armConstraint != null) armConstraint.weight = 0f;
        if (bodyConstraint != null) bodyConstraint.weight = 0f;

        if (simpleInteractable != null)
        {
            simpleInteractable.selectEntered.AddListener(OnGrab);
            simpleInteractable.selectExited.AddListener(OnRelease);
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        // 'args.interactorObject' is the player's controller/hand
        playerHandTransform = args.interactorObject.transform;
        isAttached = true;
        armConstraint.weight = 1f;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isAttached = false;
        playerHandTransform = null;
        armConstraint.weight = 0f;
        bodyConstraint.weight = 0f;
    }

    void OnDestroy()
    {
        if (simpleInteractable != null)
        {
            simpleInteractable.selectEntered.RemoveListener(OnGrab);
            simpleInteractable.selectExited.RemoveListener(OnRelease);
        }
    }
    void FixedUpdate()
    {
        if (isAttached && playerHandTransform != null)
        {
            // Move the Red Box toward the player's hand using Physics
            // This is what makes the arm stop when it hits the Spine Capsule Collider
            Vector3 targetPosition = playerHandTransform.position;
            rb.MovePosition(Vector3.Lerp(rb.position, targetPosition, Time.fixedDeltaTime * followSpeed));
            rb.MoveRotation(playerHandTransform.rotation);

            // Update Body Lean based on how far the hand is from the body
            if (bodyConstraint != null)
            {
                // Adjust this logic based on your preferred leaning distance
                float dist = Vector3.Distance(rb.position, armConstraint.data.root.position);
                bodyConstraint.weight = Mathf.Clamp01((dist - 0.4f) * 2f);
            }
        }

    }
}