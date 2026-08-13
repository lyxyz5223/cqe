using Assets.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Tools
{
    public class ExtendTools
    {
        /// <summary>
        /// 判断是否需要生成新的段
        /// 计算轴向为z轴
        /// </summary>
        /// <param name="lastSegment">最后一个段</param>
        /// <param name="size">段的大小</param>
        /// <returns>是否需要扩展新的片段</returns>
        public static bool ShouldExtend(GameObject lastSegment, Vector3 size = default)
        {
            return lastSegment.transform.position.z + size.z - PlayerController.Instance.PlayerTransform.position.z <= Mathf.Abs(GameManager.Instance.Config.PreGeneratedDistance);
        }
        /// <summary>
        /// 判断是否需要生成新的段
        /// 计算轴向为z轴
        /// </summary>
        /// <param name="lastSegmentPos">最后一个段的位置</param>
        /// <param name="size">段的大小</param>
        /// <returns>是否需要扩展新的片段</returns>
        public static bool ShouldExtend(Vector3 lastSegmentPos, Vector3 size = default)
        {
            return lastSegmentPos.z + size.z - PlayerController.Instance.PlayerTransform.position.z <= Mathf.Abs(GameManager.Instance.Config.PreGeneratedDistance);
        }

        /// <summary>
        /// 判断是否需要移除第一个段
        /// 计算轴向为z轴
        /// </summary>
        /// <param name="firstSegment">第一个段</param>
        /// <param name="size">段的大小</param>
        /// <returns>是否需要移除第一个段</returns>
        public static bool ShouldRemove(GameObject firstSegment, Vector3 size = default)
        {
            return PlayerController.Instance.PlayerTransform.position.z - firstSegment.transform.position.z - size.z >= Mathf.Abs(GameManager.Instance.Config.PostDestroyedDistance);
        }
    }
}
