using UnityEngine;
using UnityEngine.SceneManagement;

public class VRMenuManager : MonoBehaviour
{
    public GameObject Panel_Start;
    public GameObject Panel_Select;

    // Call this when 'START' is pressed
    public void ShowOptions()
    {
        Panel_Start.SetActive(false);
        Panel_Select.SetActive(true);
    }

    // Call this to go back
    public void ShowStart()
    {
        Panel_Start.SetActive(true);
        Panel_Select.SetActive(false);
    }

    // Scene Loading functions
    public void LoadCPR() { SceneManager.LoadScene("CPR_Scene"); }
    public void LoadWounds() { SceneManager.LoadScene("Wounds_Scene"); }
    public void LoadBurns() { SceneManager.LoadScene("Burns_Scene"); }
}
