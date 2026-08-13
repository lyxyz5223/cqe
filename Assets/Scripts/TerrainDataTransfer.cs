using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TerrainDataTransfer : EditorWindow
{

    TerrainData terrainDataFrom;
    TerrainData terrainDataTo;
    bool isJustCopyData;

    [MenuItem("Custom/TerrainDataTransfer")]
    private static void ShowWindow()
    {
        var window = GetWindow<TerrainDataTransfer>();
        window.titleContent = new GUIContent("TerrainDataTransfer");
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("原始地形数据");
        terrainDataFrom = (TerrainData)EditorGUILayout.ObjectField(terrainDataFrom, typeof(TerrainData), true);

        GUILayout.Label("目标地形数据");
        terrainDataTo = (TerrainData)EditorGUILayout.ObjectField(terrainDataTo, typeof(TerrainData), true);

        isJustCopyData = GUILayout.Toggle(isJustCopyData, "仅复制地形数据，不创建新地形");

        if (GUILayout.Button("复制terrain"))
        {
            TransferData(terrainDataFrom, terrainDataTo);
        }
    }

    private void TransferData(TerrainData dataFrom, TerrainData dataTo)
    {
        //dataTo = new TerrainData();  会导致重定向，从而找不到本尊
        ///复制参数
        dataTo.heightmapResolution = dataFrom.heightmapResolution;
        dataTo.size = dataFrom.size;
        dataTo.wavingGrassAmount = dataFrom.wavingGrassAmount;
        dataTo.wavingGrassSpeed = dataFrom.wavingGrassSpeed;
        dataTo.wavingGrassStrength = dataFrom.wavingGrassStrength;
        dataTo.wavingGrassTint = dataFrom.wavingGrassTint;
        dataTo.detailPrototypes = dataFrom.detailPrototypes;
        dataTo.treeInstances = dataFrom.treeInstances;
        dataTo.treePrototypes = dataFrom.treePrototypes;
        dataTo.alphamapResolution = dataFrom.alphamapResolution;
        dataTo.baseMapResolution = dataFrom.baseMapResolution;
        dataTo.splatPrototypes = dataFrom.splatPrototypes;

        float[,] heights = dataFrom.GetHeights(0, 0, dataFrom.heightmapResolution, dataFrom.heightmapResolution);
        dataTo.SetHeights(0, 0, heights);

        float[,,] alphaMap = dataFrom.GetAlphamaps(0, 0, dataFrom.alphamapWidth, dataFrom.alphamapHeight);
        dataTo.SetAlphamaps(0, 0, alphaMap);

        ///是否只是拷贝数据
        if (isJustCopyData)
            return;

        //在场景中创建地表
        GameObject obj = Terrain.CreateTerrainGameObject(dataTo);
        obj.name = "TerrainNew";
    }
}
