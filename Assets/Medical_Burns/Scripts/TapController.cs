using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapController : MonoBehaviour
{
    public ParticleSystem waterParticle;
    public Animator handleAnimator;
    private bool isOn = false;

    // Start is called before the first frame update
    //void Start()
    //{
    //    handleAnimator = GetComponent<Animator>();
    //    var emission = waterParticle.emission;
    //    emission.enabled = false;
    //}

    // Update is called once per frame
   public void ToggleTap()
    {
        isOn = !isOn;
        if(isOn)
        {
            handleAnimator.Play("Handle_Open");
            waterParticle.Play();
        }
        else
        {
            handleAnimator.Play("Handle_Off");
            waterParticle.Play();
        }
            
    }
}
