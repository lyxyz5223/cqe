using Assets.Editor;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Config")]
public class GameConfig : ScriptableObject
{
    private static GameConfig _instance = null;

    public static GameConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameConfig>();
            }
            return _instance;
        }
    }


    //[SerializeField] private float dropGenerationPlayerDistance = 50f; // 距离玩家小于或等于多少米生成掉落物
    [SerializeField] private float dropGenerationSpacing = 1f; // 生成间隔
    [SerializeField] private float jumpHeight = 1f; // 玩家跳跃高度
    [SerializeField] private float scorePerMeter = 5f; // 每运动1米添加的分数
    [SerializeField] private float playerSpeed = 5f; // 玩家移动速度
    [SerializeField] private float playerInvincibleTime = 2f; // 玩家无敌时间(BlinkingTime闪烁时间)
    [SerializeField] private float playerSlidingTime = 1f; // 玩家下滑时间
    [SerializeField] private float preGeneratedDistance = 50f; // 预生成距离
    [SerializeField] private float postDestroyedDistance = 20f; // 当玩家超过某个物体后的某个距离后开始销毁该物体

    public float DropGenerationSpacing { get => dropGenerationSpacing; private set { dropGenerationSpacing = value; } } // 生成间隔
    public float JumpHeight { get => jumpHeight; private set { jumpHeight = value; } }
    public float ScorePerMeter { get => scorePerMeter; private set { scorePerMeter = value; } }
    public float PlayerSpeed { get => playerSpeed; private set { playerSpeed = value; } }
    public float PlayerInvincibleTime { get => playerInvincibleTime; private set { playerInvincibleTime = value; } }
    public float PlayerSlidingTime { get => playerSlidingTime; private set { playerSlidingTime = value; } }
    public float PreGeneratedDistance { get => preGeneratedDistance; private set { preGeneratedDistance = value; } }
    public float PostDestroyedDistance { get => postDestroyedDistance; private set { postDestroyedDistance = value; } }

    [Serializable]
    public class AudioEntry
    {
        public string key;
        public AudioClip clip;
    }

    // 具体类型定义
    [Serializable]
    public class StringAudioDict : SerializableDictionary<string, AudioClip> { }
    public StringAudioDict AudioEntries = new ();

    public GameObject[] PlayerModelPrefabs = null;
    public GameObject[] RoadSegmentPrefabs = null;
    public GameObject[] TerrainPrefabs = null;
    public GameObject[] DecorationPrefabs = null;
    public GameObject[] DropPrefabs = null;
    public GameObject[] ObstaclePrefabs = null;

    private void Awake()
    {

    }

}
