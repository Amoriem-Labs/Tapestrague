using UnityEngine;
using UnityEngine.InputSystem;

//Author: Erica Zhao

public class DoorInteraction : MonoBehaviour
{

    Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        //note: actual animation to be tweaked later when the door is replaced
        //with actual door image and not an easily scalable rectangle png
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        //other conditions (i.e. if player is close by) to be added
        {
            animator.SetTrigger("open");
        }
    }
}
