using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Input
{
    public class KeyboardInputController : MonoBehaviour
    {
        private PlayerInputController playerInputController = null;

        private InputAction playerMove;
        private InputAction playerSprint;

        void Awake()
        {
        }

        void OnEnable()
        {
            if (playerInputController == null)
            {
                playerInputController = GetComponent<PlayerInputController>();
                if (playerInputController == null)
                {
                    Debug.LogError("PlayerInputController component is not found.");
                    return;
                }
            }
            if (playerInputController.InputActions == null)
            {
                Debug.LogError("InputActionAsset is not assigned.");
                return;
            }
            var inputActionsMap = playerInputController.InputActions.FindActionMap("PlayerKeyboard");
            playerMove = inputActionsMap.FindAction("Move");
            playerSprint = inputActionsMap.FindAction("Sprint");
            if (playerMove == null)
            {
                Debug.LogError("Move action not found in PlayerKeyboard action map.");
                return;
            }
            playerInputController.InputActions.Enable();
            // 订阅事件
            //inputActions.PlayerKeyboard.Move.performed += OnInputPerformed;
            playerMove.performed += OnMoveInputPerformed;
            playerSprint.started += OnSprintInputStarted;
            playerSprint.canceled += OnSprintInputCanceled;
        }

        void OnDisable()
        {
            if (playerInputController.InputActions == null)
                return;
            playerInputController.InputActions.Disable();
            //inputActions.PlayerKeyboard.Move.performed -= OnInputPerformed;
            if (playerMove != null)
                playerMove.performed -= OnMoveInputPerformed;
            if (playerSprint != null)
            {
                playerSprint.started -= OnSprintInputStarted;
                playerSprint.canceled -= OnSprintInputCanceled;
            }
        }

        private void Update()
        {
        }

        void OnMoveInputPerformed(InputAction.CallbackContext ctx)
        {
            if (playerInputController == null)
            {
                Debug.LogError("PlayerInputController component is not found.");
                return;
            }
            Vector2 input = ctx.ReadValue<Vector2>();
            if (input.x < 0)
            {
                playerInputController.InvokeMove(MoveDirection.Left);
            }
            else if (input.x > 0)
            {
                playerInputController.InvokeMove(MoveDirection.Right);
            }
            if (input.y < 0)
            {
                playerInputController.InvokeSlide();
            }
            else if (input.y > 0)
            {
                playerInputController.InvokeJump();
            }
        }

        void OnSprintInputStarted(InputAction.CallbackContext ctx)
        {
            if (playerInputController == null)
            {
                Debug.LogError("PlayerInputController component is not found.");
                return;
            }
            playerInputController.InvokeSprint(true);
        }
        void OnSprintInputCanceled(InputAction.CallbackContext ctx)
        {
            if (playerInputController == null)
            {
                Debug.LogError("PlayerInputController component is not found.");
                return;
            }
            playerInputController.InvokeSprint(false);
        }
    }
}
