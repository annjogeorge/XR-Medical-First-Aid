using UnityEngine;
using TMPro; 

public class WoundTreatment : MonoBehaviour
{
    [Header("Visual Elements")]
    public GameObject bloodDecal;
    public GameObject bandageObject;
    public TextMeshProUGUI instructionText; // Drag your TextMeshPro object here

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip splashSound;

    private int step = 0; 

    private void Start()
    {
        if (bandageObject != null) bandageObject.SetActive(false);
        instructionText.text = "Step 1: Apply firm pressure with Gauze.";
    }

    void OnTriggerEnter(Collider other)
    {
        // STEP 1: GAUZE
        if (other.CompareTag("Gauze") && step == 0)
        {
            instructionText.text = "Holding pressure...";
            Invoke("StopBleeding", 3.0f); 
        }
        // STEP 2: WATER
        else if (other.CompareTag("Water") && step == 1)
        {
            CleanWound();
            PlaySound(splashSound);
        }
        // STEP 3: OINTMENT
        else if (other.CompareTag("Ointment") && step == 2)
        {
            ApplyOintment();
        }
        // STEP 4: BANDAGE
        else if (other.CompareTag("BandageRoll") && step == 3)
        {
            FinishTreatment();
        }
    }

    void StopBleeding()
    {
        Renderer rend = bloodDecal.GetComponent<Renderer>();
        if (rend != null) rend.material.color = new Color(0.3f, 0, 0);
        step = 1;
        instructionText.text = "Step 2: Rinse the wound with Water.";
    }

    void CleanWound()
    {
        Renderer rend = bloodDecal.GetComponent<Renderer>();
        if (rend != null) {
            Color c = rend.material.color;
            c.a = 0.3f;
            rend.material.color = c;
        }
        step = 2;
        instructionText.text = "Step 3: Apply Ointment to protect the area.";
    }

    void ApplyOintment()
    {
        step = 3;
        instructionText.text = "Step 4: Cover with a sterile Bandage.";
    }

    void FinishTreatment()
    {
        bloodDecal.SetActive(false);
        bandageObject.SetActive(true);
        step = 4;
        instructionText.text = "Treatment Complete! Great job.";
    }

    void PlaySound(AudioClip clip) {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
    }
}