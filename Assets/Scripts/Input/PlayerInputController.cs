using Assets.Player;
using Assets.Simulations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions = null;
    public InputActionAsset InputActions { get => inputActions; private set => inputActions = value; }

    public interface IPlayerInputHandler
    {
        public void OnJump();
        public void OnSlide();
        public void OnMove(MoveDirection direction);

        public void OnRun(bool isRunning);

        public void OnSprint(bool isSprinting);
    }

    public class PlayerAnimInputHandler : IPlayerInputHandler
    {
        private PlayerAnimationController animCtrller = null;
        public PlayerAnimInputHandler(PlayerAnimationController animCtrller)
        {
            this.animCtrller = animCtrller;
        }
        public void OnJump()
        {
            animCtrller.Jump();
        }
        public void OnSlide()
        {
            animCtrller.Slide();
        }
        public void OnMove(MoveDirection direction)
        {
            animCtrller.Move(direction);
        }
        public void OnRun(bool isRunning)
        {
            if (isRunning)
                animCtrller.Run();
            else
                animCtrller.StopRunning();
        }

        public void OnSprint(bool isSprinting)
        {
            if (isSprinting)
                animCtrller.Sprint();
            else
                animCtrller.StopSprinting();
        }
    }

    private class PlayerShowInputHandler : IPlayerInputHandler
    {
        private PlayerShowController playerShowController = null;
        public PlayerShowInputHandler(PlayerShowController playerShowController)
        {
            this.playerShowController = playerShowController;
        }
        public void OnJump()
        {
            playerShowController.Jump();
        }
        public void OnSlide()
        {
            playerShowController.Slide();
        }
        public void OnMove(MoveDirection direction)
        {
            if (direction != MoveDirection.None)
                playerShowController.Move(direction);
        }

        public void OnRun(bool isRunning)
        {
            if (isRunning)
                playerShowController.Run();
            else
                playerShowController.StopRunning();
        }

        public void OnSprint(bool isSprinting)
        {
            if (isSprinting)
                playerShowController.Sprint();
            else
                playerShowController.StopSprinting();
        }
    }

    private class PlayerCtrllerInputHandler : IPlayerInputHandler
    {
        private PlayerController playerController = null;
        public PlayerCtrllerInputHandler(PlayerController playerController)
        {
            this.playerController = playerController;
        }
        public void OnJump()
        {
            playerController.Jump();
        }
        public void OnSlide()
        {
            playerController.Slide();
        }
        public void OnMove(MoveDirection direction)
        {
            if (direction != MoveDirection.None)
                playerController.ChangeTrack(direction);
        }

        public void OnRun(bool isRunning)
        {
            if (isRunning)
                playerController.Run();
            else
                playerController.StopRunning();
        }
        public void OnSprint(bool isSprinting)
        {
            if (isSprinting)
                playerController.Sprint();
            else
                playerController.StopSprinting();
        }
    }


    private PlayerController playerController = null;
    private PlayerShowController playerShowCtrller = null;
    private PlayerAnimationController playerAnimCtrller = null;
    public IPlayerInputHandler InputHandler = null;


    void OnEnable()
    {
        playerController = GetComponent<PlayerController>();
        playerAnimCtrller = GetComponent<PlayerAnimationController>();
        playerShowCtrller = GetComponent<PlayerShowController>();
        if (playerController != null)
            InputHandler = new PlayerCtrllerInputHandler(playerController);
        else if (playerShowCtrller != null)
            InputHandler = new PlayerShowInputHandler(playerShowCtrller);
        else if (playerAnimCtrller != null)
            InputHandler = new PlayerAnimInputHandler(playerAnimCtrller);
    }

    private void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //bool jumped = false;
        //bool slided = false;
        //MoveDirection direction = MoveDirection.None;
        //if (Input.GetButtonDown("Horizontal"))
        //{
        //    direction = (Input.GetAxisRaw("Horizontal") > 0 ? MoveDirection.Right : MoveDirection.Left);
        //}
        //if (Input.GetButtonDown("Vertical"))
        //{
        //    if (Input.GetAxisRaw("Vertical") > 0)
        //    {
        //        jumped = true;
        //    }
        //    else
        //    {
        //        slided = true;
        //    }
        //}
        //if (jumped)
        //{
        //    InputHandler?.OnJump();
        //}
        //else if (slided)
        //{
        //    InputHandler?.OnSlide();
        //}
        if (playerAnimCtrller == null || playerAnimCtrller.IsRunning())
        {
            InputHandler?.OnRun(true);
        }
        else if (playerAnimCtrller != null && !playerAnimCtrller.IsRunning())
        {
            InputHandler?.OnRun(false);
        }


        //// 检测鼠标左键点击，播放咕咕嘎嘎
        //if (Input.GetMouseButtonDown(0))
        //{
        //    // 从鼠标位置发射射线
        //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //    // 检测射线是否碰到物体
        //    if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject.CompareTag("Player"))
        //    {
        //        Debug.Log($"Mouse clicked at: {hit.point}, hit object: {hit.collider.gameObject.name}");
        //        // 获取被点击的游戏物体
        //        GameObject clickedObject = hit.collider.gameObject;
        //        playerAnimCtrller.Taunt();
        //        GameManager.Instance.CreateAndPlay("Gugugaga", loop: false, destroyOnEnd: true);
        //    }
        //}
    }


    public void InvokeJump()
    {
        InputHandler?.OnJump();
    }

    public void InvokeSlide()
    {
        InputHandler?.OnSlide();
    }

    public void InvokeSprint(bool isSprinting)
    {
        InputHandler?.OnSprint(isSprinting);
    }

    public void InvokeMove(MoveDirection direction)
    {
        InputHandler?.OnMove(direction);
    }

    public void InvokeRun()
    {
        InputHandler?.OnMove(MoveDirection.None);
    }

}


