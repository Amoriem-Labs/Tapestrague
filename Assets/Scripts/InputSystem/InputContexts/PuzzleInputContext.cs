using System;
using UnityEngine;

namespace InputSystem.InputContexts
{
    public class PuzzleInputContext : MonoBehaviour, IInputContext
    {
        public int Priority => 30;
        public GameAction BlockedActions => GameAction.All;
    
        private enum PointerState { Idle, Clicking, Dragging }

        private PointerState _pointerState;
        
        #region Input Context
    
        public bool Handle(ActionEvent e)
        {
            switch (e.Action)
            {
                case GameAction.PointerPress:
                    return HandlePointerPress(e);
                case GameAction.PointerMove:
                    return HandlePointerMove(e);
                default:
                    break;
            }

            return false;
        }

        private bool HandlePointerMove(ActionEvent actionEvent)
        {
            switch (_pointerState)
            {
                case PointerState.Clicking:
                    Debug.Log($"Start Dragging: {actionEvent.PointerScreenPos}");
                    _pointerState = PointerState.Dragging;
                    return true;
                case PointerState.Dragging:
                    Debug.Log($"Dragging: {actionEvent.PointerScreenPos}");
                    return true;
                default:
                    return false;
            }
        }

        private bool HandlePointerPress(ActionEvent actionEvent)
        {
            switch (actionEvent.Phase)
            {
                case Phase.Started:
                    _pointerState = PointerState.Clicking;
                    return true;
                case Phase.Released:
                    switch (_pointerState)
                    {
                        case PointerState.Clicking:
                            Debug.Log($"Clicked: {actionEvent.PointerScreenPos}");
                            break;
                        case PointerState.Dragging:
                            Debug.Log($"Drag Finished: {actionEvent.PointerScreenPos}");
                            InputRouter.Remove(this);
                            break;
                        default:
                            break;
                    }
                    _pointerState = PointerState.Idle;
                    return true;
                case Phase.Canceled:
                    _pointerState = PointerState.Idle;
                    return true;
                default:
                    return false;
            }
        }

        #endregion
    }
}