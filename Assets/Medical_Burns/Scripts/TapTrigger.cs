using UnityEngine;

public class TapTrigger : MonoBehaviour
{
    public Animator patientAnimator; // Drag man_model_dec here
    public Animator handleAnimator;  // Drag Left_Handle here
    public ParticleSystem waterParticles;
    public NPCHandGuide npcHandGuide; // Reference to the NPC hand guide script
    public bool isWaterOn = false; // Track water state
    private void Start()
    {
        waterParticles.Stop(); // Ensure water is off at the
       
    }

    private void OnTriggerEnter(Collider other)
    {
        // Checks for the VR hand tag or name
        if (other.CompareTag("PlayerHand") || other.name.Contains("Hand"))
        {
            waterParticles.Play();
            //patientAnimator.SetTrigger("StartHeal");
            handleAnimator.SetTrigger("TurnTap");
            isWaterOn = true;
            TreatmentManager manager = FindObjectOfType<TreatmentManager>();
            if (manager != null)
            {
                manager.WaterOn = true;
            }
        }
    }
}