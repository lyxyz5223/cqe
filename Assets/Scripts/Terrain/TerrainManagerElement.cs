using Assets.Scripts.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManagerElement : MonoBehaviour
{
    [Tooltip("地形块大小（x, y高度, z）")]
    public Vector3 TerrainSize = Vector3.zero;
    [Tooltip("最大掉落物生成线数量（一个地形块）")]
    public int MaxDropLinesCount = 0;
    public int MaxObstaclesCount = 0;
    public FloatRange ObstaclesSpacingRange = new(3f, 0f, false, true);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
