using UnityEngine;

public class SpineBend : MonoBehaviour
{
    public Transform handTarget;        // Your ikTarget
    public Transform tapZone;
    public float maxBendAngle = 20f;    // How far forward the NPC leans
    private Quaternion originalRotation;

    void Start()
    {
        originalRotation = transform.localRotation;
    }

    void LateUpdate() // LateUpdate so it runs after Animator
    {
        if (handTarget == null || tapZone == null) return;

        // How close is the hand to the tap (0 to 1)
        float progress = 1f - Mathf.Clamp01(
            Vector3.Distance(handTarget.position, tapZone.position) / 1f
        );

        // Lean forward as hand gets closer to tap
        Quaternion targetRotation = originalRotation *
            Quaternion.Euler(maxBendAngle * progress, 0f, 0f);

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            Time.deltaTime * 2f
        );
    }
}
