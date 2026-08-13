using Assets.Player;
using Assets.Scripts.Tools;
using Assets.Scripts.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class TerrainManager : MonoBehaviour
{
    private static TerrainManager _instance = null;
    public static TerrainManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<TerrainManager>();
            }
            return _instance;
        }
        private set => _instance = value;
    }

    public List<GameObject> Terrains = new();
    public class RefObj
    {
        private int refCount = 0;
        private GameObject _gameObject = null;
        public GameObject gameObject { get => _gameObject; }
        public Action<GameObject> OnDestroy;
        public RefObj(GameObject gameObject, bool refIncrement = false)
        {
            this._gameObject = gameObject;
            this.refCount = refIncrement ? 1 : 0;
        }
        public RefObj Ref()
        {
            ++refCount;
            return this;
        }
        public void Release()
        {
            --refCount;
            if (refCount == 0)
            {
                Destroy(_gameObject);
            }
        }
    }
    public Dictionary<GameObject, RefObj> TerrainRefCounts = new();
    public System.Action<RefObj> OnTerrainAdded = null;

    // 地形的对象池队列
    private readonly ObjectPool<GameObject> terrainPool = new();

    [SerializeField] private GameObject[] trackPoints = null;
    public GameObject[] TrackPoints { get => trackPoints; }

    private void Awake()
    {
        _instance = this;
    }
    void OnEnable()
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

    public bool ShouldExtend()
    {
        return Terrains.Count == 0 || ExtendTools.ShouldExtend(Terrains.Last(), Terrains.Last().GetComponent<TerrainManagerElement>().TerrainSize);
    }

    public void ExtendOne(GameObject roadSegment)
    {
        if (roadSegment == null)
            return;
        Vector3 pos = Vector3.zero;
        if (Terrains.Count > 0)
        {
            GameObject lastTerrain = Terrains.Last();
            pos = lastTerrain.transform.position;
            pos.z = lastTerrain.transform.position.z + lastTerrain.GetComponent<TerrainManagerElement>().TerrainSize.z;
        }
        GameObject[] terrainPrefabs = GameManager.Instance.Config.TerrainPrefabs;
        if (terrainPrefabs == null || terrainPrefabs.Length == 0)
        {
            Debug.LogError("No terrain prefabs configured!");
            return;
        }
        GameObject terrainPrefab = terrainPrefabs[Random.Range(0, terrainPrefabs.Length)];
        GameObject newTerrain;
        newTerrain = terrainPool.Get(() => Instantiate(terrainPrefab, pos, Quaternion.identity));
        newTerrain.SetActive(true);
        SetTerrainParent(newTerrain);
        newTerrain.transform.position = pos;
        Terrains.Add(newTerrain);
        RefObj refObj = new RefObj(newTerrain, true);
        refObj.OnDestroy += (terrain) => terrainPool.Return(terrain);
        TerrainRefCounts.Add(newTerrain, refObj);
        OnTerrainAdded?.Invoke(refObj);
    }

    private void SetTerrainParent(GameObject terrain)
    {
        terrain.transform.parent = transform;
    }

    public bool ShouldRemove()
    {
        // 玩家在地形前方的距离超出指定距离
        return Terrains.Count > 0 && ExtendTools.ShouldRemove(Terrains.First(), Terrains.First().GetComponent<TerrainManagerElement>().TerrainSize);
    }

    public void RemoveOne()
    {
        if (Terrains.Count == 0)
            return;
        GameObject terrain = Terrains[0];
        if (TerrainRefCounts.TryGetValue(terrain, out RefObj refObj))
        {
            refObj.Release();
        }
        Terrains.RemoveAt(0);
        terrain.SetActive(false);
    }
}
