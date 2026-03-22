using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine;

 public class Call999Button : MonoBehaviour
{
    public Image buttonImage;

    void Update()
    {
        // Flashing red effect
        float flash = Mathf.PingPong(Time.time * 2f, 1f);
        if (buttonImage != null)
            buttonImage.color = Color.Lerp(
                new Color(0.8f, 0.1f, 0.1f),
                new Color(1.0f, 0.3f, 0.3f),
                flash);
    }

    public void OnPressed()
    {
        Debug.Log("999: Called");
        StopAllCoroutines();
        if (buttonImage != null)
            buttonImage.DOColor(new Color(0.2f, 0.7f, 0.3f), 0.3f);
        TreatmentManager.Instance?.TryCompleteStep1();
        gameObject.SetActive(false);
    }
}

