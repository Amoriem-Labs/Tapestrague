using UnityEngine;

namespace InputSystem.InputContexts
{
    public class ExplorationInputContext : MonoBehaviour, IInputContext
    {
        [SerializeField] private KnightMovement _player;
        
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

        public bool Handle(ActionEvent e) =>
            // only implement enabled inputs
            e.Action switch
            {
                GameAction.Move => HandleMove(e),
                GameAction.Interact => HandleInteract(e),
                GameAction.PointerPress => HandlePointerPress(e),
                _ => false
            };

        private bool HandlePointerPress(ActionEvent actionEvent)
        {
            // if (actionEvent.Phase != Phase.Started) return false;
            //
            // var coll = Physics2D.OverlapPoint(actionEvent.PointerWorldPos);
            // if (!coll || !coll.TryGetComponent(out PuzzleInputContext puzzle)) return false;
            //
            // InputRouter.Push(puzzle);
            Debug.Log("Pointer Click");
            return true;
        }

        private bool HandleInteract(ActionEvent actionEvent)
        {
            Debug.Log("Interact");
            return true;
        }

        private bool HandleMove(ActionEvent actionEvent)
        {
            _player.HandleMoveInput(actionEvent.Move);
            return true;
        }

        #endregion
    }
}