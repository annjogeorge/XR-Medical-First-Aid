using UnityEngine;
using TMPro;

public class EducationalManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject introPanel;      // General info about burns
    public GameObject quizPanel;       // The 1st/2nd/3rd degree buttons
    public GameObject treatmentPanel;  // Instructions for bandaging
    public GameObject tryAgainText;    // Small text that says "Try Again"

    [Header("Treatment System")]
    public BandageWrap bandageScript;  // Drag the Arm object here

    void Start()
    {
        // Initial State: Only show intro
        introPanel.SetActive(true);
        quizPanel.SetActive(false);
        treatmentPanel.SetActive(false);
        tryAgainText.SetActive(false);

        // Ensure bandaging is locked
        if (bandageScript != null) bandageScript.enabled = false;
    }

    // Called by the "Next" button on the Intro Panel
    public void ShowQuiz()
    {
        introPanel.SetActive(false);
        quizPanel.SetActive(true);
    }

    // Called by the Wrong Answer buttons (1st or 3rd degree)
    public void WrongAnswer()
    {
        tryAgainText.SetActive(true);
    }

    // Called by the Correct Answer button (2nd degree)
    public void CorrectAnswer()
    {
        quizPanel.SetActive(false);
        treatmentPanel.SetActive(true);

        // Unlock the bandaging logic
        if (bandageScript != null) bandageScript.enabled = true;

        Debug.Log("Correct! Treatment unlocked.");
    }
}