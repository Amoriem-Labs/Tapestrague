using UnityEngine;

namespace InputSystem.InputContexts
{
    public class ExplorationInputContext : MonoBehaviour, IInputContext
    {
        // base context
        public int Priority => 10;
        // block all inputs
        public GameAction BlockedActions => GameAction.All;

        private void OnEnable()
        {
            InputRouter.Push(this);
        }

        private void OnDisable()
        {
            InputRouter.Remove(this);
        }

        #region Input Context

        public bool Handle(ActionEvent e)
        {
            // only implement enabled inputs
            switch (e.Action)
            {
                case GameAction.Move:
                    return HandleMove(e);
                case GameAction.Interact:
                    return HandleInteract(e);
                case GameAction.PointerPress:
                    return HandlePointerPress(e);
                default:
                    break;
            }

            return false;
        }

        private bool HandlePointerPress(ActionEvent actionEvent)
        {
            if (actionEvent.Phase != Phase.Started) return false;
            
            var coll = Physics2D.OverlapPoint(actionEvent.PointerWorldPos);
            if (!coll || !coll.TryGetComponent(out PuzzleInputContext puzzle)) return false;
            
            InputRouter.Push(puzzle);
            return true;
        }

        private bool HandleInteract(ActionEvent actionEvent)
        {
            Debug.Log("Interact");
            return true;
        }

        private bool HandleMove(ActionEvent actionEvent)
        {
            Debug.Log($"Move: {actionEvent.Move}");
            return true;
        }

        #endregion
    }
}