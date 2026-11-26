using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace InputSystem
{
    public class InputBridge : MonoBehaviour
    {
        [SerializeField] private InputActionAsset _inputAsset;
        [SerializeField] private Camera _camera;

        private InputActionMap _map;
        private bool _overUI;

        void OnEnable()
        {
            _map = _inputAsset.FindActionMap("Gameplay", true);
            _map.actionTriggered += OnAction;
            _map.Enable();
        }

        void OnDisable()
        {
            _map.actionTriggered -= OnAction;
            _map.Disable();
        }

        private void Update()
        {
            _overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        void OnAction(InputAction.CallbackContext ctx)
        {
            Vector2 screen = Pointer.current != null ? Pointer.current.position.ReadValue() : default;
            var e = new ActionEvent { PointerScreenPos = screen, PointerWorldPos = ToWorld(screen), OverUI = _overUI };

            switch (ctx.action.name)
            {
                case "Move":
                    e.Action = GameAction.Move;
                    e.Move = ctx.ReadValue<Vector2>();
                    e.Phase = ctx.started ? Phase.Started : ctx.performed ? Phase.Changed : Phase.Released;
                    break;

                case "Interact":
                case "Cancel":
                case "PointerPress":
                    if (ctx.started) return;
                    e.Action = ctx.action.name switch {
                        "Interact" => GameAction.Interact,
                        "Cancel"   => GameAction.Cancel,
                        _          => GameAction.PointerPress };
                    e.Phase = ctx.performed ? Phase.Started : Phase.Released;
                    break;

                case "PointerPosition":
                    if (!ctx.performed) return;
                    e.Action = GameAction.PointerMove;
                    break;

                default: return;
            }
            InputRouter.Dispatch(e);
        }

        Vector2 ToWorld(Vector2 screen) =>
            _camera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -_camera.transform.position.z));
    }
}