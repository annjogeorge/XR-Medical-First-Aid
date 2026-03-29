using UnityEngine;

public class HandTrigger : MonoBehaviour
{
    public Animator animator;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // test trigger
        {
            animator.SetTrigger("ExtendHand");
        }
    }
}
