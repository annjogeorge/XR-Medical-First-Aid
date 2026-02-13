using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PulsePointDetector : MonoBehaviour
{
    public CPRScenarioManager scenarioManager;
    public ActionBasedController rightController;

    private void OnTriggerEnter(Collider other)
    {
        // Check if Right Hand touches neck
        if (other.CompareTag("PlayerHand") && other.gameObject == rightController.gameObject)
        {
            rightController.SendHapticImpulse(0.6f, 0.2f);
            scenarioManager.OnPulseChecked();
            gameObject.SetActive(false); // Disable trigger after use
        }
    }
}