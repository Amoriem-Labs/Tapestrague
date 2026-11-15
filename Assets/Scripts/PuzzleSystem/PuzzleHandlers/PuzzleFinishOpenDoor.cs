using System;
using PrototypeLevel;
using UnityEngine;

namespace PuzzleSystem.PuzzleHandlers
{
    public class PuzzleFinishOpenDoor : MonoBehaviour, IPuzzleFinishHandler
    {
        [SerializeField] private DoorInteraction _doorInteraction;
        [SerializeField] private DoorControl _doorControl;
        
        public void OnPuzzleFinish(bool unlocked)
        {
            if (!unlocked) return;
            _doorInteraction?.OpenDoor();
            _doorControl?.OpenDoor();
        }
    }
}