using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoisturiserTrigger : MonoBehaviour
{
    
    private MoisturiserApplicator _applicator;

    void Start()
    {
        _applicator = GetComponentInParent<MoisturiserApplicator>();
    }

    void OnTriggerEnter(Collider other)
    {
        _applicator?.OnBurnEnter(other);
    }

    void OnTriggerStay(Collider other)
    {
        _applicator?.OnBurnStay(other);
    }

    void OnTriggerExit(Collider other)
    {
        _applicator?.OnBurnExit(other);
    }
}
