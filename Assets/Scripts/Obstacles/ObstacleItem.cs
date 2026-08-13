using Assets.Scripts.Player;
using Assets.Scripts.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleItem : MonoBehaviour
{
    public IntRange TrackIndexRange = new(0, -1); // 轨道索引范围
    public List<PlayerOperation> SafeOperations = new();
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    /// <summary>
    /// 检查当前玩家多个操作（如：跑的同时左移）对于该障碍物是否安全
    /// 对于每一个SafeOperations中的操作，当前玩家的操作列表中都必须包含才是安全的
    /// </summary>
    /// <param name="operations">当前玩家的操作列表</param>
    /// <returns></returns>
    public bool IsOperationsSafe(List<PlayerOperation> operations)
    {
        // TODO: 暂时用这个玩家操作安全逻辑，以后再改更好的
        return false;
        if (SafeOperations.Contains(PlayerOperation.All))
            return true;
        foreach (var sop in SafeOperations)
        {
            if (!operations.Contains(sop))
                return false;
        }
        return true;
    }
}
