using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SwipeInputActionsController : MonoBehaviour
{
    //[SerializeField] private InputActionAsset inputActions;
    private PlayerInputController playerInputController;

    [Header("滑动设置")]
    [SerializeField] private float minSwipeDistance = 50f;

    //private UnityEvent onSwipeUp;
    //private UnityEvent onSwipeDown;
    //private UnityEvent onSwipeLeft;
    //private UnityEvent onSwipeRight;

    private InputAction swipeMoveAction;
    private InputAction swipePressAction;
    private InputAction swipeHoldAction;
    private Vector2 startTouchPos;
    private Vector2 currentTouchPos;
    private bool isTouching;

    private void Awake()
    {
        playerInputController = GetComponent<PlayerInputController>();
        if (playerInputController == null)
        {
            Debug.LogError("PlayerInputController component is not found.");
            return;
        }

        var playerTouchMap = playerInputController.InputActions.FindActionMap("PlayerTouch");
        swipeMoveAction = playerTouchMap.FindAction("Move");
        swipePressAction = playerTouchMap.FindAction("Press");
        swipeHoldAction = playerTouchMap.FindAction("Hold");
    }

    private void OnEnable()
    {
        // 按压开始
        swipePressAction.started += OnPressStarted;
        // 按压结束（手指抬起）
        swipePressAction.canceled += OnPressCanceled;

        // 按下开始
        swipeHoldAction.started += OnHoldStarted;
        // 长按触发
        swipeHoldAction.performed += OnHoldPerformed;
        // 长按结束（手指抬起）
        swipeHoldAction.canceled += OnHoldCanceled;

        // 移动更新
        swipeMoveAction.performed += OnMovePerformed;
        swipeMoveAction.canceled += OnMoveCanceled;

        swipePressAction.Enable();
        swipeHoldAction.Enable();
        swipeMoveAction.Enable();
    }

    private void OnDisable()
    {
        swipePressAction.started -= OnPressStarted;
        swipePressAction.canceled -= OnPressCanceled;

        swipeHoldAction.started -= OnHoldStarted;
        swipeHoldAction.performed -= OnHoldPerformed;
        swipeHoldAction.canceled -= OnHoldCanceled;

        swipeMoveAction.performed -= OnMovePerformed;
        swipeMoveAction.canceled -= OnMoveCanceled;

        swipePressAction.Disable();
        swipeHoldAction.Disable();
        swipeMoveAction.Disable();
    }

    private void OnPressStarted(InputAction.CallbackContext context)
    {
        Debug.Log("Press started");
        isTouching = true;
        startTouchPos = swipeMoveAction.ReadValue<Vector2>();
        currentTouchPos = startTouchPos;
    }

    private void OnPressCanceled(InputAction.CallbackContext context)
    {
        Debug.Log("Press ended");
        if (!isTouching) return;

        isTouching = false;
        DetectSwipe(startTouchPos, currentTouchPos);
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (isTouching)
        {
            currentTouchPos = context.ReadValue<Vector2>();
            Debug.Log($"Move updated: {currentTouchPos}");
        }
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        Debug.Log("Move canceled");
    }

    private void DetectSwipe(Vector2 start, Vector2 end)
    {
        float swipeDistance = Vector2.Distance(start, end);
        Debug.Log($"Swipe detected - Distance: {swipeDistance}, Start: {start}, End: {end}");

        if (swipeDistance < minSwipeDistance)
        {
            //GameManager.Instance.CreateAndPlay("Gugugaga", loop: false, destroyOnEnd: true);
            Debug.Log("Swipe too short, ignored");
            return;
        }

        if (playerInputController == null)
        {
            Debug.LogError("PlayerInputController component is not found.");
            return;
        }
        Vector2 dir = end - start;
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            if (dir.x > 0)
            {
                Debug.Log("Swipe Right");
                playerInputController.InvokeMove(MoveDirection.Right);
            }
            else
            {
                Debug.Log("Swipe Left");
                playerInputController.InvokeMove(MoveDirection.Left);
            }
        }
        else
        {
            if (dir.y > 0)
            {
                Debug.Log("Swipe Up");
                playerInputController.InvokeJump();
            }
            else
            {
                Debug.Log("Swipe Down");
                playerInputController.InvokeSlide();
            }
        }
    }


    private void OnHoldStarted(InputAction.CallbackContext context)
    {
        Debug.Log("Hold started");
    }

    private void OnHoldPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("Hold performed");

        if (playerInputController == null)
        {
            Debug.LogError("PlayerInputController component is not found.");
            return;
        }

        playerInputController.InvokeSprint(true);
    }

    private void OnHoldCanceled(InputAction.CallbackContext context)
    {
        Debug.Log("Hold ended");

        if (playerInputController == null)
        {
            Debug.LogError("PlayerInputController component is not found.");
            return;
        }

        playerInputController.InvokeSprint(false);
    }

}
