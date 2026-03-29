using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach to: TrainingManager GameObject
/// Controls the full loop: menu → identify → treat → complete → menu
/// </summary>
public class TrainingManager : MonoBehaviour
{
    public static TrainingManager Instance { get; private set; }


    [Header("Burn Profiles")]
    public BurnProfile[] burnProfiles; // Assign all 3 in Inspector

    [Header("Patient")]
    public DecalProjector patientDecal;
    public BurnZoneData burnZoneData;

    [Header("Phase Panels")]
    public GameObject mainMenuPanel;
    public GameObject educationPanel;
    public GameObject treatmentPanel;
    public GameObject completionPanel;

    [Header("State")]
    public TrainingPhase currentPhase = TrainingPhase.Idle;
    public BurnProfile currentBurn = null;

    void Awake()
    {
        Instance = this;
    }

    

    void Start()
    {
  

        if (educationPanel != null) educationPanel.SetActive(false);
        if (treatmentPanel != null) treatmentPanel.SetActive(false);
        if (completionPanel != null) completionPanel.SetActive(false);
    }

    // ─── Called by MainMenuManager when player picks a burn ──────────────────

    public void StartSessionWithBurn(BurnProfile burn)
    {
        currentBurn = burn;
        Debug.Log("TRAINING: Starting → " + burn.burnDegree);

        ApplyBurnDecal();
        ApplyBurnZoneData();

        currentPhase = TrainingPhase.Identify;
        Debug.Log("TRAINING: Phase → Identify. Point at burn to inspect.");
    }

    // ─── Called by StartTreatmentButton ──────────────────────────────────────

    public void StartTreatmentPhase()
    {
        currentPhase = TrainingPhase.Treatment;
        Debug.Log("TRAINING: Phase → Treatment");

        if (educationPanel != null) educationPanel.SetActive(false);

        TreatmentManager.Instance?.BeginTreatment(currentBurn);
    }

    // ─── Called by TreatmentManager when all steps complete ──────────────────

    public void OnTreatmentComplete()
    {
        currentPhase = TrainingPhase.Complete;
        Debug.Log("TRAINING: Phase → Complete");

        CompletionUI completionUI = completionPanel?.GetComponent<CompletionUI>();
        completionUI?.Show(currentBurn);

        if (completionPanel != null) completionPanel.SetActive(true);


    }

    // ─── Called by CompletionUI Try Another button ────────────────────────────

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }



    // ─── Internal ─────────────────────────────────────────────────────────────

    void ApplyBurnDecal()
    {
        if (patientDecal == null || currentBurn?.decalMaterial == null) return;
        patientDecal.material = currentBurn.decalMaterial;
    }

    void ApplyBurnZoneData()
    {
        if (burnZoneData == null || currentBurn == null) return;
        burnZoneData.ApplyFromProfile(currentBurn);
    }
}

public enum TrainingPhase
{
    Idle,
    Identify,
    Treatment,
    Complete
}