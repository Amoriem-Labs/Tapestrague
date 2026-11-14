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

        var player = GameObject.FindGameObjectWithTag("Player");
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if ((player.transform.position - transform.position).sqrMagnitude < 25)
            {
                animator.SetTrigger("open");
            } else
            {
                Debug.Log("can't open yet");
            }
        }
    }
}
