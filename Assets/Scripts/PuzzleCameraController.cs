using UnityEngine;
using Unity.Cinemachine;

/*
Author: Alden Okoh-Aduako
Provides two functions to enable or disable a camera by changing the camera's priority.
Intended to be used to focus on or unfocus from a puzzle.
*/

public class PuzzleCameraController : MonoBehaviour
{

    [SerializeField] private CinemachineCamera puzzleCamera;

    private const int UNFOCUSED_CAMERA_PRIORITY = -50;
    private const int FOCUSED_CAMERA_PRIORITY = 50;
    void Awake()
    {
        // if puzzle camera priority hasn't been set manually, unfocus on awake
        if(puzzleCamera.Priority == 0) {
            Unfocus();
        }
    }

    // Enables the camera focused on the puzzle
    public void Focus()
    {
        puzzleCamera.Priority = FOCUSED_CAMERA_PRIORITY;
    }

    // Disables the camera focused on the puzzle
    public void Unfocus()
    {
        puzzleCamera.Priority = UNFOCUSED_CAMERA_PRIORITY;
    }
    
}
