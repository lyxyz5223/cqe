using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerShowController : MonoBehaviour
{
    [SerializeField] internal GameObject player;
    [Tooltip("玩家模型展示平台，三个元素分别为左中右")]
    [SerializeField] private GameObject[] playerShowPlatforms = new GameObject[3];
    [SerializeField] private GameObject[] playerShowPlatformCenterPoints = new GameObject[3];
    [SerializeField] private float platformMoveSpeed = 1f;

    MoveDirection platformMoveDirection = MoveDirection.None;
    private GameObject animatingPlayer = null;

    private PlayerAnimationController playerAnimCtrller = null;

    private Rigidbody rb;

    private Coroutine movePlatformCoroutine = null;

    private void OnEnable()
    {
        playerAnimCtrller = GetComponent<PlayerAnimationController>();
        rb = GetComponent<Rigidbody>();
        ChangePlayerModel(GameManager.Instance.CurrentPlayerIndex);
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    private IEnumerator MovePlatformsCoroutine()
    {
        while (platformMoveDirection != MoveDirection.None)
        {
            Vector3 c1 = MovePlatformTo(playerShowPlatforms[0], playerShowPlatformCenterPoints[0].transform.position);
            Vector3 c2 = MovePlatformTo(playerShowPlatforms[1], playerShowPlatformCenterPoints[1].transform.position);
            Vector3 c3 = MovePlatformTo(playerShowPlatforms[2], playerShowPlatformCenterPoints[2].transform.position);

            //rb.MovePosition(c2);
            player.transform.position = new Vector3(c2.x, player.transform.position.y, c2.z);

            if (platformMoveDirection == MoveDirection.Left)
            {
                animatingPlayer.transform.position = new Vector3(c1.x, animatingPlayer.transform.position.y, c1.z);
            }
            else if (platformMoveDirection == MoveDirection.Right)
            {
                animatingPlayer.transform.position = new Vector3(c3.x, animatingPlayer.transform.position.y, c3.z);
            }

            if (Vector3.Distance(c1, playerShowPlatformCenterPoints[0].transform.position) < 0.001f
                && Vector3.Distance(c2, playerShowPlatformCenterPoints[1].transform.position) < 0.001f
                && Vector3.Distance(c3, playerShowPlatformCenterPoints[2].transform.position) < 0.001f)
            {
                platformMoveDirection = MoveDirection.None;
                rb.velocity = Vector3.zero;
                Destroy(animatingPlayer);
                yield break; // 退出协程
            }

            yield return null; // 等待下一帧
        }
    }

    private Vector3 MovePlatformTo(GameObject platform, Vector3 targetPos)
    {
        Vector3 currentPos = platform.transform.position;
        float step = platformMoveSpeed * Time.deltaTime;
        Vector3 stepPos = Vector3.MoveTowards(currentPos, targetPos, step);
        platform.transform.position = stepPos;
        if (Vector3.Distance(platform.transform.position, targetPos) < 0.001f)
        {
            platform.transform.position = targetPos;
        }
        return platform.transform.position;
    }



    public void ChangePlayerModel(int newPlayerIndex)
    {
        int currentIndex = GameManager.Instance.CurrentPlayerIndex;
        var pmPrefabs = GameManager.Instance.Config.PlayerModelPrefabs;
        // 从currentIndex切换到playerIndex，计算需要移动的距离
        int leftDistance = (newPlayerIndex > currentIndex) ? currentIndex + (pmPrefabs.Length - newPlayerIndex) : currentIndex - newPlayerIndex;
        int rightDistance = (newPlayerIndex > currentIndex) ? newPlayerIndex - currentIndex : (pmPrefabs.Length - currentIndex) + newPlayerIndex;
        if (leftDistance < rightDistance)
        {
            // 使用左边的平台
            // 将平台交换
            GameObject temp = playerShowPlatforms[0];
            playerShowPlatforms[0] = playerShowPlatforms[1];
            playerShowPlatforms[1] = temp;
            platformMoveDirection = MoveDirection.Left;
            InstantiatePlayerModel(newPlayerIndex, playerShowPlatformCenterPoints[0]);
        }
        else
        {
            // 使用右边的平台
            GameObject temp = playerShowPlatforms[2];
            playerShowPlatforms[2] = playerShowPlatforms[1];
            playerShowPlatforms[1] = temp;
            platformMoveDirection = MoveDirection.Right;
            InstantiatePlayerModel(newPlayerIndex, playerShowPlatformCenterPoints[2]);
        }
        if (movePlatformCoroutine != null)
            StopCoroutine(movePlatformCoroutine);
        movePlatformCoroutine = StartCoroutine(MovePlatformsCoroutine());
        GameManager.Instance.CurrentPlayerIndex = newPlayerIndex;
    }

    public void InstantiatePlayerModel(int prefabIndex, GameObject platform)
    {
        var playerModelPrefab = GameManager.Instance.Config.PlayerModelPrefabs[prefabIndex];
        var genPos = Vector3.zero;
        var newPlayer = Instantiate(playerModelPrefab, genPos, platform.transform.rotation, transform);
        newPlayer.transform.localPosition = genPos;
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
        if (animatingPlayer != null)
            Destroy(animatingPlayer);
        animatingPlayer = player;
        player = newPlayer;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"PlayerShowController Trigger Enter {other.name}");
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"PlayerShowController Collision Enter {collision.gameObject.name}");
        playerAnimCtrller.Land();
    }

    public void Jump()
    {
        if (playerAnimCtrller.Airborne)
            return;
        playerAnimCtrller.Jump();
        var v = Mathf.Sqrt(2 * Physics.gravity.magnitude * GameManager.Instance.Config.JumpHeight);
        rb.velocity = new Vector3(rb.velocity.x, v, rb.velocity.z);
    }

    public void Slide()
    {
        playerAnimCtrller.Slide();
    }
    public void Move(MoveDirection direction)
    {
        playerAnimCtrller.Move(direction);
    }

    public void Run()
    {
        playerAnimCtrller.Run();
    }

    public void StopRunning()
    {
        playerAnimCtrller.StopRunning();
    }

    public void Sprint()
    {
        playerAnimCtrller.Sprint();
    }

    public void StopSprinting()
    {
        playerAnimCtrller.StopSprinting();
    }
}
