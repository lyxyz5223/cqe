using Assets.Player;
using Assets.Scripts;
using Assets.Scripts.Tools;
using Assets.Scripts.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Splines;
using Random = UnityEngine.Random;

public class DropManager : MonoBehaviour
{
    [SerializeField] private GameObject dropParent = null;

    private Coroutine coroutine = null;
    [SerializeField] private List<GenerationObjectInfo> drops = new();
    private ObjectPool<int/*prefab index*/, GameObject/*wrapper*/> dropsPool = new();
    private ExtendController dropExtendCtrller = null;
    private float prevWaitTime = 0f;
    private List<TerrainManager.RefObj> latestExtendedTerrains = new();

    class ExtendDropCallback : ExtendController.IExtendCallback
    {
        DropManager manager = null;
        public ExtendDropCallback(DropManager manager)
        {
            this.manager = manager;
        }
        public void OnExtend()
        {
            manager.GenerateDrops();
        }
        public void OnRemove(int index, GameObject obj)
        {
            foreach (Transform transform in obj.transform)
            {
                transform.gameObject.SetActive(true);
            }
            obj.SetActive(false);
            manager.dropsPool.Return(manager.drops[index].prefabIndex, obj);
            manager.drops.RemoveAt(index);
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
            dropExtendCtrller.YieldInstruction = new WaitForSeconds(time);
    }
    private void OnEnable()
    {
        if (dropExtendCtrller == null)
        {
            YieldInstruction wait = new WaitForSeconds(PlayerController.Instance.GetWaitTimeCoroutineLoop(ref prevWaitTime));
            dropExtendCtrller = new ExtendController(new ExtendDropCallback(this), wait);
        }
        coroutine ??= StartCoroutine(dropExtendCtrller.CheckAndExtend(drops));
    }

    private void OnDisable()
    {
        StopCoroutine(coroutine);
        coroutine = null;
    }
    /// <summary>
    /// 生成掉落物（金币等）
    /// </summary>
    /// <returns>是否生成了新的掉落物</returns>
    public bool GenerateDrops()
    {
        var latestExtendedSegments = latestExtendedTerrains;
        if (latestExtendedSegments.Count == 0 || GameManager.Instance.Config.DropPrefabs.Length == 0)
            return false;
        while (true) // 每次循环生成一个路段的掉落物，直到最后一个路段上的掉落物已经生成完毕，才停止生成新的掉落物
        {
            if (latestExtendedSegments.Count == 0)
            {
                return true;
            }
            // 改用latestExtendedSegments，因此下面这段代码不再需要
            //if (drops.Count > 0)
            //{
            //    GameObject lastOne = drops.Last().gameObject;
            //    if (ShouldExtend(lastOne))
            //    {
            //        break;
            //    }
            //}
            for (int i = 0; i < latestExtendedSegments.Count; ++i)
            {
                GameObject segment = latestExtendedSegments[i].gameObject;
                latestExtendedSegments[i].Release();
                latestExtendedSegments.RemoveAt(i--);
                if (segment == null)
                    continue;
                // 随机选一个掉落物预制体
                int prefabIndex = Random.Range(0, GameManager.Instance.Config.DropPrefabs.Length);
                GenerateDropsOnSegment(segment, prefabIndex);
            }
        }
    }

    /// <summary>
    /// 历史遗留代码
    /// 随机生成掉落物，50%的概率在地形块上生成掉落物，50%的概率不生成掉落物
    /// </summary>
    /// <param name="segment"></param>
    /// <param name="prefabIndex"></param>
    public void GenerateDropsOnSegmentRandomly(GameObject segment, int prefabIndex)
    {
        // 50%的概率生成掉落物，50%的概率不生成掉落物（即生成一个空的路段）
        bool shouldGenerate = Random.Range(0, 2) == 1 ? true : false;
        // 如果需要不生成
        if (!shouldGenerate)
        {
            Debug.Log($"Skipped generating on segment: {segment.name}");
            // 生成一个空物体
            GenerateDrop(-1, segment.transform.position);
            return;
        }
        GenerateDropsOnSegment(segment, prefabIndex);
    }

    /// <summary>
    /// 在指定的地形块上随机选择并沿着一条曲线生成掉落物
    /// 最大生成线条数由地形块上的TerrainManagerElement组件的MaxDropLinesCount属性决定
    /// 生成线条的曲线由地形块上的CoinGenerationLine组件决定
    /// </summary>
    /// <param name="segment">地形块片段</param>
    /// <param name="prefabIndex">掉落物的预制体索引</param>
    public void GenerateDropsOnSegment(GameObject segment, int prefabIndex)
    {
        CoinGenerationLine[] lines = segment.GetComponentsInChildren<CoinGenerationLine>();
        if (lines.Length == 0)
        {
            Debug.Log($"There is no CoinGenerationLine on road segment: {segment.name}");
            Debug.Log($"Skipped generating on road segment: {segment.name}");
            // 生成一个空物体
            GenerateDrop(-1, segment.transform.position);
            return;
        }
        // 随机生成一定数量的掉落物，数量范围为[0, MaxDropLinesCount]
        var terrainElem = segment.GetComponent<TerrainManagerElement>();
        int dropLineCount = Random.Range(0, terrainElem.MaxDropLinesCount + 1);
        List<FloatRange> usedZOffsetRanges = new();
        for (int j = 0; j < dropLineCount; ++j)
        {
            // 随机选一条曲线
            CoinGenerationLine line = lines[Random.Range(0, lines.Length)];
            Spline spline = line.GetComponent<SplineContainer>().Spline;
            float lineLength = spline.GetLength();
            float zLineLength = SplinesUtils.GetLengthAlongAxis(spline, Axis.Z);
            // 选一个轨道放置曲线
            IntRange trackIndexRange = line.TrackIndexRange.GetAbsoluteRange(TerrainManager.Instance.TrackPoints.Length);
            int trackIndex = Random.Range(trackIndexRange.start, trackIndexRange.end + 1);
            // 生成一个随机的z轴偏移量，范围[ZAxisOffsetRange.x, ZAxisOffsetRange.y]
            FloatRange zOffsetRange = line.ZAxisOffsetRange.GetAbsoluteRange(terrainElem.TerrainSize.z);
            // TODO: 优化随机算法
            float zOffset = 0f;
            bool isInvalidZOffset = true;
            int reTryCounter = 100; // 最多尝试100次重新生成
            while (isInvalidZOffset && reTryCounter >= 0)
            {
                --reTryCounter;
                // 在范围内生成一个随机的z轴偏移量
                zOffset = Random.Range(zOffsetRange.start, zOffsetRange.end);
                isInvalidZOffset = false;
                // 检测zOffset是否在已使用的范围内，如果在，则重新生成
                foreach (var range in usedZOffsetRanges)
                {
                    if (range.start - zLineLength <= zOffset && zOffset <= range.end)
                    {
                        isInvalidZOffset = true;
                        break; // 如果zOffset在已使用的范围内，则重新生成
                    }
                }
                if (isInvalidZOffset)
                {
                    Debug.Log($"DropManager: [{reTryCounter}] zOffset {zOffset} is in used range, retrying...");
                }
            }
            ;
            // 将已使用的zOffset范围加入列表，避免后续生成的掉落物与当前生成的掉落物重叠
            usedZOffsetRanges.Add(new FloatRange(zOffset, zOffset + zLineLength, false, false));
            GenerateDropsAlongLine(spline, lineLength, prefabIndex, trackIndex, zOffset, segment);
        }
    }



    /// <summary>
    /// 根据指定的曲线生成掉落物，掉落物将沿着曲线分布
    /// </summary>
    /// <param name="spline">需要生成掉落物的曲线</param>
    /// <param name="lineLength">曲线的长度（不用spline，因为长度也许在外部计算）</param>
    /// <param name="prefabIndex">掉落物的预制体索引</param>
    /// <param name="trackIndex">轨道索引（对曲线指定）</param>
    /// <param name="zOffset">z轴偏移量</param>
    /// <param name="segment">所在的块</param>
    public void GenerateDropsAlongLine(Spline spline, float lineLength, int prefabIndex, int trackIndex, float zOffset, GameObject segment)
    {
        var trackPoints = TerrainManager.Instance.TrackPoints;
        // 获取该轨道的x坐标
        float trackX = trackPoints[trackIndex < trackPoints.Length ? trackIndex : trackPoints.Length - 1].transform.position.x;

        List<Terrain> terrains = new();
        foreach (var terrainObj in TerrainManager.Instance.Terrains)
        {
            Terrain[] ts = terrainObj.GetComponentsInChildren<Terrain>();
            terrains.AddRange(ts);
        }
        for (float step = 0; step <= lineLength; step += GameManager.Instance.Config.DropGenerationSpacing)
        {
            // 获取该曲线在step位置的世界坐标
            float3 lineStepPosition = SplineUtility.EvaluatePosition(spline, SplineUtility.GetNormalizedInterpolation(spline, step, PathIndexUnit.Distance));
            lineStepPosition.x += trackX; // 将曲线上点的x坐标调整到对应轨道的x坐标
            lineStepPosition.z += segment.transform.position.z + zOffset; // 曲线上点的z坐标移动至当前道路块
            //lineStepPosition.y += road.transform.position.y; // 曲线上点的y坐标移动至当前道路块
            Vector3 wpos = new (lineStepPosition.x, 0f, lineStepPosition.z);
            int terrainIndex = TerrainUtils.GetTerrainIndex(terrains, wpos);
            if( terrainIndex >= 0)
                lineStepPosition.y += TerrainUtils.GetTerrainHeight(terrains[terrainIndex], wpos); // 加上地形采样y坐标

            //if (drops.Count > 0)
            //{
            //    // 在最后一个后面生成新的
            //    z = drops.Last().gameObject.transform.position.z + Mathf.Abs(GameManager.Instance.DropGenerationSpacing);
            //}
            Vector3 pos = new(lineStepPosition.x, lineStepPosition.y, lineStepPosition.z);
            GenerateDrop(prefabIndex, pos);
        }
    }

    /// <summary>
    /// 在指定位置生成一个物体，并添加到掉落物列表中
    /// </summary>
    /// <param name="prefabIndex">掉落物预制体索引，为-1代表生成空物体</param>
    /// <param name="localPos">生成位置</param>
    /// <returns>生成的掉落物对象</returns>
    public void GenerateDrop(int prefabIndex, Vector3 localPos)
    {
        GameObject dropWrapper = dropsPool.Get(prefabIndex, () => 
        {
            GameObject dropWrapper = new("Drop Wrapper");
            SetDropParent(dropWrapper);
            if (prefabIndex >= 0 && prefabIndex < GameManager.Instance.Config.DropPrefabs.Length)
            {
                GameObject newDrop = Instantiate(GameManager.Instance.Config.DropPrefabs[prefabIndex], Vector3.zero, Quaternion.identity);
                newDrop.transform.SetParent(dropWrapper.transform);
                drops.Add(new GenerationObjectInfo { gameObject = dropWrapper, prefabIndex = prefabIndex });
            }
            return dropWrapper;
        });
        dropWrapper.SetActive(true);
        dropWrapper.transform.localPosition = localPos;
        drops.Add(new GenerationObjectInfo { gameObject = dropWrapper, prefabIndex = prefabIndex });
    }

    // 设置掉落物的父物体为Drops
    private void SetDropParent(GameObject drop)
    {
        if (dropParent != null)
        {
            drop.transform.SetParent(dropParent.transform);
            return;
        }
        drop.transform.SetParent(transform);
    }

}
