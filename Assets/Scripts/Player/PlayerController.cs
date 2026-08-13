using Assets.Bonus;
using Assets.Player;
using Assets.Scripts.Player;
using Assets.Simulations;
using Cinemachine.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using static PlayerInputController;

namespace Assets.Player
{
    public class PlayerController : MonoBehaviour
    {
        private static PlayerController instance = null;

        public static PlayerController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<PlayerController>();
                }
                return instance;
            }
            private set { instance = value; }
        }
        [SerializeField] private PlayerElement player = null; // 玩家
        //public GameObject Player { get => player; set { player = value; } }

        [SerializeField] private int currentTrackIndex = 3; // 当前在第几条跑道上，0为最左边，2为最右边
        [SerializeField] private bool invincible = false; // 是否无敌，碰撞障碍物时不会扣血
        [SerializeField] private float invincibleBlinkingSpeed = 3f; // 无敌时闪烁的速度，次/秒
        //[SerializeField] private string transparentLayerName = "TransparentFX"; // 无敌时的透明层
        [SerializeField] private int transparentLayer = 1; // 无敌时的透明层


        public bool Invincible { get => invincible; private set { invincible = value; } }

        private Rigidbody rb = null;
        private Vector3 lastRbPosition = Vector3.zero; // 上一帧的rb位置，用于计算距离

        AudioSource sprintingSound = null;
        private bool isSprinting = false; // 是否在冲刺
        private bool isSliding = false;
        private bool isBlinking = false;
        private float blinkingTimer = 0f; // 闪烁计时器
        private int originalLayer = -1; // 原始层，用于恢复

        private Int64 distance = 0; // 跑了多少米
        private float deltaDistance = 0; // <1米的部分距离，累积到distance中

        [SerializeField] private float rotationSpeed = 5f; // 下滑时旋转（蹲下）的速度
        [SerializeField] private float preferredWaitTimeForCoroutine = 0.5f;
        // 玩家生命值显示的父对象，子对象为每个生命值（爱心）的UI元素，按顺序排列
        [SerializeField] private GameObject healthUIParent = null;
        [SerializeField] private GameObject settlementScoreboard = null;

        public GameObject SettlementScoreboard { get => settlementScoreboard; private set { settlementScoreboard = value; } }

        private PlayerAnimationController playerAnimCtrller = null;

        // 当前玩家的状态，使用位运算表示多个状态
        private BitArray playerStates = new(Enum.GetValues(typeof(PlayerOperation)).Cast<int>().Max() + 1);

        internal void SetInvincible(bool value)
        {
            invincible = value;
        }

        internal void SetSliding(bool value)
        {
            isSliding = value;
            // 设置状态
            playerStates.Set((int)PlayerOperation.Slide, value);
        }
        internal void SetBlinking(bool value)
        {
            isBlinking = value;
        }

        public void ChangePlayerModel(int playerIndex)
        {
            var playerModelPrefab = GameManager.Instance.Config.PlayerModelPrefabs[playerIndex];
            var newPlayer = Instantiate(playerModelPrefab, player.transform.position, player.transform.rotation, transform);
            // 修改PlayerShowController的属性
            //var showCtrller = GetComponent<PlayerShowController>();
            //if (showCtrller != null)
            //    showCtrller.player = newPlayer;
            // 修改PlayerAnimationController的属性
            if (playerAnimCtrller != null)
                playerAnimCtrller.animator = newPlayer.GetComponent<Animator>();
            // 修改虚拟摄像机的跟随目标
            var vcam = FindObjectOfType<Cinemachine.CinemachineVirtualCamera>();
            if (vcam != null)
            {
                vcam.Follow = newPlayer.transform;
                vcam.LookAt = newPlayer.transform;
            }
            Destroy(player.gameObject);
            player = newPlayer.GetComponent<PlayerElement>();
        }

        public Transform PlayerTransform { get => player.transform; }

        private void OnEnable()
        {
            instance = this;
            playerAnimCtrller = GetComponent<PlayerAnimationController>();
            //rb = player.GetComponent<Rigidbody>();
            rb = GetComponent<Rigidbody>();
            lastRbPosition = rb.position;
            ChangePlayerModel(GameManager.Instance.CurrentPlayerIndex);
        }

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            // 计算距离
            UpdateDistance();
            if (isBlinking)
            {
                blinkingTimer += Time.deltaTime;
                if (blinkingTimer * invincibleBlinkingSpeed >= 1f)
                {
                    blinkingTimer = 0f;
                    if (originalLayer == -1)
                        originalLayer = player.gameObject.layer;
                    SetLayerRecusrively((player.gameObject.layer == transparentLayer) ? originalLayer : transparentLayer);
                }
            }
            else
            {
                if (originalLayer != -1)
                {
                    SetLayerRecusrively(originalLayer);
                    originalLayer = -1;
                }
            }
        }

        private void SetLayerRecusrivelyHelper(GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.layer = layer;
                SetLayerRecusrivelyHelper(child.gameObject, layer);
            }
        }
        private void SetLayerRecusrively(int layer)
        {
            SetLayerRecusrivelyHelper(gameObject, layer);
        }

        // 更新角色跑动的距离，角色的距离由其在z轴上的位置决定
        private void UpdateDistance()
        {
            deltaDistance += rb.position.z - lastRbPosition.z;
            if (deltaDistance >= 1)
            {
                distance += (Int64)deltaDistance;
                deltaDistance -= (Int64)deltaDistance;
            }
            lastRbPosition = rb.position;
        }

        public Int64 GetDistance()
        {
            return distance;
        }

        // 根据方向切换跑道，切换时会有动画，切换完成后会把玩家移动到目标跑道上
        // 如果direction为None，则直接把玩家移动到当前跑道上（用于碰撞后回到跑道中心）
        public void ChangeTrack(MoveDirection direction)
        {
            switch (direction)
            {
                case MoveDirection.Left:
                {
                    playerAnimCtrller.Move(direction);
                    currentTrackIndex = (currentTrackIndex - 1 >= 0) ? currentTrackIndex - 1 : 0;
                    playerStates.Set((int)PlayerOperation.MoveLeft, true);
                    break;
                }
                case MoveDirection.Right:
                {
                    playerAnimCtrller.Move(direction);
                    currentTrackIndex = (currentTrackIndex + 1 < RoadManager.Instance.TrackPoints.Length) ? currentTrackIndex + 1 : (RoadManager.Instance.TrackPoints.Length - 1);
                    playerStates.Set((int)PlayerOperation.MoveRight, true);
                        break;
                }
                case MoveDirection.None:
                {
                    //rb.MovePosition(new Vector3(RoadManager.Instance.TrackPoints[currentTrackIndex].transform.position.x, rb.position.y, rb.position.z));
                    break;
                }
            }
        }
        public void MoveLeft()
        {
            if (GameManager.Instance.GameOver)
                return;
            ChangeTrack(MoveDirection.Left);
        }

        public void MoveRight()
        {
            if (GameManager.Instance.GameOver)
                return;
            ChangeTrack(MoveDirection.Right);
        }

        public void Jump()
        {
            if (GameManager.Instance.GameOver)
                return;
            if (!playerAnimCtrller.Airborne)
            {
                JumpAnyway();
            }
        }

        public void Land()
        {
            playerAnimCtrller.Land();
            playerStates.Set((int)PlayerOperation.Jump, false);
        }

        public void JumpAnyway()
        {
            playerAnimCtrller.JumpAnyway();
            var v = Mathf.Sqrt(2 * Physics.gravity.magnitude * GameManager.Instance.Config.JumpHeight);
            rb.velocity = new Vector3(rb.velocity.x, v, rb.velocity.z);
            playerStates.Set((int)PlayerOperation.Jump, true);
            GameManager.Instance.CreateAndPlay("Jump", false, true);
        }

        public void Slide()
        {
            if (isSliding || GameManager.Instance.GameOver)
                return;
            SetSliding(true);
            playerAnimCtrller.Slide();
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y > 0 ? -rb.velocity.y : rb.velocity.y, rb.velocity.z);
            GameManager.Instance.CreateAndPlay("Slide", false, true);
            Simulation.Schedule<Simulation.DelayedEvent>(GameManager.Instance.Config.PlayerSlidingTime).OnExecute += () => SetSliding(false);
        }

        public void Sprint()
        {
            if (isSprinting || GameManager.Instance.GameOver)
                return;
            isSprinting = true;
            playerAnimCtrller.Sprint();
            playerStates.Set((int)PlayerOperation.Sprint, true);
            sprintingSound = GameManager.Instance.CreateAndPlay("Sprint", true, true);
        }

        public void StopSprinting()
        {
            if (!isSprinting)
                return;
            isSprinting = false;
            playerAnimCtrller.StopSprinting();
            playerStates.Set((int)PlayerOperation.Sprint, false);
            sprintingSound.Stop();
            Destroy(sprintingSound.gameObject);
        }

        public void Run()
        {
            if (GameManager.Instance.GameOver)
                return;
            playerAnimCtrller.Run();
            playerStates.Set((int)PlayerOperation.Run, true);
            float velocityX = 0.0f;
            float targetX = RoadManager.Instance.TrackPoints[currentTrackIndex].transform.position.x;
            //float newX = Mathf.MoveTowards(rb.position.x, targetX, changeTrackSpeed * Time.deltaTime);
            float newX = rb.position.x;
            if (targetX != rb.position.x)
            {
                if (Mathf.Abs(targetX - newX) < player.ChangeTrackSpeed * Time.deltaTime)
                {
                    newX = targetX;
                    velocityX = 0.0f;
                }
                else
                {
                    //newX += (targetX > newX ? 1 : -1) * changeTrackSpeed * Time.deltaTime;
                    velocityX = (targetX > newX ? 1 : -1) * player.ChangeTrackSpeed;
                }
            }
            //float newZ = rb.position.z + runSpeed * Time.deltaTime;
            rb.MovePosition(new Vector3(newX, rb.position.y, rb.position.z));
            Quaternion rotation = Quaternion.Euler(isSliding ? -60 : 0, 0, 0);
            // 平滑插值
            Quaternion smoothedRotation = Quaternion.Slerp(
                rb.rotation,
                rotation,
                Time.deltaTime * rotationSpeed
            );
            rb.MoveRotation(smoothedRotation);
            rb.velocity = new Vector3(velocityX, rb.velocity.y, isSprinting ? player.SprintSpeed : player.Speed);
        }

        public void StopRunning()
        {
            playerAnimCtrller.StopRunning();
        }

        void Die()
        {
            playerAnimCtrller.Die();
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"Player Trigger Enter: {other.gameObject.name}");
            if (other.CompareTag("Drop"))
            {
                // 销毁掉落物
                other.gameObject.SetActive(false);
                // 加分
                GameManager.Instance.Bonus(BonusItem.Coin);
            }
        }


        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log($"Player Collision Enter: {collision.gameObject.name}");
            if (collision.gameObject.CompareTag("Border"))
            {// 撞到左右边界，回到当前跑道中心
                ChangeTrack(MoveDirection.None);
            }
            else if (collision.gameObject.CompareTag("Road"))
            {// 落地
                Land();
            }
            else
            {// 撞到其他东西，判断是否是障碍物
                GameObject obstaclesParent = collision.gameObject; // 表示障碍物的父对象
                bool isObstacle = true;
                // 逐层向上查找父对象，直到找到带有Obstacle标签的对象，或者没有父对象为止
                while (!obstaclesParent.transform.CompareTag("Obstacle"))
                {
                    if (obstaclesParent.transform.parent == null)
                    {
                        isObstacle = false;
                        break;
                    }
                    obstaclesParent = obstaclesParent.transform.parent.gameObject;
                }
                if (isObstacle)
                {// 撞到障碍物，检查当前玩家是否进行的安全操作
                    var obstacleItem = collision.gameObject.GetComponentInParent<ObstacleItem>();
                    if (obstacleItem != null)
                    {
                        // 根据玩家当前的状态生成一个PlayerOperation列表，表示玩家当前正在进行的操作
                        List<PlayerOperation> ops = GetOperationsList();
                        if (!obstacleItem.IsOperationsSafe(ops))
                        {
                            //collision.collider.enabled = false;
                            Transform[] children = obstacleItem.GetComponentsInChildren<Transform>();
                            foreach (var child in children)
                            {
                                var collider = child.GetComponent<Collider>();
                                if (collider != null)
                                    collider.enabled = false;
                            }
                            TakeDamage();
                        }
                    }
                }
            }
        }

        private List<PlayerOperation> GetOperationsList()
        {
            List<PlayerOperation> ops = new();
            for (int i = 0; i < playerStates.Length; i++)
            {
                if (playerStates.Get(i))
                {
                    ops.Add((PlayerOperation)i);
                }
            }
            return ops;
        }

        /// <summary>
        /// 玩家受伤，扣血并设置无敌状态，持续一段时间后恢复
        /// 如果血量清0，则结束游戏
        /// </summary>
        private void TakeDamage()
        {
            if (invincible)
                    return;
            invincible = true;
            isBlinking = true;
            player.Health -= 33.334f;
            // 受伤并且将角色设置为无敌状态，持续一段时间后恢复
            Simulation.Schedule<InvincibleTimeEndEvent>(GameManager.Instance.Config.PlayerInvincibleTime).Player = this;
            // 将玩家的生命值UI中的一个爱心隐藏起来，表示扣血
            foreach (Transform healthUI in healthUIParent.transform)
            {
                if (healthUI.gameObject.activeInHierarchy)
                {
                    healthUI.gameObject.SetActive(false);
                    break;
                }
            }
            // 玩家未死亡
            if (player.Health > 0)
            {
                GameManager.Instance.CreateAndPlay("TakeDamage", false, true);
            }
            // 受伤后检测玩家是否死亡，如果死亡则结束游戏
            if (player.Health <= 0)
            {
                // 撞到障碍物，游戏结束
                StopSprinting();
                StopRunning();
                Die();
                var audio = GameManager.Instance.CreateAndPlay("Die", false, true);
                GameManager.Instance.EndGame();
                //Invoke(() => GameManager.Instance.EndGame(), audio.clip.length);
                //Simulation.Schedule<Simulation.DelayedEvent>(audio.clip.length).OnExecute += () => GameManager.Instance.EndGame();
            }
        }

        /// <summary>
        /// 获取协程循环的等待时间
        /// </summary>
        /// <param name="prevWaitTimeForCoroutine">上一个等待时间，函数内会更新</param>
        /// <returns>等待时间，如果不需要更新等待时间则返回负数</returns>
        public float GetWaitTimeCoroutineLoop(ref float prevWaitTimeForCoroutine)
        {
            float absPregenerateRoadDistance = Mathf.Abs(GameManager.Instance.Config.PreGeneratedDistance);
            if (prevWaitTimeForCoroutine * player.Speed > absPregenerateRoadDistance)
            {
                if (player.Speed > 0)
                {
                    float cur = absPregenerateRoadDistance / player.Speed;
                    if (cur != prevWaitTimeForCoroutine)
                    {
                        prevWaitTimeForCoroutine = cur;
                        return cur;
                    }
                    else
                        return -1;
                }
                else
                {
                    if (0 != prevWaitTimeForCoroutine)
                    {
                        prevWaitTimeForCoroutine = 0;
                        return 0;
                    }
                    else
                        return -1;
                }
            }
            return preferredWaitTimeForCoroutine != prevWaitTimeForCoroutine ? preferredWaitTimeForCoroutine : -1;
        }
    }
}