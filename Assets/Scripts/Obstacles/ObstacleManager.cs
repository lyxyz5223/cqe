using Assets.Player;
using Assets.Scripts;
using Assets.Scripts.Tools;
using Assets.Scripts.Utils;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    [SerializeField] private GameObject obstacleParent = null;
    [SerializeField] private List<GenerationObjectInfo> obstacles = new();
    private ObjectPool<int/*prefab index*/, GameObject/*wrapper*/> obstaclesPool = new();
    private ExtendController obstacleExtendCtrller = null;
    private float prevWaitTime = 0f;
    private List<TerrainManager.RefObj> latestExtendedTerrains = new();
    private Coroutine coroutine = null;

    class ExtendObstacleCallback : ExtendController.IExtendCallback
    {
        ObstacleManager manager = null;
        public ExtendObstacleCallback(ObstacleManager manager)
        {
            this.manager = manager;
        }
        public void OnExtend()
        {
            manager.GenerateObstacles();
        }
        public void OnRemove(int index, GameObject obj)
        {
            //foreach (Transform transform in obj.transform)
            //{
            //    transform.gameObject.SetActive(true);
            //}
            obj.SetActive(false);
            manager.obstaclesPool.Return(manager.obstacles[index].prefabIndex, obj);
            manager.obstacles.RemoveAt(index);
        }
        public bool ShouldExtend(GameObject lastGameObject)
        {
            return ExtendTools.ShouldExtend(lastGameObject);
        }
    }

    private void Awake()
    {
        TerrainManager.Instance.OnTerrainAdded += obj => latestExtendedTerrains.Add(obj.Ref());
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float time = PlayerController.Instance.GetWaitTimeCoroutineLoop(ref prevWaitTime);
        if (time >= 0)
            obstacleExtendCtrller.YieldInstruction = new WaitForSeconds(time);
    }
    private void OnEnable()
    {
        if (obstacleExtendCtrller == null)
        {
            YieldInstruction wait = new WaitForSeconds(PlayerController.Instance.GetWaitTimeCoroutineLoop(ref prevWaitTime));
            obstacleExtendCtrller = new ExtendController(new ExtendObstacleCallback(this), wait);
        }
        coroutine ??= StartCoroutine(obstacleExtendCtrller.CheckAndExtend(obstacles));
    }

    private void OnDisable()
    {
        StopCoroutine(coroutine);
        coroutine = null;
    }
    /// <summary>
    /// 生成掉落物（金币等）
    /// </summary>
    /// <returns>是否生成了新的障碍物</returns>
    public bool GenerateObstacles()
    {
        var latestExtendedSegments = latestExtendedTerrains;
        if (latestExtendedSegments.Count == 0 || GameManager.Instance.Config.ObstaclePrefabs.Length == 0)
            return false;
        while (true) // 每次循环生成一个路段的障碍物，直到最后一个路段上的障碍物已经生成完毕，才停止生成新的障碍物
        {
            if (latestExtendedSegments.Count == 0)
            {
                return true;
            }
            for (int i = 0; i < latestExtendedSegments.Count; ++i)
            {
                GameObject segment = latestExtendedSegments[i].gameObject;
                latestExtendedSegments[i].Release();
                latestExtendedSegments.RemoveAt(i--);
                if (segment == null)
                    continue;
                GenerateObstaclesOnSegment(segment);
            }
        }
    }

    public List<Terrain> GetTerrains()
    {
        List<Terrain> terrains = new();
        foreach (var terrainObj in TerrainManager.Instance.Terrains)
        {
            Terrain[] ts = terrainObj.GetComponentsInChildren<Terrain>();
            terrains.AddRange(ts);
        }
        return terrains;
    }

    public void GenerateObstaclesOnSegment(GameObject segment)
    {
        if (segment == null)
            return;
        var terrainElem = segment.GetComponent<TerrainManagerElement>();
        int genObstaclesCount = Random.Range(0, terrainElem.MaxObstaclesCount + 1);
        var obstaclesSpacingRange = terrainElem.ObstaclesSpacingRange.GetAbsoluteRange(terrainElem.TerrainSize.z);
        List<float> usedZOffsets = new();
        for (int i = 0; i < genObstaclesCount; ++i)
        {
            // 随机选一个障碍物预制体
            int prefabIndex = Random.Range(0, GameManager.Instance.Config.ObstaclePrefabs.Length);
            // 获取障碍物的轨道限制信息
            IntRange trackIndexRange = GameManager.Instance.Config.ObstaclePrefabs[prefabIndex].GetComponent<ObstacleItem>().TrackIndexRange.GetAbsoluteRange(TerrainManager.Instance.TrackPoints.Length);
            // 在合理范围内随机选一个轨道[start, end]
            int trackIndex = Random.Range(trackIndexRange.start, trackIndexRange.end + 1);
            // 随机选一个z偏移量，确保不会与已使用的z偏移量过于接近
            float zOffset = 0f;
            bool invalidOffset = true;
            // TODO: 优化随机生成逻辑
            int reTryCounter = 100; // 最多尝试100次重新生成
            while (invalidOffset && reTryCounter >= 0)
            {
                --reTryCounter;
                zOffset = Random.Range(0f, terrainElem.TerrainSize.z);
                invalidOffset = false;
                foreach (var usedZ in usedZOffsets)
                {
                    if (Mathf.Abs(zOffset - usedZ) < obstaclesSpacingRange.start)
                    {
                        invalidOffset = true;
                        break;
                    }
                }
                if (invalidOffset)
                    Debug.Log($"ObstacleManager: [{reTryCounter}] Invalid zOffset generated, retrying...");
            }
            usedZOffsets.Add(zOffset);
            Vector3 pos = new Vector3(TerrainManager.Instance.TrackPoints[trackIndex].transform.position.x, segment.transform.position.y, segment.transform.position.z + zOffset);
            pos.y += CalculateTerrainHeightAtPosition(pos);
            GenerateObstacle(prefabIndex, pos);
        }
    }

    public float CalculateTerrainHeightAtPosition(Vector3 worldPos)
    {
        var terrains = GetTerrains();
        int terrainIndex = TerrainUtils.GetTerrainIndex(terrains, worldPos);
        if (terrainIndex >= 0)
            return TerrainUtils.GetTerrainHeight(terrains[terrainIndex], worldPos);
        return 0f;
    }

    public void GenerateObstacle(int prefabIndex, Vector3 worldPos)
    {
        // 从对象池中获取障碍物对象，如果没有则实例化一个新的
        GameObject obstacle = obstaclesPool.Get(prefabIndex, () => {
            if (prefabIndex >= 0 && prefabIndex < GameManager.Instance.Config.ObstaclePrefabs.Length)
                return Instantiate(GameManager.Instance.Config.ObstaclePrefabs[prefabIndex]);
            return null;
        });
        if (obstacle == null)
            return;
        SetObstacleParent(obstacle);
        obstacle.SetActive(true);
        obstacle.transform.position = worldPos;
        obstacles.Add(new GenerationObjectInfo { gameObject = obstacle, prefabIndex = prefabIndex });

    }

    public void SetObstacleParent(GameObject obstacle)
    {
        if (obstacleParent != null)
        {
            obstacle.transform.SetParent(obstacleParent.transform);
            return;
        }
        obstacle.transform.SetParent(transform);
    }
}
