using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerElement : MonoBehaviour
{
    [SerializeField] private float health = 100f; // 玩家血量
    [SerializeField] private float runSpeed = 1f; // 跑动的速度，m/s
    [SerializeField] private float sprintSpeed = 1.5f; // 冲刺的速度，m/s
    [SerializeField] private float changeTrackSpeed = 5f; // 改变跑道的速度，m/s
    [Tooltip("玩家模型预制体索引，游戏启动时自动更新")]
    [SerializeField] private int prefabIndex = 0; // 玩家模型预制体索引

    public float Health { get => health; set { health = value; } }
    public float Speed { get => runSpeed; private set { runSpeed = value; } }
    public float SprintSpeed { get => sprintSpeed; private set { sprintSpeed = value; } }
    public float ChangeTrackSpeed { get => changeTrackSpeed; private set { changeTrackSpeed = value; } }
    public int PrefabIndex { get => prefabIndex; set { prefabIndex = value; } }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
