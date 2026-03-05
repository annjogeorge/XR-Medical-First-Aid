using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BurnCooling : MonoBehaviour
{
    public DecalProjector burnDecal;
    public float coolingRate = 0.1f;
    public bool isWaterRunning = false;

    public bool isFullyCooled => burnDecal != null && burnDecal.fadeFactor <= 0.05f;

    private void OnParticleCollision(GameObject other)
    {
        // 1. Check if THIS object has the "BurnArea" tag
        if (this.gameObject.CompareTag("BurnArea"))
        {
            // 2. Only proceed if water is on and we have a decal to fade
            if (isWaterRunning && burnDecal != null && burnDecal.fadeFactor > 0)
            {
                // Note: Using a multiplier instead of Time.deltaTime can be more 
                // consistent for particle collisions, but we'll keep your logic:
                burnDecal.fadeFactor -= coolingRate * Time.deltaTime;

                if (isFullyCooled)
                {
                    burnDecal.fadeFactor = 0;
                    Debug.Log("Burn is fully cooled!");
                }
            }
        }
        else
        {
            // Helpful debug to see if your tag is missing on the bone
            Debug.Log("Particle hit " + gameObject.name + " but it is not tagged BurnArea");
        }
    }
}