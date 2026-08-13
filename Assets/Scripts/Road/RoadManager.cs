using Assets.Player;
using Assets.Road;
using Assets.Scripts;
using Assets.Scripts.Tools;
using Assets.Scripts.Utils;
using Assets.Simulations;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadManager : MonoBehaviour
{
    private static RoadManager _instance = null;
    static public RoadManager Instance {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<RoadManager>();
            }
            return _instance;
        } 
        private set { _instance = value; }
    }

    [SerializeField] private GameObject roadSegmentParent = null;
    [SerializeField] private GameObject[] trackPoints = null;



    [Tooltip("当前存在的路段列表，按生成顺序排列")]
    [SerializeField] private List<GameObject> roadSegments = null;
    [Tooltip("最新创建的的路段列表，按生成顺序排列")]
    [SerializeField] private List<GameObject> latestExtendedRoads = new();


    public GameObject[] TrackPoints { get => trackPoints; }

    public List<GameObject> RoadSegments { get => roadSegments; }


    class ExtendRoadCallback : ExtendController.IExtendCallback
    {
        RoadManager manager = null;
        public ExtendRoadCallback(RoadManager manager)
        {
            this.manager = manager;
        }
        public void OnExtend()
        {
            Simulation.Schedule<ExtendRoadEvent>(0).roadManager = manager;
        }
        public void OnRemove(int index, GameObject obj)
        {
            DestroyRoadEvent ev = Simulation.Schedule<DestroyRoadEvent>(1);
            ev.roadManager = manager;
            ev.gameObject = obj;
        }
        public bool ShouldExtend(GameObject lastGameObject)
        {
            return ExtendTools.ShouldExtend(lastGameObject);
        }
    }
    private ExtendController roadExtendCtrller = null;

    private float prevWaitTimeRoad = 0f;
    private Coroutine coroutine = null;

    private void Awake()
    {
        _instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    private void OnEnable()
    {
        if (roadExtendCtrller == null)
        {
            YieldInstruction updateSomethingWaitForSeconds = new WaitForSeconds(PlayerController.Instance.GetWaitTimeCoroutineLoop(ref prevWaitTimeRoad));
            roadExtendCtrller = new ExtendController(new ExtendRoadCallback(this), updateSomethingWaitForSeconds);
        }
        coroutine ??= StartCoroutine(roadExtendCtrller.CheckAndExtend(roadSegments));
    }

    private void OnDisable()
    {
        StopCoroutine(coroutine);
        coroutine = null;
    }


    // Update is called once per frame
    void Update()
    {
        float time = PlayerController.Instance.GetWaitTimeCoroutineLoop(ref prevWaitTimeRoad);
        if (time >= 0)
            roadExtendCtrller.YieldInstruction = new WaitForSeconds(time);
    }



    // 扩展地图（生成新的路段）
    public void Extend()
    {
        while (true)
        {
            int prefabIndex = Random.Range(0, GameManager.Instance.Config.RoadSegmentPrefabs.Length);
            GameObject roadSegmentPrefab = GameManager.Instance.Config.RoadSegmentPrefabs[prefabIndex];
            Vector3 pos = new();
            if (roadSegments.Count > 0)
            {
                // 在最后一个路段后面生成新的路段
                GameObject lastSegment = roadSegments.Last();
                if (!ExtendTools.ShouldExtend(lastSegment))
                    break;
                pos = GetConnectionPointOfRoadSegment(lastSegment, true).position - GetConnectionPointOfRoadSegment(roadSegmentPrefab, false).position;
            }
            GameObject newSegment = Instantiate(roadSegmentPrefab, pos, Quaternion.identity);
            // 将新路段设置为Roads的子物体
            newSegment.transform.SetParent(roadSegmentParent != null ? roadSegmentParent.transform : transform);
            roadSegments.Add(newSegment);
            // 暂时不需要
            //latestExtendedRoads.Add(newSegment);
            Simulation.Schedule<CheckAndExtendTerrainEvent>(0).RoadSegment = newSegment;
        }
        //Simulation.Schedule<CheckAndExtendTerrains>(0);
    }




    public void DestroyRoadSegment(GameObject gameObject)
    {
        foreach (GameObject road in latestExtendedRoads)
        {
            if (road == gameObject)
                return; // 找到则不要删除
        }
        roadSegments.Remove(gameObject);
        Destroy(gameObject);
        Simulation.Schedule<CheckAndRemoveTerrainEvent>(0);
    }

    /// <summary>
    /// 获取某个路段的连接点（ConnectionPoint）的Transform，用于连接新的路段
    /// </summary>
    /// <param name="roadSegment">路段对象</param>
    /// <param name="isOutConnectionPoint">是否获取出口连接点</param>
    /// <returns>连接点的Transform，如果未找到则返回null</returns>
    public static Transform GetConnectionPointOfRoadSegment(GameObject roadSegment, bool isOutConnectionPoint)
    {
        foreach (Transform child in roadSegment.transform)
        {
            if (isOutConnectionPoint ? child.CompareTag("ConnectionPointOut") : child.CompareTag("ConnectionPointIn"))
            {
                return child;
            }
        }
        return null;
    }


    /// <summary>
    /// 以x轴为左右，y轴为上下，z轴为前后，根据旋转方向计算目标位置正确的世界坐标位置
    /// </summary>
    /// <param name="localPosition">目标位置</param>
    /// <param name="quaternion">旋转角度</param>
    /// <returns>矫正后的位置</returns>
    private Vector3 CalculateCorrectPosition(Vector3 localPosition, Quaternion quaternion)
    {
        return quaternion * localPosition;
    }

}
