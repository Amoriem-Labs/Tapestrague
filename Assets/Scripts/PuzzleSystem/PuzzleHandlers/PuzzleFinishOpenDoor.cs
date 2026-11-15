using System;
using UnityEngine;

namespace PuzzleSystem.PuzzleHandlers
{
    public class PuzzleFinishOpenDoor : MonoBehaviour, IPuzzleFinishHandler
    {
        [SerializeField] private DoorInteraction _doorInteraction;
        
        public void OnPuzzleFinish(bool unlocked)
        {
            if (!unlocked) return;
            if (!_doorInteraction)
            {
                Debug.LogError("No door interaction set");
                return;
            }
            _doorInteraction.OpenDoor();
        }
    }
}