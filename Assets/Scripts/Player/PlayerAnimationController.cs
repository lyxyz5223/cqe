using Assets.Player;
using Assets.Simulations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] internal Animator animator = null;

    public Animator Animator { get => animator; private set => animator = value; }

    private bool airborne = false;

    public bool Airborne { get => airborne; private set => airborne = value; }

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

    }


    /// <summary>
    /// 播放咕咕嘎嘎动画
    /// </summary>
    public void Taunt()
    {
        animator.SetTrigger("Taunt");
    }

    public void Move(MoveDirection direction)
    {
        switch (direction)
        {
            case MoveDirection.Left:
                {
                    animator.SetTrigger("MoveLeft");
                    break;
                }
            case MoveDirection.Right:
                {
                    animator.SetTrigger("MoveRight");
                    break;
                }
            case MoveDirection.None:
                {
                    break;
                }
        }
    }

    public void Jump()
    {
        if (!airborne)
        {
            JumpAnyway();
        }
    }

    public void JumpAnyway()
    {
        airborne = true;
        animator.SetTrigger("Jump");
        animator.SetBool("Airborne", true);
    }

    public void Land()
    {
        airborne = false;
        animator.SetBool("Airborne", false);
    }

    public void Slide()
    {
        animator.SetTrigger("Slide");
    }
    
    public void Sprint()
    {
        if (!animator.GetBool("Sprinting"))
            animator.SetBool("Sprinting", true);
    }

    public void StopSprinting()
    {
        animator.SetBool("Sprinting", false);
    }

    public void Run()
    {
        if (!animator.GetBool("Running"))
            animator.SetBool("Running", true);
    }

    public void StopRunning()
    {
        animator.SetBool("Running", false);
    }

    public bool IsRunning()
    {
        return animator.GetBool("Running");
    }

    public void Die()
    {
        animator.SetTrigger("Die");
    }

}
