using System;
using UnityEngine;

namespace InputSystem
{
    [Flags]
    public enum GameAction : byte
    {
        Move = 1 << 0,
        Interact = 1 << 1,
        Cancel = 1 << 2,
        PointerPress = 1 << 3,
        PointerMove = 1 << 4,
        
        None = 0,
        All = Byte.MaxValue,
    }
    public enum Phase { Started, Changed, Released, Canceled }

    public struct ActionEvent {
        public GameAction Action;
        public Phase Phase;
        public Vector2 Move;
        public Vector2 PointerScreenPos;
        public Vector2 PointerWorldPos;
        public bool OverUI;
    }
}