using Unity.Cinemachine;
using UnityEngine;

namespace PuzzleSystem.PuzzleHandlers
{
    public class PuzzleCameraFocus : MonoBehaviour, IPuzzleStartHandler, IPuzzleFinishHandler
    {
        private CinemachineCamera _puzzleCamera;

        private void Awake()
        {
            _puzzleCamera = GetComponentInChildren<CinemachineCamera>(includeInactive: true);
            _puzzleCamera?.gameObject.SetActive(false);
        }

        public void OnPuzzleStart()
        {
            _puzzleCamera?.gameObject.SetActive(true);
        }

        public void OnPuzzleFinish(bool unlocked)
        {
            _puzzleCamera?.gameObject.SetActive(false);
        }
    }
}