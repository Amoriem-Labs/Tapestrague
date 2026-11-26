using PuzzleSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace InputSystem.InputContexts
{
    public class CompassPuzzleInputContext : MonoBehaviour, IInputContext, IPuzzleStartHandler, IPuzzleFinishHandler
    {
        public int Priority => 30;
        public GameAction BlockedActions => GameAction.All;

        [SerializeField] private CompassPuzzle _puzzle;
        
        private bool _isPressing = false;

        [Button("TestStart")]
        private void TestStart()
        {
            InputRouter.Push(this);
        }
        
        private bool TryStartRotatingPart(Vector2 pointerWorldPos)
        {
            var idx = _puzzle.TryGetPartIndexAtWorldPosition(pointerWorldPos);
            if (idx < 0) return false;
            return _puzzle.TryRotatePart(idx);
        }
        
        #region Input Context
    
        public bool Handle(ActionEvent e) =>
            e.Action switch
            {
                GameAction.PointerPress => HandlePointerPress(e),
                _ => false
            };

        private bool HandlePointerPress(ActionEvent actionEvent)
        {
            switch (actionEvent.Phase)
            {
                case Phase.Started:
                    _isPressing = TryStartRotatingPart(actionEvent.PointerWorldPos);
                    Debug.Log("Click");
                    return true;
                case Phase.Released:
                case Phase.Canceled:
                    _isPressing = false;
                    _puzzle.ReleasePart();
                    return true;
                default:
                    return false;
            }
        }

        #endregion

        public void OnPuzzleStart()
        {
            InputRouter.Push(this);
        }

        public void OnPuzzleFinish(bool unlocked)
        {
            InputRouter.Remove(this);
        }
    }
}