using Assets.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ChangePlayerInputController : MonoBehaviour
{
    [SerializeField] private PlayerShowController showController;

    private InputActionAsset actionAsset;
    private InputActionMap actionMap;

    private InputAction swipeMoveAction;
    private InputAction swipePressAction;
    private Vector2 startTouchPos;
    private Vector2 currentTouchPos;
    private bool isTouching;

    [SerializeField] private float minSwipeDistance = 50f;

    void OnDestroy()
    {
        actionMap.Disable();
        Destroy(actionAsset);
    }



    private void Awake()
    {
        // 创建完整的Action Asset结构
        actionAsset = ScriptableObject.CreateInstance<InputActionAsset>();
        // 创建ActionMap（相当于一组相关动作）
        actionMap = new InputActionMap("Touch");
        // 创建切换玩家动作
        swipeMoveAction = actionMap.AddAction("Swipe", InputActionType.Value);
        swipeMoveAction.AddBinding("<Touchscreen>/primaryTouch/position"); // 左右滑动
        swipeMoveAction.AddBinding("<Mouse>/position"); // 左右滑动
        swipePressAction = actionMap.AddAction("Press", InputActionType.Button);
        swipePressAction.AddBinding("<Touchscreen>/primaryTouch/press"); // 按压
        swipePressAction.AddBinding("<Mouse>/leftButton"); // 按压
        // 将Map添加到Asset
        actionAsset.AddActionMap(actionMap);
        // 启用
        actionMap.Enable();
    }

    private void OnEnable()
    {
        // 按压开始
        swipePressAction.started += OnPressStarted;
        // 按压结束（手指抬起）
        swipePressAction.canceled += OnPressCanceled;

        // 移动更新
        swipeMoveAction.performed += OnMovePerformed;
        swipeMoveAction.canceled += OnMoveCanceled;

        swipePressAction.Enable();
        swipeMoveAction.Enable();
    }

    private void OnDisable()
    {
        swipePressAction.started -= OnPressStarted;
        swipePressAction.canceled -= OnPressCanceled;

        swipeMoveAction.performed -= OnMovePerformed;
        swipeMoveAction.canceled -= OnMoveCanceled;

        swipePressAction.Disable();
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
            GameManager.Instance.CreateAndPlay("Gugugaga", loop: false, destroyOnEnd: true);
            Debug.Log("Swipe too short, ignored");
            return;
        }

        if (showController == null)
        {
            Debug.LogError("PlayerShowController component is not found.");
            return;
        }
        Vector2 dir = end - start;
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            if (dir.x > 0)
            {
                Debug.Log("Swipe Right");
                var gameManager = GameManager.Instance;
                int index = gameManager.CurrentPlayerIndex;
                if (--index < 0)
                    index = gameManager.Config.PlayerModelPrefabs.Length - 1;
                showController.ChangePlayerModel(index); // Change to the next player model
            }
            else
            {
                Debug.Log("Swipe Left");
                var gameManager = GameManager.Instance;
                int index = gameManager.CurrentPlayerIndex;
                if (++index >= gameManager.Config.PlayerModelPrefabs.Length)
                    index = 0;
                showController.ChangePlayerModel(index); // Change to the next player model
            }
        }
        else
        {
            if (dir.y > 0)
            {
                Debug.Log("Swipe Up");
            }
            else
            {
                Debug.Log("Swipe Down");
            }
        }
    }
}
