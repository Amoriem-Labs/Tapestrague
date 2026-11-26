using UnityEngine;
using UnityEngine.InputSystem;

//Author: Danielle Campisi
public class MovementPromptController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject promptA;
    [SerializeField] private GameObject promptD;

    private void Start()
    {
        if (promptA != null)
        {
            promptA.SetActive(true);
        }

        if (promptD != null)
        {
            promptD.SetActive(true);
        }
    }

    private void Update()
    {
        if (Keyboard.current != null)
        {
            if (promptA != null && promptA.activeSelf && (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed))
            {
                promptA.SetActive(false);
            }

            if (promptD != null && promptD.activeSelf && (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed))
            {
                promptD.SetActive(false);
            }

            bool aDone = promptA == null || !promptA.activeSelf;
            bool dDone = promptD == null || !promptD.activeSelf;

            if (aDone && dDone)
            {
                enabled = false;
            }
        }
    }
}