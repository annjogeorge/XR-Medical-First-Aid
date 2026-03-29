using UnityEngine;
using TMPro;
using UnityEngine.Rendering.Universal;

public class WoundTreatment : MonoBehaviour
{
    [Header("Decal Projectors")]
    public DecalProjector woundDecal;     // wound decal
    public GameObject bandaidDecal;       // bandaid decal object

    [Header("UI")]
    public TextMeshProUGUI instructionText;

    private Material woundMaterial;
    private int step = 0;

    void Start()
    {
        instructionText.text = "Step 1: Use Gauze to soak the wound.";

        // Get and instance material so we can modify it
        if (woundDecal != null)
        {
            woundMaterial = Instantiate(woundDecal.material);
            woundDecal.material = woundMaterial;
        }

        // Hide bandaid initially
        if (bandaidDecal != null)
            bandaidDecal.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        // STEP 1: GAUZE
        if (other.CompareTag("Gauze") && step == 0)
        {
            FadeWound(0.7f); // slight fade
            step = 1;
            instructionText.text = "Step 2: Clean with Water.";
        }

        // STEP 2: WATER
        else if (other.CompareTag("Water") && step == 1)
        {
            FadeWound(0.4f); // more fade
            step = 2;
            instructionText.text = "Step 3: Apply Ointment.";
        }

        // STEP 3: OINTMENT
        else if (other.CompareTag("Ointment") && step == 2)
        {
            FadeWound(0.2f); // almost gone
            step = 3;
            instructionText.text = "Step 4: Apply Bandaid.";
        }

        // STEP 4: BANDAID
        else if (other.CompareTag("Bandaid") && step == 3)
        {
            ApplyBandaid(other);
        }

        else
        {
            instructionText.text = "Wrong step! Follow the correct order.";
        }
    }

    // 🎨 Fade decal using URP BaseColor
    void FadeWound(float alpha)
    {
        if (woundMaterial != null)
        {
            woundMaterial.SetColor("_BaseColor", new Color(1, 1, 1, alpha));
        }
    }

    // 🩹 Final Step
    void ApplyBandaid(Collider other)
    {
        // Hide wound completely
        if (woundDecal != null)
            woundDecal.gameObject.SetActive(false);

        // Show bandaid decal
        if (bandaidDecal != null)
            bandaidDecal.SetActive(true);

        // Remove bandaid from kit
        Destroy(other.gameObject);

        step = 4;
        instructionText.text = "Treatment Complete! Great job.";
    }
}