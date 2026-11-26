using UnityEngine;

namespace InputSystem.InputContexts
{
    public class ExplorationInputContext : MonoBehaviour, IInputContext
    {
        [SerializeField] private KnightMovement _player;
        private Collider2D[] _clickResults = new Collider2D[10];

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
            if (actionEvent.Phase != Phase.Started) return false;
            
            var size = Physics2D.OverlapPoint(actionEvent.PointerWorldPos, ContactFilter2D.noFilter, _clickResults);
            for (int i = 0; i < size; i++)
            {
                var coll = _clickResults[i];
                if (!coll || !coll.TryGetComponent(out IExplorationClickable clickable)) continue;
                
                clickable.Click();
                return true;
            }
            
            return false;
        }

        private bool HandleInteract(ActionEvent actionEvent)
        {
            if (actionEvent.Phase != Phase.Started) return false;
            
            return _player.TryInteract();
        }

        private bool HandleMove(ActionEvent actionEvent)
        {
            _player.HandleMoveInput(actionEvent.Move);
            return true;
        }

        #endregion
    }
    
    public interface IExplorationClickable
    {
        public void Click();
    }
}