using UnityEngine;
using UnityEngine.InputSystem; // Required for the new Input System

//Author: Danielle Campisi
public class MovementPromptController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject promptUIContainer;

    private void Start()
    {
        if (promptUIContainer != null)
        {
            promptUIContainer.SetActive(true);
        }
        else
        {
            Debug.LogWarning("MovementPromptController: Prompt UI Container is not assigned!", this);
        }
    }

    private void Update()
    {
        // Check if the keyboard is active and A, D, or arrow keys are pressed
        if (Keyboard.current != null)
        {
            bool pressedA = Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed;
            bool pressedD = Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed;

            if (pressedA || pressedD)
            {
                TriggerDismissal();
            }
        }
    }

    private void TriggerDismissal()
    {
        if (promptUIContainer != null)
        {
            promptUIContainer.SetActive(false);
        }

        // Disable this component once its job is done
        enabled = false;
    }
}