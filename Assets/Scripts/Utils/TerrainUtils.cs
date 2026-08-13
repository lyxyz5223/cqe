using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    public static class TerrainUtils
    {

        public static bool IsPointInTerrain(Terrain terrain, Vector3 worldPos)
        {
            Vector3 terrainMin = terrain.transform.position;
            Vector3 terrainMax = terrainMin + terrain.terrainData.size;
            return worldPos.x >= terrainMin.x && worldPos.x <= terrainMax.x &&
                   worldPos.z >= terrainMin.z && worldPos.z <= terrainMax.z;
        }


        public static float GetTerrainHeight(Terrain terrain, Vector3 worldPos)
        {
            return terrain.SampleHeight(worldPos);
        }
        /// <summary>
        /// 获取任意世界坐标的地形高度（支持多地形块）
        /// </summary>
        public static float GetMultiTerrainHeight(Vector3 worldPos, Terrain[] terrains)
        {
            foreach (Terrain terrain in terrains)
            {
                // 检查该点是否在当前地形范围内
                if (IsPointInTerrain(terrain, worldPos))
                    // 在地形范围内，直接采样
                    return terrain.SampleHeight(worldPos);
            }
            Debug.LogWarning($"坐标 {worldPos} 不在任何地形范围内");
            return 0f;
        }
        /// <summary>
        /// 获取任意世界坐标的地形高度（支持多地形块）
        /// </summary>
        /// <param name="worldPos"></param>
        /// <param name="terrains"></param>
        /// <param name="preferredTerrain">优先使用该地形块进行采样，如果不在该地块范围则遍历terrains</param>
        public static float GetMultiTerrainHeight(Vector3 worldPos, Terrain[] terrains, Terrain preferredTerrain)
        {
            if (IsPointInTerrain(preferredTerrain, worldPos))
                return preferredTerrain.SampleHeight(worldPos);
            foreach (Terrain terrain in terrains)
            {
                // 检查该点是否在当前地形范围内
                if (IsPointInTerrain(terrain, worldPos))
                {
                    // 在地形范围内，直接采样
                    return terrain.SampleHeight(worldPos);
                }
            }
            Debug.LogWarning($"坐标 {worldPos} 不在任何地形范围内");
            return 0f;
        }

        /// <summary>
        /// 获取某个世界坐标对应的地形索引
        /// </summary>
        /// <param name="terrains"></param>
        /// <param name="worldPos"></param>
        /// <returns>地形在数组中的索引，如果不在任何地形范围内则返回 -1</returns>
        public static int GetTerrainIndex(Terrain[] terrains, Vector3 worldPos)
        {
            for (int i = 0; i < terrains.Length; i++)
            {
                if (IsPointInTerrain(terrains[i], worldPos))
                    return i;
            }
            Debug.LogWarning($"坐标 {worldPos} 不在任何地形范围内");
            return -1;
        }

        public static int GetTerrainIndex(List<Terrain> terrains, Vector3 worldPos)
        {
            for (int i = 0; i < terrains.Count; i++)
            {
                Terrain terrain = terrains[i];
                if (terrain != null && IsPointInTerrain(terrain, worldPos))
                    return i;
            }
            Debug.LogWarning($"坐标 {worldPos} 不在任何地形范围内");
            return -1;
        }


    }
}
