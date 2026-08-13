using Assets.Bonus;
using Assets.Player;
using Assets.Simulations;
using Cinemachine;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
            }
            return instance;
        }
        private set { instance = value; }
    }

    // 该物体用于启动调度器，不能删除
    private static GameObject simulationObj = null;

    private Int64 score = 0; // 得分
    private Int64 distance = 0; // 当前距离
    private Int64 previousDistance = 0; // 上一次更新分数时的距离

    public Int64 Score { get => score; }
    public Int64 Distance { get => distance; }

    public GameConfig Config = null;
    public int CurrentPlayerIndex = 0;
    public CinemachineVirtualCamera cinemachineVirtualCamera;

    public bool GameStarted = false;
    public bool GameOver = false;

    private bool startingGame = false;

    private void OnEnable()
    {
        simulationObj = SimulationObject.Instance.gameObject;
        if (instance != null && instance != this)
        {
            Debug.LogWarning("Multiple instances of GameManager detected.");
            return;
        }
        instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("Multiple instances of GameManager detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        // 更新模型预制体索引
        for (int i = 0; i < Config.PlayerModelPrefabs.Length; i++)
        {
            Config.PlayerModelPrefabs[i].GetComponent<PlayerElement>().PrefabIndex = i;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameStarted && !GameOver)
            UpdateScore();
    }


    public void UpdateScore()
    {
        distance = PlayerController.Instance.GetDistance();
        if (distance > previousDistance) // 确保距离增加了才更新分数
        {
            score += (Int64)((distance - previousDistance) * Config.ScorePerMeter); // 每跑10米增加1分
            previousDistance = distance;
        }
        else
            previousDistance = distance;
    }


    // 玩家获得加分道具时调用
    public void Bonus(BonusItem bonusItem)
    {
        // 根据不同的加分道具类型增加分数
        switch (bonusItem)
        {
            case BonusItem.Coin:
                score += 10; // 金币增加10分
                break;
            default:
                break;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"OnSceneLoaded: {scene.name}");
        if (startingGame)
        {
            simulationObj = SimulationObject.Instance.gameObject;
            StartGame();
            startingGame = false;
        }
    }

    public void SwitchSceneAndStartGame()
    {
        if (startingGame)
        {
            Debug.LogWarning("Game is starting. Ignoring duplicate call.");
            return;
        }
        startingGame = true;
        LoadGameScene();
    }

    public void StartGame()
    {
        score = 0;
        GameOver = false;
        GameStarted = true;
        PlayerController.Instance.SettlementScoreboard.SetActive(false);
        PlayerController.Instance.Run();
        CreateAndPlay("BackgroundMusic");
    }

    public void ExitGame()
    {
        GameStarted = false;
        GameOver = true;
#if UNITY_EDITOR
        if (Application.isEditor)
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
#endif
        Application.Quit();
    }

    public void EndGame()
    {
        GameStarted = false;
        GameOver = true;
        // 设置结算界面分数
        PlayerController.Instance.SettlementScoreboard.GetComponent<SettlementScoreboard>().SetScore(score, distance);
        PlayerController.Instance.SettlementScoreboard.SetActive(true);
    }

    public void RestartGame()
    {
        SwitchSceneAndStartGame();
    }

    public void LoadHomepageScene()
    {
        SceneManager.LoadScene("Homepage", LoadSceneMode.Single);
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    /// <summary>
    /// 创建并播放一个音频源
    /// </summary>
    /// <param name="name">音频名称</param>
    /// <param name="loop">是否循环播放</param>
    /// <param name="destroyOnEnd">播放结束后是否销毁对象</param>
    /// <param name="volume">音量</param>
    /// <returns>返回创建的音频源</returns>
    public AudioSource CreateAndPlay(string name, bool loop = true, bool destroyOnEnd = false, float volume = 1f)
    {
        AudioClip clip = Config.AudioEntries.Get(name);
        if (clip == null)
        {
            Debug.LogError($"Audio clip with name '{name}' not found in GameConfig.");
            return null;
        }
        GameObject audioObj = new GameObject($"Audio_{name}");
        AudioSource audioSource = audioObj.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 0=2D音乐，1=3D音乐
        audioSource.volume = volume;
        audioSource.Play();
        if (!loop && destroyOnEnd)
        {
            Destroy(audioObj, clip.length);
        }
        return audioSource;
    }
}
