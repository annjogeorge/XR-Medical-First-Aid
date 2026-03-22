using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmNo : MonoBehaviour
{
    public void OnPressed()
    {
        Debug.Log("CLOTHING: Confirmed not removing");
        TreatmentManager.Instance?.TryCompleteStep2();
        gameObject.SetActive(false);
    }
}
