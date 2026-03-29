using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// Attach to: the clothing/jewellery GameObject on the patient
public class WatchRemove : MonoBehaviour
{
    private XRSimpleInteractable _interactable;

    void Awake()
    {
        _interactable = GetComponent<XRSimpleInteractable>();
        if (_interactable == null)
            _interactable = gameObject.AddComponent<XRSimpleInteractable>();

        _interactable.selectEntered.AddListener(OnClicked);
    }

    void OnClicked(SelectEnterEventArgs args)
    {
        // Hide the clothing
        if (TrainingManager.Instance?.currentPhase != TrainingPhase.Treatment) return;
        if (TreatmentManager.Instance?.currentStep != 1) return;
        gameObject.SetActive(false);
        TreatmentManager.Instance?.TryCompleteStep1();

    }
}