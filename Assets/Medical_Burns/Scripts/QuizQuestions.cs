using UnityEngine;

[CreateAssetMenu(menuName = "Burns Training/Quiz Question")]
public class QuizQuestion : ScriptableObject
{
    public string category; // e.g., "First Aid" or "Burn Degrees"
    public string questionText;
    public string[] options; // The multiple choice answers
    public int correctIndex; // 0 for the first option, 1 for the second, etc.

    [TextArea]
    public string explanation; // Shown after the user answers
}